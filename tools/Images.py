#!/usr/bin/env python3
import multiprocessing
import os
import re
import subprocess
import sys
import threading
from queue import Queue

# If images should only be updated when they are newer
update = True

svg_header = """<?xml version="1.0" encoding="UTF-8"?>
<svg
	xmlns="http://www.w3.org/2000/svg"
	viewBox="0 0 {0} {1}"
	version="1.1"
	width="{0}"
	height="{1}">
"""
svg_end = "</svg>\n"

lock = threading.Lock()
def print_locked(*objects, sep = ' ', end = '\n', file = sys.stdout, flush = False):
	with lock:
		print(*objects, sep = sep, end = end, file = file, flush = flush)

class SourceImage:
	regex = re.compile(r"""
		\s*<\?xml.*?\?>       # Match the xml tag
		\s*<svg.*?viewBox="\s*\d+\s+\d+\s+(?P<width>\d+)\s+(?P<height>\d+)\s*".*?>
		# Match the svg tag and capture the size
		(?P<content>.*)</svg> # Match the content until the end of the file
		""", re.VERBOSE | re.DOTALL)

	def __init__(self, filename):
		with open(filename) as f:
			content = f.read()
		# Extract the header
		match = self.regex.match(content)
		# Parse the svg file partially to get the width and the height
		if not match:
			print_locked("No match found for {} in {}".format(self.regex, content[:min(50, len(content))]))
			raise ValueError("No match found")

		self.filename = filename
		self.stat = os.stat(filename)
		self.width = int(match.group("width"))
		self.height = int(match.group("height"))
		self.content = match.group("content")

class TargetImage:
	def __init__(self, source, path, name = None,
		icon_width = None, icon_height = None,
		image_width = None, image_height = None,
		background = None, ending = ".png"):
		# Set the default values
		if not name:
			# Without .svg
			name = os.path.splitext(os.path.basename(source.filename))[0]
		if not image_width:
			image_width = source.width
		if not image_height:
			image_height = source.height
		if not icon_width:
			icon_width = image_width
		if not icon_height:
			icon_height = image_height

		self.source = source
		self.path = path
		self.name = name
		self.icon_width = icon_width
		self.icon_height = icon_height
		self.image_width = image_width
		self.image_height = image_height
		self.background = background
		self.ending = ending

	"""Assemble the image into a string with the given sizes"""
	def assemble(self):
		content = self.source.content
		# Scale the icon if necessary
		if self.icon_width != self.source.width or self.icon_height != self.source.height:
			content = '<g transform="scale({}, {})">\n{}</g>\n'.format(
				self.icon_width / self.source.width,
				self.icon_height / self.source.height,
				content)
		# Add padding if necessary
		if self.image_width != self.icon_width or self.image_height != self.icon_height:
			content = '<g transform="translate({}, {})">\n{}</g>\n'.format(
				(self.image_width - self.icon_width) / 2,
				(self.image_height - self.icon_height) / 2,
				content)
		return svg_header.format(self.image_width, self.image_height) + content + svg_end

	def render(self):
		# Check if the image is already new enough
		target_filename = os.path.join(self.path, self.name) + self.ending
		if update and os.path.isfile(target_filename):
			stat = os.stat(target_filename)
			if stat.st_mtime > self.source.stat.st_mtime:
				print_locked("Skipping {} to {}".format(self.source.filename, target_filename))
				return

		print_locked("Rendering {} to {}".format(self.source.filename, target_filename))
		# Create the folder if it doesn't exist
		with lock:
			if not os.path.isdir(self.path):
				os.mkdir(self.path)
		args = ["inkscape", "-z", "-e",
			os.path.abspath(target_filename),
			"-w", str(self.image_width),
			"-h", str(self.image_height),
			"/dev/stdin"]
		if self.background:
			args.append("-b")
			args.append("#0fb9de")
		process = subprocess.Popen(args,
			stdin = subprocess.PIPE, stdout = subprocess.DEVNULL,
			stderr = subprocess.DEVNULL)
		# Writes input and waits for the process to exit
		process.communicate(self.assemble().encode("utf-8"))
		if process.returncode != 0:
			raise ValueError("inkscape exited with non zero status")

""" Do work in multiple threads """
def worker():
	while True:
		target = render_queue.get()
		target.render()
		render_queue.task_done()

def render_icon(source, paths):
	if type(source) is str:
		source = SourceImage(source)
	for path in paths:
		target = TargetImage(source, **path)
		render_queue.put(target)

def add_android_paths(source, paths, width = None, height = None, background = None):
	root = "Zeltlager/Zeltlager.Droid/Resources/"
	if not width:
		width = source.width
	if not height:
		height = source.height

	def add(name, factor):
		paths.append({
			"path": root + "drawable-{}dpi".format(name),
			"image_width": width * factor,
			"image_height": height * factor,
			"background": background
		})

	paths.append({
		"path": root + "drawable",
		"image_width": width * 4,
		"image_height": height * 4,
		"background": background
	})
	add("l", 0.75)
	add("m", 1)
	add("h", 1.5)
	add("xh", 2)
	add("xxh", 3)
	add("xxxh", 4)

def add_windows_logo_paths(source, paths, background = None):
	root = "Zeltlager/Zeltlager.Windows/Assets"

	def add(width, height = None):
		if not height:
			height = width
		name = os.path.splitext(os.path.basename(source.filename))[0]
		paths.append({
			"path": root,
			"name": "{}-{}x{}".format(name, width, height),
			"icon_width": min(width, height),
			"icon_height": min(width, height),
			"image_width": width,
			"image_height": height,
			"background": background
		})

	add(30)
	add(50)
	add(126, 126)
	add(150, 150)
	add(620, 300)

def add_ios_logo_paths(source, paths, background = True):
	root = "Zeltlager/Zeltlager.iOS/Assets.xcassets/AppIcons.appiconset"

	def add(name, scales, width, height = None):
		if not height:
			height = width
		for scale in scales:
			if scale == 1:
				image_name = name
			else:
				image_name = "{}@{}x".format(name, scale)
			paths.append({
				"path": root,
				"name": image_name,
				"icon_width": min(width, height) * scale,
				"icon_height": min(width, height) * scale,
				"image_width": width * scale,
				"image_height": height * scale,
				"background": background
			})

	# iPhone, iPad Settings
	add("Icon-Small", [1, 2, 3], 29)
	# iPhone Spotlight, iPad Spotlight iOS 7, 8
	add("Icon-Small-40", [1, 2, 3], 40)
	# iPhone App iOS 5, 6
	add("Icon", [1, 2], 57)
	# iPhone App iOS 7, 8
	add("Icon-60", [2, 3], 60)
	# iPad Spotlight iOS 5, 6
	add("Icon-Small-50", [1, 2], 50)
	# iPad Pro App
	add("iPad-Pro", [2], 83.5)
	# iPad App iOS 5, 6
	add("Icon-72", [1, 2], 72)
	# iPad App iOS 7, 8
	add("Icon-76", [1, 2], 76)

def add_ios_logo_itunes_paths(source, paths, background = True):
	root = "Zeltlager/Zeltlager.iOS"

	def add(name, scales, width, image_width, image_height = None):
		if not image_height:
			image_height = image_width
		for scale in scales:
			if scale == 1:
				image_name = name
			else:
				image_name = "{}@{}x".format(name, scale)
			paths.append({
				"path": root,
				"name": image_name,
				"icon_width": width * scale,
				"icon_height": width * scale,
				"image_width": image_width * scale,
				"image_height": image_height * scale,
				"background": background,
				"ending": ""
			})

	# iTunes Artwork
	add("iTunesArtwork", [1, 2], 128, 512)

def add_ios_launchimage_paths(source, paths, background = True):
	root = "Zeltlager/Zeltlager.iOS/Assets.xcassets/LaunchImage.launchimage"

	def add(name, width, image_width, image_height = None):
		if not image_height:
			image_height = image_width
		image_name = "{}{}x{}".format(name, image_width, image_height)
		paths.append({
			"path": root,
			"name": image_name,
			"icon_width": width,
			"icon_height": width,
			"image_width": image_width,
			"image_height": image_height,
			"background": background
		})

	# iPhone Portrait iOS 5, 6
	add("Launch", 128, 320, 480)
	add("Launch", 256, 640, 960)
	add("Launch", 256, 640, 1136)
	# iPhone Portrait iOS 8, 9
	add("Launch", 300, 750, 1334)
	add("Launch", 600, 1242, 2208)
	# iPhone Landscape
	add("Launch", 600, 2208, 1242)
	# iPad Portrait
	add("Launch", 320, 768, 1024)
	add("Launch", 640, 1536, 2048)
	# iPad Landscape
	add("Launch", 320, 1024, 768)
	add("Launch", 640, 2048, 1536)
	# iPad Portrait without status bar
	add("Launch", 320, 768, 1004)
	add("Launch", 640, 1536, 2008)
	# iPad Landscape without status bar
	add("Launch", 640, 2048, 1496)
	# AppleTV
	add("Launch", 640, 1920, 1080)

def add_ios_paths(source, paths, width = None, height = None, background = False):
	root = "Zeltlager/Zeltlager.iOS/Resources"
	if not width:
		width = source.width
	if not height:
		height = source.height
	name = os.path.splitext(os.path.basename(source.filename))[0]

	def add(scales, width, height = None):
		if not height:
			height = width
		for scale in scales:
			if scale == 1:
				image_name = name
			else:
				image_name = "{}@{}x".format(name, scale)
			paths.append({
				"path": root,
				"name": image_name,
				"icon_width": min(width, height) * scale,
				"icon_height": min(width, height) * scale,
				"image_width": width * scale,
				"image_height": height * scale,
				"background": background
			})
	add([1, 2, 3], width, height)

def main():
	global render_queue
	# Check if we are in the right folder
	if not os.path.isfile("tools/Images.py"):
		print_locked("Please call this script from the root directory of the project")
		return

	# Create the render queue and worker threads
	render_queue = Queue()
	for i in range(multiprocessing.cpu_count()):
		t = threading.Thread(target = worker)
		# The thread will be killed if the main thread exits
		t.daemon = True
		t.start()

	logo = SourceImage("Icons/icon.svg")
	logo_paths = []
	# 48px for the application icon
	add_android_paths(logo, logo_paths, 48, 48)
	add_windows_logo_paths(logo, logo_paths)
	add_ios_logo_paths(logo, logo_paths)
	add_ios_logo_itunes_paths(logo, logo_paths)
	add_ios_launchimage_paths(logo, logo_paths)
	render_icon(logo, logo_paths)

	# Convert all icons
	icon_paths = []
	# 24px for system icons
	add_android_paths(None, icon_paths, 24, 24)
	icon_dir = "Icons/UIsvg"
	for icon in os.listdir(icon_dir):
		# Ignore non-images
		if icon == ".DS_Store":
			continue
		icon_path = os.path.join(icon_dir, icon)
		icon = SourceImage(icon_path)
		# Clone paths
		paths = icon_paths[:]
		add_ios_paths(icon, paths, 24, 24)
		render_icon(icon, paths)
	# Render tent icon
	icon = SourceImage("Icons/icon.svg")
	paths = icon_paths[:]
	add_ios_paths(icon, paths, 48, 48)
	render_icon(icon, paths)

	# Wait until all icons are rendered
	render_queue.join()

if __name__ == "__main__":
	main()
