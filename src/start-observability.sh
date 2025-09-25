#!/bin/bash

echo "🚀 Starting .NET Microservices with Observability Stack"
echo "======================================================"

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "❌ Docker is not running. Please start Docker and try again."
    exit 1
fi

echo "✅ Docker is running"

# Navigate to the src directory
cd "$(dirname "$0")"

echo "📦 Starting all services..."
docker-compose up -d

echo "⏳ Waiting for services to be healthy..."
sleep 30

echo "🔍 Checking service health..."
docker-compose ps

echo ""
echo "🌐 Observability Services:"
echo "   • Jaeger (Tracing): http://localhost:16686"
echo "   • Prometheus (Metrics): http://localhost:9090"
echo "   • OpenTelemetry Collector: http://localhost:4317"

echo ""
echo "🏗️  Application Services:"
echo "   • Catalog API: http://localhost:6000"
echo "   • Basket API: http://localhost:6001"
echo "   • Discount gRPC: http://localhost:6002"
echo "   • Ordering API: http://localhost:6003"
echo "   • YARP Gateway: http://localhost:6004"
echo "   • Shopping Web: http://localhost:6005"

echo ""
echo "📊 Metrics Endpoints:"
echo "   • Catalog API: http://localhost:6000/metrics"
echo "   • Basket API: http://localhost:6001/metrics"
echo "   • Ordering API: http://localhost:6003/metrics"
echo "   • YARP Gateway: http://localhost:6004/metrics"
echo "   • Shopping Web: http://localhost:6005/metrics"

echo ""
echo "✅ All services are starting up!"
echo "   Check the logs with: docker-compose logs -f"
echo "   Stop services with: docker-compose down"
