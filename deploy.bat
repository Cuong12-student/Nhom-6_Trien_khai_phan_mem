@echo off
echo ========================================================
echo DANG TRIEN KHAI UNG DUNG BANG DOCKER COMPOSE (IaC)
echo ========================================================

:: 1. Dung va xoa container cu neu dang ton tai
docker compose down

:: 2. Tu dong build lai va khoi chay container o che do ngam
docker compose up -d --build

echo ========================================================
echo TRIEN KHAI THANH CONG! UNG DUNG DANG CHAY TAI PORT 8080
echo ========================================================
pause