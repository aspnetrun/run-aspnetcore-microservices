@echo off
echo 🚀 Starting .NET Microservices with Observability Stack
echo ======================================================

REM Check if Docker is running
docker info >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ Docker is not running. Please start Docker and try again.
    pause
    exit /b 1
)

echo ✅ Docker is running

REM Navigate to the src directory
cd /d "%~dp0"

echo 📦 Starting all services...
docker-compose up -d

echo ⏳ Waiting for services to be healthy...
timeout /t 30 /nobreak >nul

echo 🔍 Checking service health...
docker-compose ps

echo.
echo 🌐 Observability Services:
echo    • Jaeger (Tracing): http://localhost:16686
echo    • Prometheus (Metrics): http://localhost:9090
echo    • OpenTelemetry Collector: http://localhost:4317

echo.
echo 🏗️  Application Services:
echo    • Catalog API: http://localhost:6000
echo    • Basket API: http://localhost:6001
echo    • Discount gRPC: http://localhost:6002
echo    • Ordering API: http://localhost:6003
echo    • YARP Gateway: http://localhost:6004
echo    • Shopping Web: http://localhost:6005

echo.
echo 📊 Metrics Endpoints:
echo    • Catalog API: http://localhost:6000/metrics
echo    • Basket API: http://localhost:6001/metrics
echo    • Ordering API: http://localhost:6003/metrics
echo    • YARP Gateway: http://localhost:6004/metrics
echo    • Shopping Web: http://localhost:6005/metrics

echo.
echo ✅ All services are starting up!
echo    Check the logs with: docker-compose logs -f
echo    Stop services with: docker-compose down

pause
