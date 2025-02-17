#!/bin/bash

# Load environment variables from the .env file in the parent folder
export $(grep -v '^#' ../../../.env | xargs)

# Run SteamCMD with credentials
steamcmd +login "$STEAM_USERNAME" "$STEAM_PASSWORD" +run_app_build "$VDF_PATH_DEMO" +quit

echo "Demo build upload complete!"