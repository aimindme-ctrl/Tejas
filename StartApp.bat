@echo off
echo Starting TejasCareConnect...
echo.

echo [1/2] Starting Web API on port 5088...
start "Web API" cmd /k "cd TejasCareConnect.Web && dotnet run --launch-profile http"

echo [2/2] Waiting for API to start...
timeout /t 10 /nobreak > nul

echo Starting MAUI App...
cd TejasCareConnect
dotnet run -f net9.0-windows10.0.19041.0

pause
