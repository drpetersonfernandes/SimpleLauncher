echo Publishing for x64...
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o ./bin/Release/Publish/win-x64

echo Publishing for x86...
dotnet publish -c Release -r win-x86 --self-contained false -p:PublishSingleFile=true -o ./bin/Release/Publish/win-x86

echo Publishing for ARM64...
dotnet publish -c Release -r win-arm64 --self-contained false -p:PublishSingleFile=true -o ./bin/Release/Publish/win-arm64

echo All builds completed!

pause