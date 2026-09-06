@echo off
cd /d %~dp0..\src\Web
echo Frontend at http://localhost:5173  (proxies API to port 5080)
npm.cmd run dev
