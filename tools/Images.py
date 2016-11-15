#!/usr/bin/env python3
import os
import re
import subprocess

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
			name = source.filename[:-4]
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
		print("Rendering {} to {}".format(self.source.filename, os.path.join(self.path, self.name)))
		args = ["inkscape", "-z", "-e",
			os.path.join(self.path, self.name) + ".png",
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

def render_icon(filename, paths):
	source = SourceImage(filename)
	for path in paths:
		target = TargetImage(source, **path)
		target.render()

def main():
	# Check if we are in the right folder
	if not os.path.isfile("tools/Images.py"):
		print("Please call this script from the root directory of the project")
		return

	logo_paths = [{"path": ".", "image_width": 800, "image_height": 800}]
	render_icon("Icons/icon.svg", logo_paths)

if __name__ == "__main__":
	main()
