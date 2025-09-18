@echo off
echo ===================================
echo  Publishing Simple Launcher
echo ===================================

rmdir /s /q ./bin/Publish 2>nul

echo.
echo Publishing for Windows x64...
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o ./bin/Publish/win-x64
if %errorlevel% neq 0 (
    echo FAILED to publish for x64.
    goto end
)
echo x64 publish complete.

echo.
echo Publishing for Windows ARM64...
dotnet publish -c Release -r win-arm64 --self-contained false -p:PublishSingleFile=true -o ./bin/Publish/win-arm64
if %errorlevel% neq 0 (
    echo FAILED to publish for ARM64.
    goto end
)
echo ARM64 publish complete.

echo.
echo ===================================
echo All builds completed successfully!
echo ===================================
echo.

:end
pause
