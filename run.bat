@echo off
chcp 65001 > nul
echo ==============================================
echo   DANG KHOI DONG HE THONG KITTYPHONE VA CHATBOT
echo ==============================================
echo.
echo [1/2] Dang khoi chay Website KittyPhone (ASP.NET Core)...
start "Website KittyPhone (ASP.NET Core)" cmd /k "dotnet run"

echo [2/2] Dang khoi chay KittyBot AI Microservice (NodeJS - Port 3001)...
start "KittyBot AI Microservice (NodeJS)" cmd /k "cd Chat_bot && npm start"

echo.
echo ==============================================
echo Da khoi dong xong!
echo - Website KittyPhone: https://localhost:7260 (hoac http://localhost:5190)
echo - Chatbot Microservice (NodeJS): http://localhost:3001
echo - Trang test rieng Chatbot: http://localhost:3001/index.html
echo - Chatbot da duoc tich hop truc tiep, chay Visual Studio (F5) hay chay run.bat deu dung duoc 100%!
echo ==============================================
pause

