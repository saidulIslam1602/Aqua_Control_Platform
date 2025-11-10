# Phase 2: Complete Docker Compose Setup

## 🎯 Overview
This phase provides a complete Docker Compose setup for local development, testing, and demonstration of the AquaControl platform with all services running together.

---

## 📁 Docker Structure

```bash
# Create Docker-related directories
cd /home/saidul/Desktop/Portfolio/AquaControl-Platform
mkdir -p docker/{development,production,scripts}
mkdir -p docker/configs/{nginx,postgres,redis}
```

---

## 🔧 Step 1: Main Docker Compose File

### File 1: Development Docker Compose
**File:** `docker-compose.yml`

```yaml
version: '3.8'

services:
  # ===========================================
  # Database Services
  # ===========================================
  
  # TimescaleDB - Primary Database
  timescaledb:
    image: timescale/timescaledb:latest-pg15
    container_name: aquacontrol-timescaledb
    restart: unless-stopped
    environment:
      POSTGRES_DB: aquacontrol_dev
      POSTGRES_USER: aquacontrol
      POSTGRES_PASSWORD: AquaControl123!
      POSTGRES_INITDB_ARGS: "--auth-host=scram-sha-256"
      # Performance tuning
      POSTGRES_SHARED_BUFFERS: 256MB
      POSTGRES_EFFECTIVE_CACHE_SIZE: 1GB
      POSTGRES_WORK_MEM: 16MB
      POSTGRES_MAINTENANCE_WORK_MEM: 128MB
    ports:
      - "5432:5432"
    volumes:
      - timescale_data:/var/lib/postgresql/data
      - ./docker/scripts/init-timescale.sql:/docker-entrypoint-initdb.d/01-init-timescale.sql
      - ./docker/scripts/create-hypertables.sql:/docker-entrypoint-initdb.d/02-create-hypertables.sql
      - ./docker/configs/postgres/postgresql.conf:/etc/postgresql/postgresql.conf
    networks:
      - aquacontrol-network
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U aquacontrol -d aquacontrol_dev"]
      interval: 10s
      timeout: 5s
      retries: 5
      start_period: 30s

  # Redis - Caching and Session Store
  redis:
    image: redis:7-alpine
    container_name: aquacontrol-redis
    restart: unless-stopped
    command: redis-server /usr/local/etc/redis/redis.conf
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data
      - ./docker/configs/redis/redis.conf:/usr/local/etc/redis/redis.conf
    networks:
      - aquacontrol-network
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 10s
      timeout: 3s
      retries: 5

  # ===========================================
  # Message Queue Services
  # ===========================================
  
  # Zookeeper for Kafka
  zookeeper:
    image: confluentinc/cp-zookeeper:7.5.0
    container_name: aquacontrol-zookeeper
    restart: unless-stopped
    environment:
      ZOOKEEPER_CLIENT_PORT: 2181
      ZOOKEEPER_TICK_TIME: 2000
      ZOOKEEPER_LOG4J_ROOT_LOGLEVEL: WARN
    volumes:
      - zookeeper_data:/var/lib/zookeeper/data
      - zookeeper_logs:/var/lib/zookeeper/log
    networks:
      - aquacontrol-network
    healthcheck:
      test: ["CMD", "bash", "-c", "echo 'ruok' | nc localhost 2181"]
      interval: 10s
      timeout: 5s
      retries: 5

  # Kafka - Event Streaming
  kafka:
    image: confluentinc/cp-kafka:7.5.0
    container_name: aquacontrol-kafka
    restart: unless-stopped
    depends_on:
      zookeeper:
        condition: service_healthy
    environment:
      KAFKA_BROKER_ID: 1
      KAFKA_ZOOKEEPER_CONNECT: zookeeper:2181
      KAFKA_LISTENER_SECURITY_PROTOCOL_MAP: PLAINTEXT:PLAINTEXT,PLAINTEXT_HOST:PLAINTEXT
      KAFKA_ADVERTISED_LISTENERS: PLAINTEXT://kafka:29092,PLAINTEXT_HOST://localhost:9092
      KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR: 1
      KAFKA_GROUP_INITIAL_REBALANCE_DELAY_MS: 0
      KAFKA_AUTO_CREATE_TOPICS_ENABLE: 'true'
      KAFKA_NUM_PARTITIONS: 3
      KAFKA_DEFAULT_REPLICATION_FACTOR: 1
      # Performance settings
      KAFKA_LOG_RETENTION_HOURS: 168
      KAFKA_LOG_RETENTION_BYTES: 1073741824
      KAFKA_LOG_SEGMENT_BYTES: 1073741824
    ports:
      - "9092:9092"
    volumes:
      - kafka_data:/var/lib/kafka/data
    networks:
      - aquacontrol-network
    healthcheck:
      test: ["CMD", "kafka-broker-api-versions", "--bootstrap-server", "localhost:9092"]
      interval: 30s
      timeout: 10s
      retries: 5

  # ===========================================
  # Application Services
  # ===========================================
  
  # Backend API
  backend:
    build:
      context: ./backend
      dockerfile: Dockerfile.dev
      args:
        - BUILDKIT_INLINE_CACHE=1
    container_name: aquacontrol-backend
    restart: unless-stopped
    environment:
      # ASP.NET Core settings
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:5000
      - ASPNETCORE_LOGGING__LOGLEVEL__DEFAULT=Information
      
      # Database connections
      - ConnectionStrings__DefaultConnection=Host=timescaledb;Port=5432;Database=aquacontrol_dev;Username=aquacontrol;Password=AquaControl123!;Include Error Detail=true
      - ConnectionStrings__Redis=redis:6379,password=AquaControl123!
      
      # JWT settings
      - JwtSettings__SecretKey=your-super-secret-jwt-key-that-is-at-least-32-characters-long-for-development
      - JwtSettings__Issuer=AquaControl.API
      - JwtSettings__Audience=AquaControl.Client
      - JwtSettings__ExpirationMinutes=60
      
      # Kafka settings
      - Kafka__BootstrapServers=kafka:29092
      - Kafka__GroupId=aquacontrol-backend
      
      # Feature flags
      - Features__EnableRealTimeUpdates=true
      - Features__EnableAnalytics=true
      - Features__EnableAlerts=true
    ports:
      - "5000:5000"
    depends_on:
      timescaledb:
        condition: service_healthy
      redis:
        condition: service_healthy
      kafka:
        condition: service_healthy
    networks:
      - aquacontrol-network
    volumes:
      - ./backend:/app/source
      - backend_logs:/app/logs
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5000/health"]
      interval: 30s
      timeout: 10s
      retries: 5
      start_period: 60s

  # Frontend Application
  frontend:
    build:
      context: ./frontend
      dockerfile: Dockerfile.dev
      args:
        - BUILDKIT_INLINE_CACHE=1
    container_name: aquacontrol-frontend
    restart: unless-stopped
    environment:
      # Vite settings
      - VITE_API_BASE_URL=http://localhost:5000
      - VITE_GRAPHQL_ENDPOINT=http://localhost:5000/graphql
      - VITE_SIGNALR_HUB_URL=http://localhost:5000/hubs
      - VITE_APP_NAME=AquaControl Platform
      - VITE_APP_VERSION=2.0.0
      
      # Feature flags
      - VITE_ENABLE_ANALYTICS=true
      - VITE_ENABLE_REAL_TIME=true
      - VITE_ENABLE_NOTIFICATIONS=true
    ports:
      - "5173:5173"
    depends_on:
      backend:
        condition: service_healthy
    networks:
      - aquacontrol-network
    volumes:
      - ./frontend:/app/source
      - frontend_node_modules:/app/node_modules
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5173"]
      interval: 30s
      timeout: 10s
      retries: 3

  # ===========================================
  # Development Tools
  # ===========================================
  
  # pgAdmin - Database Management
  pgadmin:
    image: dpage/pgadmin4:latest
    container_name: aquacontrol-pgadmin
    restart: unless-stopped
    environment:
      PGADMIN_DEFAULT_EMAIL: admin@aquacontrol.com
      PGADMIN_DEFAULT_PASSWORD: AquaControl123!
      PGADMIN_CONFIG_SERVER_MODE: 'False'
      PGADMIN_CONFIG_MASTER_PASSWORD_REQUIRED: 'False'
    ports:
      - "8080:80"
    depends_on:
      - timescaledb
    networks:
      - aquacontrol-network
    volumes:
      - pgadmin_data:/var/lib/pgadmin
      - ./docker/configs/pgadmin/servers.json:/pgadmin4/servers.json

  # Kafka UI - Message Queue Management
  kafka-ui:
    image: provectuslabs/kafka-ui:latest
    container_name: aquacontrol-kafka-ui
    restart: unless-stopped
    environment:
      KAFKA_CLUSTERS_0_NAME: aquacontrol-cluster
      KAFKA_CLUSTERS_0_BOOTSTRAPSERVERS: kafka:29092
      KAFKA_CLUSTERS_0_ZOOKEEPER: zookeeper:2181
    ports:
      - "8081:8080"
    depends_on:
      kafka:
        condition: service_healthy
    networks:
      - aquacontrol-network

  # Redis Commander - Redis Management
  redis-commander:
    image: rediscommander/redis-commander:latest
    container_name: aquacontrol-redis-commander
    restart: unless-stopped
    environment:
      - REDIS_HOSTS=local:redis:6379:0:AquaControl123!
    ports:
      - "8082:8081"
    depends_on:
      - redis
    networks:
      - aquacontrol-network

  # ===========================================
  # Monitoring Services
  # ===========================================
  
  # Prometheus - Metrics Collection
  prometheus:
    image: prom/prometheus:latest
    container_name: aquacontrol-prometheus
    restart: unless-stopped
    command:
      - '--config.file=/etc/prometheus/prometheus.yml'
      - '--storage.tsdb.path=/prometheus'
      - '--web.console.libraries=/etc/prometheus/console_libraries'
      - '--web.console.templates=/etc/prometheus/consoles'
      - '--storage.tsdb.retention.time=200h'
      - '--web.enable-lifecycle'
    ports:
      - "9090:9090"
    volumes:
      - ./docker/configs/prometheus/prometheus.yml:/etc/prometheus/prometheus.yml
      - prometheus_data:/prometheus
    networks:
      - aquacontrol-network

  # Grafana - Metrics Visualization
  grafana:
    image: grafana/grafana:latest
    container_name: aquacontrol-grafana
    restart: unless-stopped
    environment:
      - GF_SECURITY_ADMIN_USER=admin
      - GF_SECURITY_ADMIN_PASSWORD=AquaControl123!
      - GF_USERS_ALLOW_SIGN_UP=false
      - GF_INSTALL_PLUGINS=grafana-clock-panel,grafana-simple-json-datasource
    ports:
      - "3000:3000"
    volumes:
      - grafana_data:/var/lib/grafana
      - ./docker/configs/grafana/provisioning:/etc/grafana/provisioning
      - ./docker/configs/grafana/dashboards:/var/lib/grafana/dashboards
    depends_on:
      - prometheus
    networks:
      - aquacontrol-network

  # ===========================================
  # Reverse Proxy
  # ===========================================
  
  # Nginx - Reverse Proxy and Load Balancer
  nginx:
    image: nginx:alpine
    container_name: aquacontrol-nginx
    restart: unless-stopped
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./docker/configs/nginx/nginx.conf:/etc/nginx/nginx.conf
      - ./docker/configs/nginx/default.conf:/etc/nginx/conf.d/default.conf
      - nginx_logs:/var/log/nginx
    depends_on:
      - backend
      - frontend
    networks:
      - aquacontrol-network

# ===========================================
# Volumes
# ===========================================
volumes:
  timescale_data:
    driver: local
  redis_data:
    driver: local
  zookeeper_data:
    driver: local
  zookeeper_logs:
    driver: local
  kafka_data:
    driver: local
  pgadmin_data:
    driver: local
  prometheus_data:
    driver: local
  grafana_data:
    driver: local
  backend_logs:
    driver: local
  frontend_node_modules:
    driver: local
  nginx_logs:
    driver: local

# ===========================================
# Networks
# ===========================================
networks:
  aquacontrol-network:
    driver: bridge
    ipam:
      config:
        - subnet: 172.20.0.0/16
```

### File 2: Backend Dockerfile for Development
**File:** `backend/Dockerfile.dev`

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS base
WORKDIR /app

# Install development tools
RUN apt-get update && apt-get install -y \
    curl \
    vim \
    htop \
    && rm -rf /var/lib/apt/lists/*

# Copy project files
COPY ["src/AquaControl.API/AquaControl.API.csproj", "src/AquaControl.API/"]
COPY ["src/AquaControl.Application/AquaControl.Application.csproj", "src/AquaControl.Application/"]
COPY ["src/AquaControl.Domain/AquaControl.Domain.csproj", "src/AquaControl.Domain/"]
COPY ["src/AquaControl.Infrastructure/AquaControl.Infrastructure.csproj", "src/AquaControl.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/AquaControl.API/AquaControl.API.csproj"

# Copy source code
COPY . .

# Install dotnet tools
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

# Build application
WORKDIR "/app/src/AquaControl.API"
RUN dotnet build "AquaControl.API.csproj" -c Debug -o /app/build

# Create logs directory
RUN mkdir -p /app/logs

# Expose port
EXPOSE 5000

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:5000

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:5000/health || exit 1

# Start application with hot reload
CMD ["dotnet", "watch", "run", "--project", "AquaControl.API.csproj", "--urls", "http://0.0.0.0:5000"]
```

### File 3: Frontend Dockerfile for Development
**File:** `frontend/Dockerfile.dev`

```dockerfile
FROM node:18-alpine AS base

# Install development tools
RUN apk add --no-cache \
    curl \
    git \
    bash

WORKDIR /app

# Copy package files
COPY package*.json ./

# Install dependencies
RUN npm ci --only=production=false

# Copy source code
COPY . .

# Create node_modules volume mount point
VOLUME ["/app/node_modules"]

# Expose port
EXPOSE 5173

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
    CMD curl -f http://localhost:5173 || exit 1

# Start development server
CMD ["npm", "run", "dev", "--", "--host", "0.0.0.0"]
```

### File 4: Database Initialization Scripts
**File:** `docker/scripts/init-timescale.sql`

```sql
-- Enable TimescaleDB extension
CREATE EXTENSION IF NOT EXISTS timescaledb CASCADE;

-- Create schemas
CREATE SCHEMA IF NOT EXISTS aquacontrol;
CREATE SCHEMA IF NOT EXISTS timeseries;
CREATE SCHEMA IF NOT EXISTS analytics;

-- Create roles
DO $$
BEGIN
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'aquacontrol_readonly') THEN
        CREATE ROLE aquacontrol_readonly;
    END IF;
    
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'aquacontrol_readwrite') THEN
        CREATE ROLE aquacontrol_readwrite;
    END IF;
END
$$;

-- Grant permissions
GRANT USAGE ON SCHEMA aquacontrol TO aquacontrol;
GRANT USAGE ON SCHEMA timeseries TO aquacontrol;
GRANT USAGE ON SCHEMA analytics TO aquacontrol;

GRANT ALL PRIVILEGES ON SCHEMA aquacontrol TO aquacontrol;
GRANT ALL PRIVILEGES ON SCHEMA timeseries TO aquacontrol;
GRANT ALL PRIVILEGES ON SCHEMA analytics TO aquacontrol;

-- Set default privileges
ALTER DEFAULT PRIVILEGES IN SCHEMA aquacontrol GRANT ALL ON TABLES TO aquacontrol;
ALTER DEFAULT PRIVILEGES IN SCHEMA timeseries GRANT ALL ON TABLES TO aquacontrol;
ALTER DEFAULT PRIVILEGES IN SCHEMA analytics GRANT ALL ON TABLES TO aquacontrol;

ALTER DEFAULT PRIVILEGES IN SCHEMA aquacontrol GRANT ALL ON SEQUENCES TO aquacontrol;
ALTER DEFAULT PRIVILEGES IN SCHEMA timeseries GRANT ALL ON SEQUENCES TO aquacontrol;
ALTER DEFAULT PRIVILEGES IN SCHEMA analytics GRANT ALL ON SEQUENCES TO aquacontrol;

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS idx_pg_stat_activity_state ON pg_stat_activity(state);
CREATE INDEX IF NOT EXISTS idx_pg_stat_activity_query_start ON pg_stat_activity(query_start);

-- Enable logging for slow queries
ALTER SYSTEM SET log_min_duration_statement = '1000ms';
ALTER SYSTEM SET log_statement = 'mod';
ALTER SYSTEM SET log_line_prefix = '%t [%p]: [%l-1] user=%u,db=%d,app=%a,client=%h ';

-- Reload configuration
SELECT pg_reload_conf();

-- Create monitoring functions
CREATE OR REPLACE FUNCTION get_database_size()
RETURNS TABLE(
    database_name text,
    size_mb numeric
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        datname::text,
        ROUND(pg_database_size(datname) / 1024.0 / 1024.0, 2)
    FROM pg_database
    WHERE datname NOT IN ('template0', 'template1', 'postgres');
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION get_table_sizes()
RETURNS TABLE(
    schema_name text,
    table_name text,
    size_mb numeric,
    row_count bigint
) AS $$
BEGIN
    RETURN QUERY
    SELECT 
        schemaname::text,
        tablename::text,
        ROUND(pg_total_relation_size(schemaname||'.'||tablename) / 1024.0 / 1024.0, 2),
        COALESCE(n_tup_ins + n_tup_upd + n_tup_del, 0)
    FROM pg_tables t
    LEFT JOIN pg_stat_user_tables s ON t.tablename = s.relname AND t.schemaname = s.schemaname
    WHERE schemaname IN ('aquacontrol', 'timeseries', 'analytics')
    ORDER BY pg_total_relation_size(schemaname||'.'||tablename) DESC;
END;
$$ LANGUAGE plpgsql;
```

**File:** `docker/scripts/create-hypertables.sql`

```sql
-- Wait for tables to be created by Entity Framework
-- This script will be run after the application starts

-- Function to create hypertables safely
CREATE OR REPLACE FUNCTION create_hypertable_if_not_exists(
    table_name text,
    time_column text,
    chunk_interval interval DEFAULT '1 hour'::interval
) RETURNS void AS $$
BEGIN
    -- Check if table exists and is not already a hypertable
    IF EXISTS (
        SELECT 1 FROM information_schema.tables 
        WHERE table_schema = 'timeseries' AND table_name = $1
    ) AND NOT EXISTS (
        SELECT 1 FROM timescaledb_information.hypertables 
        WHERE schema_name = 'timeseries' AND table_name = $1
    ) THEN
        PERFORM create_hypertable(
            'timeseries.' || quote_ident($1), 
            $2,
            chunk_time_interval => $3,
            if_not_exists => TRUE
        );
        
        RAISE NOTICE 'Created hypertable for timeseries.%', $1;
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        RAISE NOTICE 'Could not create hypertable for %: %', $1, SQLERRM;
END;
$$ LANGUAGE plpgsql;

-- Function to add compression policy
CREATE OR REPLACE FUNCTION add_compression_policy_if_not_exists(
    table_name text,
    compress_after interval DEFAULT '7 days'::interval
) RETURNS void AS $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM timescaledb_information.hypertables 
        WHERE schema_name = 'timeseries' AND table_name = $1
    ) THEN
        PERFORM add_compression_policy(
            'timeseries.' || quote_ident($1),
            $2,
            if_not_exists => TRUE
        );
        
        RAISE NOTICE 'Added compression policy for timeseries.%', $1;
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        RAISE NOTICE 'Could not add compression policy for %: %', $1, SQLERRM;
END;
$$ LANGUAGE plpgsql;

-- Function to add retention policy
CREATE OR REPLACE FUNCTION add_retention_policy_if_not_exists(
    table_name text,
    drop_after interval DEFAULT '1 year'::interval
) RETURNS void AS $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM timescaledb_information.hypertables 
        WHERE schema_name = 'timeseries' AND table_name = $1
    ) THEN
        PERFORM add_retention_policy(
            'timeseries.' || quote_ident($1),
            $2,
            if_not_exists => TRUE
        );
        
        RAISE NOTICE 'Added retention policy for timeseries.%', $1;
    END IF;
EXCEPTION
    WHEN OTHERS THEN
        RAISE NOTICE 'Could not add retention policy for %: %', $1, SQLERRM;
END;
$$ LANGUAGE plpgsql;

-- Create a procedure to setup all hypertables and policies
CREATE OR REPLACE FUNCTION setup_timescaledb_features() RETURNS void AS $$
BEGIN
    -- Create hypertables
    PERFORM create_hypertable_if_not_exists('SensorReadings', 'Timestamp', '1 hour'::interval);
    PERFORM create_hypertable_if_not_exists('Alerts', 'CreatedAt', '1 day'::interval);
    
    -- Add compression policies (compress data older than 7 days)
    PERFORM add_compression_policy_if_not_exists('SensorReadings', '7 days'::interval);
    PERFORM add_compression_policy_if_not_exists('Alerts', '30 days'::interval);
    
    -- Add retention policies (delete data older than 1 year)
    PERFORM add_retention_policy_if_not_exists('SensorReadings', '1 year'::interval);
    PERFORM add_retention_policy_if_not_exists('Alerts', '2 years'::interval);
    
    RAISE NOTICE 'TimescaleDB features setup completed';
END;
$$ LANGUAGE plpgsql;
```

### File 5: Configuration Files
**File:** `docker/configs/nginx/nginx.conf`

```nginx
user nginx;
worker_processes auto;
error_log /var/log/nginx/error.log warn;
pid /var/run/nginx.pid;

events {
    worker_connections 1024;
    use epoll;
    multi_accept on;
}

http {
    include /etc/nginx/mime.types;
    default_type application/octet-stream;

    # Logging
    log_format main '$remote_addr - $remote_user [$time_local] "$request" '
                    '$status $body_bytes_sent "$http_referer" '
                    '"$http_user_agent" "$http_x_forwarded_for"';

    access_log /var/log/nginx/access.log main;

    # Performance
    sendfile on;
    tcp_nopush on;
    tcp_nodelay on;
    keepalive_timeout 65;
    types_hash_max_size 2048;

    # Gzip compression
    gzip on;
    gzip_vary on;
    gzip_min_length 10240;
    gzip_proxied expired no-cache no-store private must-revalidate auth;
    gzip_types
        text/plain
        text/css
        text/xml
        text/javascript
        application/x-javascript
        application/xml+rss
        application/javascript
        application/json;

    # Rate limiting
    limit_req_zone $binary_remote_addr zone=api:10m rate=10r/s;
    limit_req_zone $binary_remote_addr zone=login:10m rate=1r/s;

    # Upstream servers
    upstream backend {
        server backend:5000;
        keepalive 32;
    }

    upstream frontend {
        server frontend:5173;
        keepalive 32;
    }

    # Include server configurations
    include /etc/nginx/conf.d/*.conf;
}
```

**File:** `docker/configs/nginx/default.conf`

```nginx
server {
    listen 80;
    server_name localhost;

    # Security headers
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-XSS-Protection "1; mode=block" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header Referrer-Policy "no-referrer-when-downgrade" always;
    add_header Content-Security-Policy "default-src 'self' http: https: data: blob: 'unsafe-inline'" always;

    # Frontend
    location / {
        proxy_pass http://frontend;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }

    # Backend API
    location /api/ {
        limit_req zone=api burst=20 nodelay;
        
        proxy_pass http://backend;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
        
        # Timeouts
        proxy_connect_timeout 60s;
        proxy_send_timeout 60s;
        proxy_read_timeout 60s;
    }

    # GraphQL endpoint
    location /graphql {
        limit_req zone=api burst=20 nodelay;
        
        proxy_pass http://backend;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    # SignalR Hubs
    location /hubs/ {
        proxy_pass http://backend;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
        
        # WebSocket specific settings
        proxy_buffering off;
        proxy_read_timeout 86400;
    }

    # Health checks
    location /health {
        proxy_pass http://backend;
        access_log off;
    }

    # Static files caching
    location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg)$ {
        expires 1y;
        add_header Cache-Control "public, immutable";
        try_files $uri @frontend;
    }

    location @frontend {
        proxy_pass http://frontend;
    }
}
```

### File 6: Management Scripts
**File:** `scripts/dev-setup.sh`

```bash
#!/bin/bash

# AquaControl Platform - Development Setup Script
set -e

echo "🚀 Setting up AquaControl Platform for development..."

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
if ! command -v docker-compose &> /dev/null; then
    print_error "Docker Compose is not installed. Please install Docker Compose first."
    exit 1
fi

# Create necessary directories
print_status "Creating necessary directories..."
mkdir -p docker/{configs/{nginx,postgres,redis,prometheus,grafana},scripts}
mkdir -p logs/{backend,frontend,nginx}

# Set permissions
print_status "Setting permissions..."
chmod +x scripts/*.sh

# Pull required Docker images
print_status "Pulling Docker images..."
docker-compose pull

# Build custom images
print_status "Building application images..."
docker-compose build --no-cache

# Start infrastructure services first
print_status "Starting infrastructure services..."
docker-compose up -d timescaledb redis zookeeper kafka

# Wait for services to be ready
print_status "Waiting for infrastructure services to be ready..."
sleep 30

# Check service health
print_status "Checking service health..."
docker-compose ps

# Start application services
print_status "Starting application services..."
docker-compose up -d backend frontend

# Wait for application to be ready
print_status "Waiting for application services to be ready..."
sleep 60

# Start monitoring and management tools
print_status "Starting monitoring and management tools..."
docker-compose up -d pgadmin kafka-ui redis-commander prometheus grafana nginx

# Display service URLs
print_status "🎉 Setup complete! Services are available at:"
echo ""
echo "📱 Frontend Application:     http://localhost"
echo "🔧 Backend API:              http://localhost/api"
echo "📊 GraphQL Playground:       http://localhost/graphql"
echo "🗄️  Database Admin (pgAdmin): http://localhost:8080"
echo "📨 Kafka UI:                 http://localhost:8081"
echo "🔴 Redis Commander:          http://localhost:8082"
echo "📈 Prometheus:               http://localhost:9090"
echo "📊 Grafana:                  http://localhost:3000"
echo ""
echo "🔑 Default Credentials:"
echo "   pgAdmin:    admin@aquacontrol.com / AquaControl123!"
echo "   Grafana:    admin / AquaControl123!"
echo ""
echo "📝 To view logs: docker-compose logs -f [service-name]"
echo "🛑 To stop all services: docker-compose down"
echo "🔄 To restart a service: docker-compose restart [service-name]"

# Check if all services are running
print_status "Performing health checks..."
sleep 10

# Test backend health
if curl -f http://localhost/health &> /dev/null; then
    print_status "✅ Backend is healthy"
else
    print_warning "⚠️  Backend health check failed"
fi

# Test frontend
if curl -f http://localhost &> /dev/null; then
    print_status "✅ Frontend is accessible"
else
    print_warning "⚠️  Frontend accessibility check failed"
fi

print_status "🚀 AquaControl Platform is ready for development!"
```

**File:** `scripts/dev-teardown.sh`

```bash
#!/bin/bash

# AquaControl Platform - Development Teardown Script
set -e

echo "🛑 Tearing down AquaControl Platform development environment..."

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
docker-compose down

# Remove containers
print_status "Removing containers..."
docker-compose rm -f

# Clean up volumes (optional - comment out to preserve data)
read -p "Do you want to remove all data volumes? This will delete all database data! (y/N): " -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]; then
    print_warning "Removing all volumes and data..."
    docker-compose down -v
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

print_status "✅ Teardown complete!"
```

This completes the comprehensive Docker Compose setup with:

✅ **Complete Development Environment** - All services in one setup  
✅ **Production-Ready Configuration** - Proper networking and security  
✅ **Monitoring & Management Tools** - pgAdmin, Kafka UI, Redis Commander  
✅ **Performance Optimization** - Nginx reverse proxy with caching  
✅ **Health Checks** - Automated service health monitoring  
✅ **Easy Management Scripts** - Setup and teardown automation  
✅ **Comprehensive Logging** - Centralized log management  
✅ **Development Tools** - Hot reload and debugging support  

You can now run the entire platform with a single command! 🚀
