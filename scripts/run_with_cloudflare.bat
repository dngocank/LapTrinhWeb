@echo off
chcp 65001 > nul
echo ========================================================
echo   KHỞI CHẠY KITTYPHONE VÀ CLOUDFLARE TUNNEL
echo ========================================================
echo.

cd /d "%~dp0\.."

echo [1/2] Dang khoi chay ung dung ASP.NET Core tren cong 5190...
start "KittyPhone Web" cmd /c "dotnet run --launch-profile http"

timeout /t 5 > nul

echo [2/2] Dang mo Cloudflare Tunnel...
if exist "d:\cloudflared.exe" (
    start "Cloudflare Tunnel" cmd /k "d:\cloudflared.exe tunnel --protocol http2 --edge-ip-version 4 --url http://localhost:5190"
) else if exist "d:\Downloads\BTL_PTTKYC\cloudflared.exe" (
    start "Cloudflare Tunnel" cmd /k "d:\Downloads\BTL_PTTKYC\cloudflared.exe tunnel --protocol http2 --edge-ip-version 4 --url http://localhost:5190"
) else (
    echo Khong tim thay cloudflared.exe!
)

echo.
echo ========================================================
echo He thong da khoi chay xong!
echo Ban co the xem URL Cloudflare trong cua so Tunnel vua bat.
echo ========================================================
pause
