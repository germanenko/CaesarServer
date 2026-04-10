#!/bin/bash

set -e

echo "🧹 Cleaning output..."
rm -rf out

echo "⚙️ Publishing .NET app..."
dotnet publish planner-node-service/src/planner_node_service.Api \
  -c Release \
  -o ./out \
  /p:UseAppHost=false \
  /p:DebugType=None \
  /p:DebugSymbols=false

echo "🐳 Building Docker image..."
docker build -t planner-node-service .

echo "✅ Done!"