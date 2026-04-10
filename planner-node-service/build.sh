#!/bin/bash

set -e

echo "🧹 Cleaning output..."
rm -rf out

echo "⚙️ Publishing .NET app..."

dotnet publish src/planner_node_service.Api/planner_node_service.Api.csproj \
  -c Release \
  -o ./out \
  /p:UseAppHost=false \
  /p:DebugType=None \
  /p:DebugSymbols=false

echo "🐳 Building Docker image..."
docker build -t planner-node-service .

echo "✅ Done!"