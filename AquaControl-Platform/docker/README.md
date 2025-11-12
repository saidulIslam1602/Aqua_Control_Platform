# AquaControl Platform - Docker Setup

This directory contains Docker configuration files for the AquaControl Platform development environment.

## Structure

```
docker/
├── configs/
│   ├── nginx/          # Nginx reverse proxy configuration
│   ├── postgres/       # PostgreSQL configuration (if needed)
│   ├── redis/          # Redis configuration (if needed)
│   ├── prometheus/    # Prometheus metrics collection configuration
│   └── grafana/        # Grafana dashboard configuration
└── scripts/
    ├── init-timescale.sql      # TimescaleDB initialization script
    └── create-hypertables.sql  # Hypertable creation script
```

## Quick Start

1. **Start all services:**
   ```bash
   docker-compose up -d
   ```

2. **View logs:**
   ```bash
   docker-compose logs -f [service-name]
   ```

3. **Stop all services:**
   ```bash
   docker-compose down
   ```

4. **Stop and remove volumes:**
   ```bash
   docker-compose down -v
   ```

## Services

### Application Services
- **backend**: ASP.NET Core API (port 5000)
- **frontend**: Vue.js 3 application (port 5173)

### Database Services
- **timescaledb**: PostgreSQL with TimescaleDB extension (port 5432)
- **redis**: Redis cache and session store (port 6379)

### Message Queue Services
- **zookeeper**: Zookeeper for Kafka coordination (port 2181)
- **kafka**: Kafka event streaming platform (port 9092)

### Development Tools
- **pgadmin**: Database administration (port 8080)
- **kafka-ui**: Kafka management UI (port 8081)
- **redis-commander**: Redis management UI (port 8082)

### Monitoring Services
- **prometheus**: Metrics collection (port 9090)
- **grafana**: Metrics visualization (port 3000)

### Reverse Proxy
- **nginx**: Reverse proxy and load balancer (port 80)

## Access URLs

- Frontend: http://localhost
- Backend API: http://localhost/api
- GraphQL: http://localhost/graphql
- pgAdmin: http://localhost:8080
- Kafka UI: http://localhost:8081
- Redis Commander: http://localhost:8082
- Prometheus: http://localhost:9090
- Grafana: http://localhost:3000

## Default Credentials

- **pgAdmin**: admin@aquacontrol.com / AquaControl123!
- **Grafana**: admin / AquaControl123!
- **PostgreSQL**: aquacontrol / AquaControl123!
- **Redis**: AquaControl123!

## Database Initialization

The TimescaleDB database is automatically initialized with:
- TimescaleDB extension enabled
- Required schemas (aquacontrol, timeseries, analytics)
- Database roles and permissions
- Monitoring functions

Hypertables are created automatically when the application starts and creates the required tables.

## Health Checks

All services include health checks. You can verify service health with:

```bash
docker-compose ps
```

## Troubleshooting

### Services not starting
1. Check logs: `docker-compose logs [service-name]`
2. Verify ports are not in use: `netstat -tuln | grep [port]`
3. Check Docker resources: `docker system df`

### Database connection issues
1. Ensure TimescaleDB is healthy: `docker-compose ps timescaledb`
2. Check connection string in backend environment variables
3. Verify network connectivity: `docker network inspect aquacontrol-network`

### Build issues
1. Clear Docker cache: `docker system prune -a`
2. Rebuild without cache: `docker-compose build --no-cache`
3. Check Dockerfile syntax and paths

## Development Workflow

1. **Start infrastructure services:**
   ```bash
   docker-compose up -d timescaledb redis zookeeper kafka
   ```

2. **Start application services:**
   ```bash
   docker-compose up -d backend frontend
   ```

3. **View application logs:**
   ```bash
   docker-compose logs -f backend frontend
   ```

4. **Run database migrations:**
   ```bash
   docker-compose exec backend dotnet ef database update
   ```

5. **Access database:**
   ```bash
   docker-compose exec timescaledb psql -U aquacontrol -d aquacontrol_dev
   ```

## Production Considerations

This Docker Compose setup is configured for **development only**. For production:

1. Use environment-specific configuration files
2. Implement proper secrets management
3. Configure SSL/TLS certificates
4. Set up proper backup strategies
5. Configure resource limits
6. Use production-grade images
7. Implement proper monitoring and alerting
8. Set up log aggregation
9. Configure network security
10. Use orchestration platforms (Kubernetes) for production

