#!/bin/bash

# AquaControl Platform - Development Startup Script
# This script sets up and starts the entire development environment

set -e

echo "🚀 Starting AquaControl Platform Development Environment..."

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    print_error "Docker is not running. Please start Docker and try again."
    exit 1
fi

# Check if Docker Compose is available
if ! command -v docker-compose > /dev/null 2>&1; then
    print_error "Docker Compose is not installed. Please install Docker Compose and try again."
    exit 1
fi

# Navigate to project root
cd "$(dirname "$0")/.."

print_status "Cleaning up any existing containers..."
docker-compose -f docker-compose.dev.yml down --remove-orphans

print_status "Building and starting services..."
docker-compose -f docker-compose.dev.yml up --build -d

print_status "Waiting for services to be healthy..."

# Wait for database
print_status "Waiting for TimescaleDB to be ready..."
timeout=60
counter=0
while ! docker-compose -f docker-compose.dev.yml exec -T timescaledb pg_isready -U aquacontrol -d aquacontrol_dev > /dev/null 2>&1; do
    if [ $counter -ge $timeout ]; then
        print_error "TimescaleDB failed to start within $timeout seconds"
        docker-compose -f docker-compose.dev.yml logs timescaledb
        exit 1
    fi
    sleep 2
    counter=$((counter + 2))
    echo -n "."
done
print_success "TimescaleDB is ready!"

# Wait for Redis
print_status "Waiting for Redis to be ready..."
counter=0
while ! docker-compose -f docker-compose.dev.yml exec -T redis redis-cli ping > /dev/null 2>&1; do
    if [ $counter -ge $timeout ]; then
        print_error "Redis failed to start within $timeout seconds"
        docker-compose -f docker-compose.dev.yml logs redis
        exit 1
    fi
    sleep 2
    counter=$((counter + 2))
    echo -n "."
done
print_success "Redis is ready!"

# Wait for backend API
print_status "Waiting for Backend API to be ready..."
counter=0
while ! curl -f http://localhost:5000/health > /dev/null 2>&1; do
    if [ $counter -ge 120 ]; then
        print_error "Backend API failed to start within 120 seconds"
        docker-compose -f docker-compose.dev.yml logs backend
        exit 1
    fi
    sleep 3
    counter=$((counter + 3))
    echo -n "."
done
print_success "Backend API is ready!"

# Wait for frontend
print_status "Waiting for Frontend to be ready..."
counter=0
while ! curl -f http://localhost:5173 > /dev/null 2>&1; do
    if [ $counter -ge 90 ]; then
        print_error "Frontend failed to start within 90 seconds"
        docker-compose -f docker-compose.dev.yml logs frontend
        exit 1
    fi
    sleep 3
    counter=$((counter + 3))
    echo -n "."
done
print_success "Frontend is ready!"

print_success "🎉 AquaControl Platform is now running!"
echo ""
echo "📱 Application URLs:"
echo "   Frontend:  http://localhost:5173"
echo "   Backend:   http://localhost:5000"
echo "   API Docs:  http://localhost:5000/swagger"
echo "   Health:    http://localhost:5000/health"
echo ""
echo "🗄️  Database URLs:"
echo "   PostgreSQL: localhost:5432"
echo "   Redis:      localhost:6379"
echo ""
echo "📊 Useful Commands:"
echo "   View logs:     docker-compose -f docker-compose.dev.yml logs -f [service]"
echo "   Stop all:      docker-compose -f docker-compose.dev.yml down"
echo "   Restart:       docker-compose -f docker-compose.dev.yml restart [service]"
echo "   Shell access:  docker-compose -f docker-compose.dev.yml exec [service] bash"
echo ""
echo "🔧 Development Features:"
echo "   ✅ Hot reload enabled for both frontend and backend"
echo "   ✅ Source code mounted as volumes for live editing"
echo "   ✅ Debug logging enabled"
echo "   ✅ Development CORS policies active"
echo ""

# Show running containers
print_status "Running containers:"
docker-compose -f docker-compose.dev.yml ps

echo ""
print_success "Setup complete! Happy coding! 🚀"
