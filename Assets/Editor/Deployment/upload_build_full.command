#!/bin/bash

DIR="$(cd "$(dirname "$0")" && pwd)"
cd "$DIR"

if [ -f ../../../.env ]; then
    export $(grep -v '^#' ../../../.env | xargs)
else
    echo ".env file not found!"
    exit 1
fi

VDF_PATH_ABS="$DIR/$VDF_PATH_FULL"

# Run SteamCMD with credentials
steamcmd +login "$STEAM_USERNAME" "$STEAM_PASSWORD" +run_app_build "$VDF_PATH_ABS" +quit

echo "Full build upload complete!"