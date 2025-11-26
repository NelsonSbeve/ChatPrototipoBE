cd ChatPrototipo.API
dotnet publish -c Release -r linux-x64 --self-contained false -o ./publish
dotnet ./publish/ChatPrototipo.API.dll