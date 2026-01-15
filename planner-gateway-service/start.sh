rm -rf ./publish
dotnet publish -c Release -o ./publish
dotnet ./publish/planner_gateway_service.dll --urls "http://localhost:8080"