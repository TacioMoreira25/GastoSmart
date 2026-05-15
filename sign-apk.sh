#!/bin/bash
# Script to sign APK files after build

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
KEYSTORE_PATH="$SCRIPT_DIR/.keystore/debug.keystore"
KEYSTORE_PASS="android"
KEY_ALIAS="debug"
KEY_PASS="android"
APKSIGNER="/home/tacio/Android/Sdk/build-tools/37.0.0/apksigner"

echo "[APK Signer] Working directory: $(pwd)"
echo "[APK Signer] Script directory: $SCRIPT_DIR"
echo "[APK Signer] Keystore: $KEYSTORE_PATH"

# Find all unsigned APKs in the build output (both Debug and Release)
found_count=0
for apk_dir in "$SCRIPT_DIR"/GastoSmart.App/bin/Debug/net10.0-android "$SCRIPT_DIR"/GastoSmart.App/bin/Release/net10.0-android; do
    if [ -d "$apk_dir" ]; then
        for apk in "$apk_dir"/*.apk; do
            if [ -f "$apk" ] && ! [[ "$apk" == *"-Signed"* ]]; then
                found_count=$((found_count + 1))
                echo "[APK Signer] Signing APK: $apk"
                
                if "$APKSIGNER" sign \
                    --ks "$KEYSTORE_PATH" \
                    --ks-pass "pass:$KEYSTORE_PASS" \
                    --ks-key-alias "$KEY_ALIAS" \
                    --key-pass "pass:$KEY_PASS" \
                    "$apk" 2>&1; then
                    echo "[APK Signer] ✓ Successfully signed: $(basename "$apk")"
                else
                    echo "[APK Signer] ✗ Failed to sign: $apk" >&2
                    exit 1
                fi
            fi
        done
    fi
done

if [ $found_count -eq 0 ]; then
    echo "[APK Signer] No unsigned APKs found to sign"
else
    echo "[APK Signer] Successfully signed $found_count APK(s)"
fi

