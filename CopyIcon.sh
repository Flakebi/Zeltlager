#!/usr/bin/env bash
if [ ! -f CopyIcon.sh ]; then
	echo "Please run this script in the root directory of the project."
	exit 1
fi

cp icon.png Zeltlager/Zeltlager.Droid/Resources/drawable/
cp icon.png Zeltlager/Zeltlager.Droid/Resources/drawable-hdpi
cp icon.png Zeltlager/Zeltlager.Droid/Resources/drawable-xhdpi/
cp icon.png Zeltlager/Zeltlager.Droid/Resources/drawable-xxhdpi/

cp icon.png Zeltlager/Zeltlager.Windows/Assets/

echo "Rememberto update Zeltlager/Zeltlager.Windows/Assets/iconxxx.png"
