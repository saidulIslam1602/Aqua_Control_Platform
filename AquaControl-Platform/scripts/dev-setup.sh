#!/bin/bash

# AquaControl Platform - Development Setup Script
set -e

echo "Setting up AquaControl Platform for development..."

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
    print_error "Docker is not installed. Please install Docker first."
    exit 1
fi

# Check if Docker Compose is installed
if ! command -v docker-compose &> /dev/null && ! docker compose version &> /dev/null; then
    print_error "Docker Compose is not installed. Please install Docker Compose first."
    exit 1
fi

# Create necessary directories
print_status "Creating necessary directories..."
mkdir -p docker/{configs/{nginx,postgres,redis,prometheus,grafana},scripts}
mkdir -p logs/{backend,frontend,nginx}

# Set permissions
print_status "Setting permissions..."
chmod +x scripts/*.sh 2>/dev/null || true

# Pull required Docker images
print_status "Pulling Docker images..."
docker-compose pull || docker compose pull

# Build custom images
print_status "Building application images..."
docker-compose build --no-cache || docker compose build --no-cache

# Start infrastructure services first
print_status "Starting infrastructure services..."
docker-compose up -d timescaledb redis zookeeper kafka || docker compose up -d timescaledb redis zookeeper kafka

# Wait for services to be ready
print_status "Waiting for infrastructure services to be ready..."
sleep 30

# Check service health
print_status "Checking service health..."
docker-compose ps || docker compose ps

# Start application services
print_status "Starting application services..."
docker-compose up -d backend frontend || docker compose up -d backend frontend

# Wait for application to be ready
print_status "Waiting for application services to be ready..."
sleep 60

# Start monitoring and management tools
print_status "Starting monitoring and management tools..."
docker-compose up -d pgadmin kafka-ui redis-commander prometheus grafana nginx || docker compose up -d pgadmin kafka-ui redis-commander prometheus grafana nginx

# Display service URLs
print_status "Setup complete! Services are available at:"
echo ""
echo "Frontend Application:     http://localhost"
echo "Backend API:              http://localhost/api"
echo "GraphQL Playground:       http://localhost/graphql"
echo "Database Admin (pgAdmin): http://localhost:8080"
echo "Kafka UI:                 http://localhost:8081"
echo "Redis Commander:          http://localhost:8082"
echo "Prometheus:               http://localhost:9090"
echo "Grafana:                  http://localhost:3000"
echo ""
echo "Default Credentials:"
echo "   pgAdmin:    admin@aquacontrol.com / AquaControl123!"
echo "   Grafana:    admin / AquaControl123!"
echo ""
echo "To view logs: docker-compose logs -f [service-name]"
echo "To stop all services: docker-compose down"
echo "To restart a service: docker-compose restart [service-name]"

# Check if all services are running
print_status "Performing health checks..."
sleep 10

# Test backend health
if curl -f http://localhost/health &> /dev/null; then
    print_status "Backend is healthy"
else
    print_warning "Backend health check failed"
fi

# Test frontend
if curl -f http://localhost &> /dev/null; then
    print_status "Frontend is accessible"
else
    print_warning "Frontend accessibility check failed"
fi

print_status "AquaControl Platform is ready for development!"

