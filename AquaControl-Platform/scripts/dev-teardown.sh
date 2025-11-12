#!/bin/bash

# AquaControl Platform - Development Teardown Script
set -e

echo "Tearing down AquaControl Platform development environment..."

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

print_status() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

# Stop all services
print_status "Stopping all services..."
docker-compose down || docker compose down

# Remove containers
print_status "Removing containers..."
docker-compose rm -f || docker compose rm -f

# Clean up volumes (optional - comment out to preserve data)
read -p "Do you want to remove all data volumes? This will delete all database data! (y/N): " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    print_warning "Removing all volumes and data..."
    docker-compose down -v || docker compose down -v
    docker volume prune -f
else
    print_status "Keeping data volumes intact"
fi

# Clean up networks
print_status "Cleaning up networks..."
docker network prune -f

# Clean up unused images (optional)
read -p "Do you want to remove unused Docker images? (y/N): " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    print_status "Removing unused images..."
    docker image prune -f
fi

print_status "Teardown complete!"

