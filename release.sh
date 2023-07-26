#!/bin/bash -x
VERSION=$(jq -r .Version Info.json)
BUILD="bin/Debug"
TMPBASE="copy"
TMPOUT="$TMPBASE/DriverAssist"
RELEASE="dist"

rm -rf  "./$TMPBASE"
mkdir -p "$TMPOUT"

cp "$BUILD/DriverAssist.dll" "$TMPOUT"
cp "Info.json" "$TMPOUT/DriverAssist"

rm -f "$RELEASE/DriverAssist-v${VERSION}.zip"
7z a "$RELEASE/DriverAssist-v${VERSION}.zip" "./$TMPBASE/*"
