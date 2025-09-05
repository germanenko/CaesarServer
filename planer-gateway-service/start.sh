rm -rf ./publish
dotnet publish -c Release -o ./publish
dotnet ./publish/planer-gateway-service.dll --urls "http://localhost:8080"