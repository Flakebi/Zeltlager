#!/usr/bin/env python3
import os
import re
import subprocess

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
			print("No match found for {} in {}".format(self.regex, content[:min(50, len(content))]))
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
		background = None):
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
		target_filename = os.path.join(self.path, self.name) + ".png"
		if update and os.path.isfile(target_filename):
			stat = os.stat(target_filename)
			if stat.st_mtime > self.source.stat.st_mtime:
				print("Skipping {} to {}".format(self.source.filename, target_filename))
				return

		print("Rendering {} to {}".format(self.source.filename, target_filename))
		# Create the folder if it doesn't exist
		if not os.path.isdir(self.path):
			os.mkdir(self.path)
		args = ["inkscape", "-z", "-e",
			target_filename,
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

def render_icon(source, paths):
	if type(source) is str:
		source = SourceImage(source)
	for path in paths:
		target = TargetImage(source, **path)
		target.render()

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
	
def add_windows_logo_paths(source, paths, width = None, height = None, background = None):
	root = "Zeltlager/Zeltlager.Windows/Assets"
	if not width:
		width = source.width
	if not height:
		height = source.height

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

def main():
	# Check if we are in the right folder
	if not os.path.isfile("tools/Images.py"):
		print("Please call this script from the root directory of the project")
		return

	logo = SourceImage("Icons/icon.svg")
	logo_paths = []
	# 48px for the application icon
	add_android_paths(logo, logo_paths, 48, 48)
	add_windows_logo_paths(logo, logo_paths, 48, 48)
	render_icon(logo, logo_paths)

	# Convert all icons
	icon_paths = []
	# 24px for system icons
	#add_android_paths(None, paths, 24, 24)
	icon_dir = "Icons/UIsvg"
	for icon in os.listdir(icon_dir):
		icon_path = os.path.join(icon_dir, icon)
		render_icon(icon_path, icon_paths)

if __name__ == "__main__":
	main()
