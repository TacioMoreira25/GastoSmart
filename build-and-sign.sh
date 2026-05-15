#!/bin/bash
# build-and-sign.sh - Build e assina o APK em uma única linha

set -e

CONFIG="${1:-Debug}"
PROJECT_PATH="GastoSmart.App/GastoSmart.App.csproj"

echo "📦 Building GastoSmart.App ($CONFIG configuration)..."
dotnet build "$PROJECT_PATH" -c "$CONFIG"

echo "✍️  Signing APK..."
bash sign-apk.sh

echo "✅ Build and signing complete!"
echo "📱 APK location:"
echo "   GastoSmart.App/bin/$CONFIG/net10.0-android/com.companyname.gastosmart.android.apk"

