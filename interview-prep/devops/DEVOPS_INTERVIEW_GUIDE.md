# 🚀 AquaControl Platform DevOps - Comprehensive Interview Preparation Guide

## Table of Contents
1. [DevOps Overview](#devops-overview)
2. [Docker & Containerization](#docker--containerization)
3. [Docker Compose](#docker-compose)
4. [CI/CD Pipeline](#cicd-pipeline)
5. [Infrastructure as Code](#infrastructure-as-code)
6. [Monitoring & Logging](#monitoring--logging)
7. [Security & Secrets Management](#security--secrets-management)
8. [Networking & Load Balancing](#networking--load-balancing)
9. [Database Management](#database-management)
10. [Performance & Optimization](#performance--optimization)
11. [Troubleshooting & Debugging](#troubleshooting--debugging)
12. [Common Interview Questions](#common-interview-questions)

---

## 1. DevOps Overview

### What is DevOps in This Project?

**Answer**: "In the AquaControl Platform, I implemented DevOps practices to automate deployment, ensure consistency across environments, and enable rapid iteration. The stack includes:
- **Containerization**: Docker for all services (backend, frontend, databases)
- **Orchestration**: Docker Compose for local development, Kubernetes for production
- **Infrastructure as Code**: Terraform for AWS infrastructure
- **CI/CD**: GitHub Actions for automated testing and deployment
- **Monitoring**: Prometheus and Grafana for metrics and visualization
- **Logging**: Centralized logging with Serilog and ELK stack"

### Key Responsibilities:
- Container orchestration and management
- Automated deployment pipelines
- Infrastructure provisioning and management
- Monitoring and alerting setup
- Security and secrets management
- Performance optimization
- Disaster recovery and backup strategies

---

## 2. Docker & Containerization

### Backend Dockerfile (Multi-stage Build)

```dockerfile
# AquaControl-Platform/backend/Dockerfile.prod
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["src/AquaControl.API/AquaControl.API.csproj", "src/AquaControl.API/"]
COPY ["src/AquaControl.Application/AquaControl.Application.csproj", "src/AquaControl.Application/"]
COPY ["src/AquaControl.Domain/AquaControl.Domain.csproj", "src/AquaControl.Domain/"]
COPY ["src/AquaControl.Infrastructure/AquaControl.Infrastructure.csproj", "src/AquaControl.Infrastructure/"]

RUN dotnet restore "src/AquaControl.API/AquaControl.API.csproj"

# Copy source code and build
COPY . .
WORKDIR "/src/src/AquaControl.API"
RUN dotnet build "AquaControl.API.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "AquaControl.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create non-root user for security
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

COPY --from=publish /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl --fail http://localhost:5000/health || exit 1

EXPOSE 5000
ENTRYPOINT ["dotnet", "AquaControl.API.dll"]
```

**Interview Answer**: "I use multi-stage Docker builds to:
1. **Build Stage**: Restore dependencies and compile code
2. **Publish Stage**: Create optimized release build
3. **Runtime Stage**: Copy only runtime artifacts to minimal base image

Benefits:
- **Smaller Images**: Final image only contains runtime dependencies
- **Security**: Non-root user, minimal attack surface
- **Performance**: Faster pulls and deploys
- **Caching**: Layer caching speeds up builds"

### Frontend Dockerfile

```dockerfile
# AquaControl-Platform/frontend/Dockerfile.prod
FROM node:20-alpine AS build
WORKDIR /app

# Copy package files and install dependencies
COPY package*.json ./
RUN npm ci --only=production

# Copy source and build
COPY . .
RUN npm run build

# Production image with nginx
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

# Remove default nginx static assets
RUN rm -rf ./*

# Copy built assets from build stage
COPY --from=build /app/dist .

# Copy custom nginx configuration
COPY nginx.prod.conf /etc/nginx/conf.d/default.conf

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD wget --quiet --tries=1 --spider http://localhost:80/health || exit 1

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

### Nginx Configuration for Frontend

```nginx
# nginx.prod.conf
server {
    listen 80;
    server_name _;
    root /usr/share/nginx/html;
    index index.html;

    # Gzip compression
    gzip on;
    gzip_vary on;
    gzip_min_length 1024;
    gzip_types text/plain text/css text/xml text/javascript application/javascript application/json;

    # Security headers
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;

    # API proxy
    location /api {
        proxy_pass http://backend:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    # SignalR WebSocket proxy
    location /hubs {
        proxy_pass http://backend:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }

    # SPA routing
    location / {
        try_files $uri $uri/ /index.html;
    }

    # Cache static assets
    location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg|woff|woff2|ttf|eot)$ {
        expires 1y;
        add_header Cache-Control "public, immutable";
    }

    # Health check endpoint
    location /health {
        access_log off;
        return 200 "healthy\n";
        add_header Content-Type text/plain;
    }
}
```

**Interview Answer**: "The nginx configuration handles:
- **Reverse Proxy**: Routes /api requests to backend
- **WebSocket Support**: Proxies SignalR connections
- **SPA Routing**: Serves index.html for all routes
- **Compression**: Gzip for text assets
- **Caching**: Long-term cache for static assets
- **Security**: Security headers for XSS, clickjacking protection"

---

## 3. Docker Compose

### Development Environment

```yaml
# docker-compose.dev.yml
version: '3.8'

services:
  backend:
    build:
      context: ./backend
      dockerfile: Dockerfile.dev
    container_name: aquacontrol-backend-dev
    restart: unless-stopped
    ports:
      - "5000:5000"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:5000
      - ConnectionStrings__DefaultConnection=Host=timescaledb;Port=5432;Database=aquacontrol_dev;Username=aquacontrol;Password=${DB_PASSWORD:-AquaControl123!}
      - ConnectionStrings__Redis=redis:6379
      - JwtSettings__SecretKey=${JWT_SECRET:-development-secret-key-change-in-production-at-least-32-characters-long-enough}
    volumes:
      - ./backend/src:/app/src
      - ./backend/logs:/app/logs
    depends_on:
      timescaledb:
        condition: service_healthy
      redis:
        condition: service_healthy
    networks:
      - aquacontrol-network
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5000/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s

  frontend:
    build:
      context: ./frontend
      dockerfile: Dockerfile.dev
    container_name: aquacontrol-frontend-dev
    restart: unless-stopped
    ports:
      - "5173:5173"
    environment:
      - VITE_API_BASE_URL=http://localhost:5000
    volumes:
      - ./frontend/src:/app/src
      - ./frontend/node_modules:/app/node_modules
    depends_on:
      - backend
    networks:
      - aquacontrol-network

  timescaledb:
    image: timescale/timescaledb:latest-pg15
    container_name: aquacontrol-timescaledb-dev
    restart: unless-stopped
    environment:
      - POSTGRES_DB=aquacontrol_dev
      - POSTGRES_USER=aquacontrol
      - POSTGRES_PASSWORD=${DB_PASSWORD:-AquaControl123!}
      - TIMESCALEDB_TELEMETRY=off
    ports:
      - "5433:5432"  # Avoid conflict with local PostgreSQL
    volumes:
      - timescaledb_data_dev:/var/lib/postgresql/data
      - ./data-engineering/timescaledb/init-scripts:/docker-entrypoint-initdb.d
      - ./data-engineering/timescaledb/config/postgresql.conf:/etc/postgresql/postgresql.conf
    networks:
      - aquacontrol-network
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U aquacontrol -d aquacontrol_dev"]
      interval: 10s
      timeout: 5s
      retries: 5

  redis:
    image: redis:7-alpine
    container_name: aquacontrol-redis-dev
    restart: unless-stopped
    ports:
      - "6381:6379"  # Avoid conflict with local Redis
    volumes:
      - redis_data_dev:/data
      - ./docker/configs/redis/redis.conf:/usr/local/etc/redis/redis.conf
    command: redis-server /usr/local/etc/redis/redis.conf
    networks:
      - aquacontrol-network
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 10s
      timeout: 3s
      retries: 5

  zookeeper:
    image: confluentinc/cp-zookeeper:7.5.0
    container_name: aquacontrol-zookeeper-dev
    environment:
      ZOOKEEPER_CLIENT_PORT: 2181
      ZOOKEEPER_TICK_TIME: 2000
    ports:
      - "2181:2181"
    volumes:
      - zookeeper_data_dev:/var/lib/zookeeper/data
      - zookeeper_logs_dev:/var/lib/zookeeper/log
    networks:
      - aquacontrol-network

  kafka:
    image: confluentinc/cp-kafka:7.5.0
    container_name: aquacontrol-kafka-dev
    depends_on:
      - zookeeper
    ports:
      - "9092:9092"
      - "9093:9093"
    environment:
      KAFKA_BROKER_ID: 1
      KAFKA_ZOOKEEPER_CONNECT: zookeeper:2181
      KAFKA_ADVERTISED_LISTENERS: PLAINTEXT://kafka:9092,PLAINTEXT_HOST://localhost:9093
      KAFKA_LISTENER_SECURITY_PROTOCOL_MAP: PLAINTEXT:PLAINTEXT,PLAINTEXT_HOST:PLAINTEXT
      KAFKA_INTER_BROKER_LISTENER_NAME: PLAINTEXT
      KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR: 1
      KAFKA_AUTO_CREATE_TOPICS_ENABLE: "true"
    volumes:
      - kafka_data_dev:/var/lib/kafka/data
    networks:
      - aquacontrol-network

volumes:
  timescaledb_data_dev:
    driver: local
  redis_data_dev:
    driver: local
  zookeeper_data_dev:
    driver: local
  zookeeper_logs_dev:
    driver: local
  kafka_data_dev:
    driver: local

networks:
  aquacontrol-network:
    driver: bridge
```

**Interview Answer**: "The Docker Compose setup:
- **Service Dependencies**: `depends_on` with health checks ensures proper startup order
- **Port Mapping**: Maps to non-conflicting ports (5433 for DB, 6381 for Redis)
- **Environment Variables**: Uses ${VAR:-default} for flexible configuration
- **Volumes**: Persists data and enables hot-reload for development
- **Networks**: Custom bridge network for service communication
- **Health Checks**: Ensures services are ready before dependent services start"

### Production Environment

```yaml
# docker-compose.prod.yml
version: '3.8'

services:
  backend:
    build:
      context: ./backend
      dockerfile: Dockerfile.prod
    container_name: aquacontrol-backend-prod
    restart: always
    ports:
      - "5000:5000"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:5000
      - ConnectionStrings__DefaultConnection=Host=timescaledb;Port=5432;Database=aquacontrol_prod;Username=aquacontrol;Password=${DB_PASSWORD}
      - ConnectionStrings__Redis=redis:6379,password=${REDIS_PASSWORD}
      - JwtSettings__SecretKey=${JWT_SECRET}
    secrets:
      - db_password
      - jwt_secret
      - redis_password
    depends_on:
      - timescaledb
      - redis
    networks:
      - aquacontrol-network
    deploy:
      replicas: 2
      resources:
        limits:
          cpus: '1'
          memory: 1G
        reservations:
          cpus: '0.5'
          memory: 512M
      restart_policy:
        condition: on-failure
        delay: 5s
        max_attempts: 3
        window: 120s

  frontend:
    build:
      context: ./frontend
      dockerfile: Dockerfile.prod
    container_name: aquacontrol-frontend-prod
    restart: always
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./docker/configs/nginx/ssl:/etc/nginx/ssl:ro
    depends_on:
      - backend
    networks:
      - aquacontrol-network
    deploy:
      resources:
        limits:
          cpus: '0.5'
          memory: 512M

  timescaledb:
    image: timescale/timescaledb:latest-pg15
    container_name: aquacontrol-timescaledb-prod
    restart: always
    environment:
      - POSTGRES_DB=aquacontrol_prod
      - POSTGRES_USER=aquacontrol
      - POSTGRES_PASSWORD_FILE=/run/secrets/db_password
      - TIMESCALEDB_TELEMETRY=off
    secrets:
      - db_password
    volumes:
      - timescaledb_data_prod:/var/lib/postgresql/data
      - ./backups:/backups
    networks:
      - aquacontrol-network
    deploy:
      resources:
        limits:
          cpus: '2'
          memory: 4G

  redis:
    image: redis:7-alpine
    container_name: aquacontrol-redis-prod
    restart: always
    command: >
      redis-server
      --requirepass ${REDIS_PASSWORD}
      --appendonly yes
      --appendfsync everysec
    volumes:
      - redis_data_prod:/data
    networks:
      - aquacontrol-network

  prometheus:
    image: prom/prometheus:latest
    container_name: aquacontrol-prometheus
    restart: always
    ports:
      - "9090:9090"
    volumes:
      - ./docker/configs/prometheus/prometheus.yml:/etc/prometheus/prometheus.yml:ro
      - prometheus_data:/prometheus
    command:
      - '--config.file=/etc/prometheus/prometheus.yml'
      - '--storage.tsdb.path=/prometheus'
      - '--storage.tsdb.retention.time=30d'
    networks:
      - aquacontrol-network

  grafana:
    image: grafana/grafana:latest
    container_name: aquacontrol-grafana
    restart: always
    ports:
      - "3000:3000"
    environment:
      - GF_SECURITY_ADMIN_PASSWORD=${GRAFANA_PASSWORD}
      - GF_INSTALL_PLUGINS=grafana-clock-panel
    volumes:
      - grafana_data:/var/lib/grafana
      - ./docker/configs/grafana/dashboards:/etc/grafana/provisioning/dashboards:ro
      - ./docker/configs/grafana/datasources:/etc/grafana/provisioning/datasources:ro
    depends_on:
      - prometheus
    networks:
      - aquacontrol-network

secrets:
  db_password:
    file: ./secrets/db_password.txt
  jwt_secret:
    file: ./secrets/jwt_secret.txt
  redis_password:
    file: ./secrets/redis_password.txt

volumes:
  timescaledb_data_prod:
  redis_data_prod:
  prometheus_data:
  grafana_data:

networks:
  aquacontrol-network:
    driver: bridge
```

**Interview Answer**: "Production setup includes:
- **Secrets Management**: Docker secrets for sensitive data
- **Resource Limits**: CPU and memory constraints
- **Restart Policies**: Automatic recovery from failures
- **Monitoring**: Prometheus and Grafana for observability
- **Backups**: Volume mounts for database backups
- **High Availability**: Multiple backend replicas"

---

## 4. CI/CD Pipeline

### GitHub Actions Workflow

```yaml
# .github/workflows/ci-cd.yml
name: CI/CD Pipeline

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main, develop]

env:
  DOCKER_REGISTRY: ghcr.io
  IMAGE_NAME: ${{ github.repository }}

jobs:
  backend-test:
    name: Backend Tests
    runs-on: ubuntu-latest
    
    services:
      postgres:
        image: postgres:15
        env:
          POSTGRES_DB: aquacontrol_test
          POSTGRES_USER: aquacontrol
          POSTGRES_PASSWORD: test123
        ports:
          - 5432:5432
        options: >-
          --health-cmd pg_isready
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5

    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Restore dependencies
        run: dotnet restore
        working-directory: ./AquaControl-Platform/backend

      - name: Build
        run: dotnet build --no-restore --configuration Release
        working-directory: ./AquaControl-Platform/backend

      - name: Run unit tests
        run: dotnet test --no-build --configuration Release --verbosity normal --collect:"XPlat Code Coverage"
        working-directory: ./AquaControl-Platform/backend

      - name: Upload coverage reports
        uses: codecov/codecov-action@v3
        with:
          files: ./AquaControl-Platform/backend/tests/**/coverage.cobertura.xml
          flags: backend

  frontend-test:
    name: Frontend Tests
    runs-on: ubuntu-latest

    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Setup Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '20'
          cache: 'npm'
          cache-dependency-path: ./AquaControl-Platform/frontend/package-lock.json

      - name: Install dependencies
        run: npm ci
        working-directory: ./AquaControl-Platform/frontend

      - name: Run linter
        run: npm run lint
        working-directory: ./AquaControl-Platform/frontend

      - name: Run unit tests
        run: npm run test:unit
        working-directory: ./AquaControl-Platform/frontend

      - name: Build
        run: npm run build
        working-directory: ./AquaControl-Platform/frontend

  docker-build:
    name: Build Docker Images
    runs-on: ubuntu-latest
    needs: [backend-test, frontend-test]
    if: github.event_name == 'push' && github.ref == 'refs/heads/main'

    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Set up Docker Buildx
        uses: docker/setup-buildx-action@v3

      - name: Log in to GitHub Container Registry
        uses: docker/login-action@v3
        with:
          registry: ${{ env.DOCKER_REGISTRY }}
          username: ${{ github.actor }}
          password: ${{ secrets.GITHUB_TOKEN }}

      - name: Extract metadata for backend
        id: meta-backend
        uses: docker/metadata-action@v5
        with:
          images: ${{ env.DOCKER_REGISTRY }}/${{ env.IMAGE_NAME }}/backend
          tags: |
            type=ref,event=branch
            type=sha,prefix={{branch}}-
            type=semver,pattern={{version}}

      - name: Build and push backend image
        uses: docker/build-push-action@v5
        with:
          context: ./AquaControl-Platform/backend
          file: ./AquaControl-Platform/backend/Dockerfile.prod
          push: true
          tags: ${{ steps.meta-backend.outputs.tags }}
          labels: ${{ steps.meta-backend.outputs.labels }}
          cache-from: type=gha
          cache-to: type=gha,mode=max

      - name: Extract metadata for frontend
        id: meta-frontend
        uses: docker/metadata-action@v5
        with:
          images: ${{ env.DOCKER_REGISTRY }}/${{ env.IMAGE_NAME }}/frontend
          tags: |
            type=ref,event=branch
            type=sha,prefix={{branch}}-
            type=semver,pattern={{version}}

      - name: Build and push frontend image
        uses: docker/build-push-action@v5
        with:
          context: ./AquaControl-Platform/frontend
          file: ./AquaControl-Platform/frontend/Dockerfile.prod
          push: true
          tags: ${{ steps.meta-frontend.outputs.tags }}
          labels: ${{ steps.meta-frontend.outputs.labels }}
          cache-from: type=gha
          cache-to: type=gha,mode=max

  deploy:
    name: Deploy to AWS EKS
    runs-on: ubuntu-latest
    needs: docker-build
    if: github.event_name == 'push' && github.ref == 'refs/heads/main'

    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Configure AWS credentials
        uses: aws-actions/configure-aws-credentials@v4
        with:
          aws-access-key-id: ${{ secrets.AWS_ACCESS_KEY_ID }}
          aws-secret-access-key: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
          aws-region: us-east-1

      - name: Update kubeconfig
        run: |
          aws eks update-kubeconfig --name aquacontrol-cluster --region us-east-1

      - name: Deploy to Kubernetes
        run: |
          kubectl set image deployment/backend backend=${{ env.DOCKER_REGISTRY }}/${{ env.IMAGE_NAME }}/backend:main-${{ github.sha }}
          kubectl set image deployment/frontend frontend=${{ env.DOCKER_REGISTRY }}/${{ env.IMAGE_NAME }}/frontend:main-${{ github.sha }}
          kubectl rollout status deployment/backend
          kubectl rollout status deployment/frontend

      - name: Verify deployment
        run: |
          kubectl get pods
          kubectl get services
```

**Interview Answer**: "The CI/CD pipeline:
1. **Testing**: Runs backend and frontend tests in parallel
2. **Building**: Builds Docker images only on main branch
3. **Publishing**: Pushes images to GitHub Container Registry
4. **Deployment**: Deploys to AWS EKS with rolling updates
5. **Verification**: Checks deployment status

Benefits:
- Automated quality checks
- Fast feedback on PRs
- Consistent deployments
- Rollback capability"

---

## 5. Infrastructure as Code

### Terraform Configuration

```hcl
# infrastructure/terraform/main.tf
terraform {
  required_version = ">= 1.0"
  
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }
  
  backend "s3" {
    bucket         = "aquacontrol-terraform-state"
    key            = "production/terraform.tfstate"
    region         = "us-east-1"
    encrypt        = true
    dynamodb_table = "terraform-state-lock"
  }
}

provider "aws" {
  region = var.aws_region
  
  default_tags {
    tags = {
      Project     = "AquaControl"
      Environment = var.environment
      ManagedBy   = "Terraform"
    }
  }
}

# VPC Configuration
module "vpc" {
  source = "terraform-aws-modules/vpc/aws"
  version = "5.0.0"

  name = "${var.project_name}-vpc"
  cidr = var.vpc_cidr

  azs             = var.availability_zones
  private_subnets = var.private_subnet_cidrs
  public_subnets  = var.public_subnet_cidrs

  enable_nat_gateway = true
  enable_vpn_gateway = false
  enable_dns_hostnames = true
  enable_dns_support   = true

  tags = {
    Name = "${var.project_name}-vpc"
  }
}

# EKS Cluster
module "eks" {
  source = "terraform-aws-modules/eks/aws"
  version = "19.0.0"

  cluster_name    = "${var.project_name}-cluster"
  cluster_version = "1.28"

  vpc_id     = module.vpc.vpc_id
  subnet_ids = module.vpc.private_subnets

  cluster_endpoint_public_access = true

  eks_managed_node_groups = {
    general = {
      desired_size = 2
      min_size     = 1
      max_size     = 4

      instance_types = ["t3.medium"]
      capacity_type  = "ON_DEMAND"

      labels = {
        role = "general"
      }

      tags = {
        Name = "${var.project_name}-node-group"
      }
    }
  }

  tags = {
    Name = "${var.project_name}-eks-cluster"
  }
}

# RDS for TimescaleDB
resource "aws_db_instance" "timescaledb" {
  identifier = "${var.project_name}-timescaledb"

  engine         = "postgres"
  engine_version = "15.3"
  instance_class = "db.t3.medium"

  allocated_storage     = 100
  max_allocated_storage = 500
  storage_type          = "gp3"
  storage_encrypted     = true

  db_name  = "aquacontrol_prod"
  username = "aquacontrol"
  password = var.db_password

  vpc_security_group_ids = [aws_security_group.rds.id]
  db_subnet_group_name   = aws_db_subnet_group.main.name

  backup_retention_period = 7
  backup_window          = "03:00-04:00"
  maintenance_window     = "mon:04:00-mon:05:00"

  enabled_cloudwatch_logs_exports = ["postgresql", "upgrade"]

  skip_final_snapshot = false
  final_snapshot_identifier = "${var.project_name}-final-snapshot"

  tags = {
    Name = "${var.project_name}-timescaledb"
  }
}

# ElastiCache for Redis
resource "aws_elasticache_cluster" "redis" {
  cluster_id           = "${var.project_name}-redis"
  engine               = "redis"
  engine_version       = "7.0"
  node_type            = "cache.t3.micro"
  num_cache_nodes      = 1
  parameter_group_name = "default.redis7"
  port                 = 6379

  subnet_group_name    = aws_elasticache_subnet_group.main.name
  security_group_ids   = [aws_security_group.redis.id]

  snapshot_retention_limit = 5
  snapshot_window         = "03:00-05:00"

  tags = {
    Name = "${var.project_name}-redis"
  }
}

# S3 Bucket for Backups
resource "aws_s3_bucket" "backups" {
  bucket = "${var.project_name}-backups"

  tags = {
    Name = "${var.project_name}-backups"
  }
}

resource "aws_s3_bucket_versioning" "backups" {
  bucket = aws_s3_bucket.backups.id

  versioning_configuration {
    status = "Enabled"
  }
}

resource "aws_s3_bucket_server_side_encryption_configuration" "backups" {
  bucket = aws_s3_bucket.backups.id

  rule {
    apply_server_side_encryption_by_default {
      sse_algorithm = "AES256"
    }
  }
}

# CloudWatch Log Group
resource "aws_cloudwatch_log_group" "application" {
  name              = "/aws/eks/${var.project_name}/application"
  retention_in_days = 30

  tags = {
    Name = "${var.project_name}-logs"
  }
}
```

**Interview Answer**: "I use Terraform for Infrastructure as Code to:
- **Version Control**: Infrastructure changes tracked in Git
- **Reproducibility**: Identical environments across dev/staging/prod
- **Automation**: Provision entire infrastructure with one command
- **State Management**: S3 backend with DynamoDB locking
- **Modularity**: Reusable modules for VPC, EKS, RDS
- **Safety**: Plan before apply, prevents accidental changes"

---

## 6. Monitoring & Logging

### Prometheus Configuration

```yaml
# docker/configs/prometheus/prometheus.yml
global:
  scrape_interval: 15s
  evaluation_interval: 15s
  external_labels:
    cluster: 'aquacontrol-prod'
    environment: 'production'

scrape_configs:
  - job_name: 'prometheus'
    static_configs:
      - targets: ['localhost:9090']

  - job_name: 'backend'
    static_configs:
      - targets: ['backend:5000']
    metrics_path: '/metrics'
    scrape_interval: 10s

  - job_name: 'timescaledb'
    static_configs:
      - targets: ['timescaledb:5432']
    scrape_interval: 30s

  - job_name: 'redis'
    static_configs:
      - targets: ['redis:6379']
    scrape_interval: 30s

  - job_name: 'node-exporter'
    static_configs:
      - targets: ['node-exporter:9100']

alerting:
  alertmanagers:
    - static_configs:
        - targets: ['alertmanager:9093']

rule_files:
  - '/etc/prometheus/alerts/*.yml'
```

### Alert Rules

```yaml
# docker/configs/prometheus/alerts/backend-alerts.yml
groups:
  - name: backend_alerts
    interval: 30s
    rules:
      - alert: HighErrorRate
        expr: rate(http_requests_total{status=~"5.."}[5m]) > 0.05
        for: 5m
        labels:
          severity: critical
        annotations:
          summary: "High error rate detected"
          description: "Error rate is {{ $value }} errors per second"

      - alert: HighResponseTime
        expr: histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m])) > 1
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "High response time detected"
          description: "95th percentile response time is {{ $value }} seconds"

      - alert: ServiceDown
        expr: up{job="backend"} == 0
        for: 2m
        labels:
          severity: critical
        annotations:
          summary: "Backend service is down"
          description: "Backend has been down for more than 2 minutes"

  - name: database_alerts
    interval: 30s
    rules:
      - alert: HighDatabaseConnections
        expr: pg_stat_database_numbackends > 80
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "High number of database connections"
          description: "Database has {{ $value }} active connections"

      - alert: DatabaseDiskSpaceLow
        expr: (pg_database_size_bytes / pg_settings_max_wal_size_bytes) > 0.8
        for: 10m
        labels:
          severity: warning
        annotations:
          summary: "Database disk space running low"
          description: "Database is using {{ $value }}% of available space"
```

### Grafana Dashboard

```json
{
  "dashboard": {
    "title": "AquaControl Platform Overview",
    "panels": [
      {
        "title": "Request Rate",
        "targets": [
          {
            "expr": "rate(http_requests_total[5m])",
            "legendFormat": "{{method}} {{endpoint}}"
          }
        ],
        "type": "graph"
      },
      {
        "title": "Response Time (95th percentile)",
        "targets": [
          {
            "expr": "histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m]))",
            "legendFormat": "{{endpoint}}"
          }
        ],
        "type": "graph"
      },
      {
        "title": "Error Rate",
        "targets": [
          {
            "expr": "rate(http_requests_total{status=~\"5..\"}[5m])",
            "legendFormat": "{{endpoint}}"
          }
        ],
        "type": "graph"
      },
      {
        "title": "Database Connections",
        "targets": [
          {
            "expr": "pg_stat_database_numbackends",
            "legendFormat": "{{datname}}"
          }
        ],
        "type": "graph"
      },
      {
        "title": "Memory Usage",
        "targets": [
          {
            "expr": "container_memory_usage_bytes / container_spec_memory_limit_bytes",
            "legendFormat": "{{container_name}}"
          }
        ],
        "type": "graph"
      },
      {
        "title": "CPU Usage",
        "targets": [
          {
            "expr": "rate(container_cpu_usage_seconds_total[5m])",
            "legendFormat": "{{container_name}}"
          }
        ],
        "type": "graph"
      }
    ]
  }
}
```

**Interview Answer**: "Monitoring stack includes:
- **Prometheus**: Metrics collection and alerting
- **Grafana**: Visualization and dashboards
- **Alert Rules**: Proactive notification of issues
- **Metrics**: Request rate, response time, errors, resource usage
- **Dashboards**: Real-time system health overview"

---

## 7. Security & Secrets Management

### Docker Secrets

```bash
# Create secrets
echo "SuperSecurePassword123!" | docker secret create db_password -
echo "your-super-secret-jwt-key-at-least-32-chars" | docker secret create jwt_secret -
echo "RedisPassword123!" | docker secret create redis_password -

# Use in docker-compose.yml
services:
  backend:
    secrets:
      - db_password
      - jwt_secret
    environment:
      - ConnectionStrings__DefaultConnection=Host=timescaledb;Password=/run/secrets/db_password
```

### AWS Secrets Manager (Terraform)

```hcl
resource "aws_secretsmanager_secret" "db_password" {
  name = "${var.project_name}/db-password"
  
  tags = {
    Name = "${var.project_name}-db-password"
  }
}

resource "aws_secretsmanager_secret_version" "db_password" {
  secret_id     = aws_secretsmanager_secret.db_password.id
  secret_string = var.db_password
}

# IAM role for EKS to access secrets
resource "aws_iam_role_policy" "secrets_access" {
  name = "${var.project_name}-secrets-access"
  role = aws_iam_role.eks_node_group.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Action = [
          "secretsmanager:GetSecretValue",
          "secretsmanager:DescribeSecret"
        ]
        Resource = [
          aws_secretsmanager_secret.db_password.arn,
          aws_secretsmanager_secret.jwt_secret.arn
        ]
      }
    ]
  })
}
```

### Kubernetes Secrets

```yaml
# k8s/secrets.yaml
apiVersion: v1
kind: Secret
metadata:
  name: aquacontrol-secrets
  namespace: default
type: Opaque
data:
  db-password: <base64-encoded-password>
  jwt-secret: <base64-encoded-secret>
  redis-password: <base64-encoded-password>

---
# Use in deployment
apiVersion: apps/v1
kind: Deployment
metadata:
  name: backend
spec:
  template:
    spec:
      containers:
        - name: backend
          env:
            - name: DB_PASSWORD
              valueFrom:
                secretKeyRef:
                  name: aquacontrol-secrets
                  key: db-password
            - name: JWT_SECRET
              valueFrom:
                secretKeyRef:
                  name: aquacontrol-secrets
                  key: jwt-secret
```

---

## 8. Networking & Load Balancing

### Kubernetes Ingress

```yaml
# k8s/ingress.yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: aquacontrol-ingress
  annotations:
    kubernetes.io/ingress.class: nginx
    cert-manager.io/cluster-issuer: letsencrypt-prod
    nginx.ingress.kubernetes.io/ssl-redirect: "true"
    nginx.ingress.kubernetes.io/proxy-body-size: "50m"
    nginx.ingress.kubernetes.io/proxy-connect-timeout: "600"
    nginx.ingress.kubernetes.io/proxy-send-timeout: "600"
    nginx.ingress.kubernetes.io/proxy-read-timeout: "600"
spec:
  tls:
    - hosts:
        - aquacontrol.com
        - api.aquacontrol.com
      secretName: aquacontrol-tls
  rules:
    - host: aquacontrol.com
      http:
        paths:
          - path: /
            pathType: Prefix
            backend:
              service:
                name: frontend
                port:
                  number: 80
    - host: api.aquacontrol.com
      http:
        paths:
          - path: /
            pathType: Prefix
            backend:
              service:
                name: backend
                port:
                  number: 5000
```

### Application Load Balancer (Terraform)

```hcl
resource "aws_lb" "main" {
  name               = "${var.project_name}-alb"
  internal           = false
  load_balancer_type = "application"
  security_groups    = [aws_security_group.alb.id]
  subnets            = module.vpc.public_subnets

  enable_deletion_protection = true
  enable_http2              = true
  enable_cross_zone_load_balancing = true

  tags = {
    Name = "${var.project_name}-alb"
  }
}

resource "aws_lb_target_group" "backend" {
  name     = "${var.project_name}-backend-tg"
  port     = 5000
  protocol = "HTTP"
  vpc_id   = module.vpc.vpc_id

  health_check {
    enabled             = true
    healthy_threshold   = 2
    unhealthy_threshold = 2
    timeout             = 5
    interval            = 30
    path                = "/health"
    matcher             = "200"
  }

  tags = {
    Name = "${var.project_name}-backend-tg"
  }
}

resource "aws_lb_listener" "https" {
  load_balancer_arn = aws_lb.main.arn
  port              = "443"
  protocol          = "HTTPS"
  ssl_policy        = "ELBSecurityPolicy-TLS-1-2-2017-01"
  certificate_arn   = aws_acm_certificate.main.arn

  default_action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.backend.arn
  }
}
```

---

## 9. Database Management

### Backup Script

```bash
#!/bin/bash
# scripts/backup.sh

set -e

BACKUP_DIR="/backups"
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
DB_NAME="aquacontrol_prod"
DB_USER="aquacontrol"
DB_HOST="timescaledb"

# Create backup directory
mkdir -p "$BACKUP_DIR"

# Backup database
echo "Starting database backup..."
pg_dump -h "$DB_HOST" -U "$DB_USER" -d "$DB_NAME" -F c -f "$BACKUP_DIR/backup_$TIMESTAMP.dump"

# Compress backup
echo "Compressing backup..."
gzip "$BACKUP_DIR/backup_$TIMESTAMP.dump"

# Upload to S3
echo "Uploading to S3..."
aws s3 cp "$BACKUP_DIR/backup_$TIMESTAMP.dump.gz" "s3://aquacontrol-backups/database/"

# Clean old local backups (keep last 7 days)
echo "Cleaning old backups..."
find "$BACKUP_DIR" -name "backup_*.dump.gz" -mtime +7 -delete

echo "Backup completed successfully!"
```

### Restore Script

```bash
#!/bin/bash
# scripts/restore.sh

set -e

BACKUP_FILE=$1
DB_NAME="aquacontrol_prod"
DB_USER="aquacontrol"
DB_HOST="timescaledb"

if [ -z "$BACKUP_FILE" ]; then
    echo "Usage: ./restore.sh <backup_file>"
    exit 1
fi

# Download from S3 if needed
if [[ $BACKUP_FILE == s3://* ]]; then
    echo "Downloading backup from S3..."
    aws s3 cp "$BACKUP_FILE" /tmp/restore.dump.gz
    BACKUP_FILE="/tmp/restore.dump.gz"
fi

# Decompress if needed
if [[ $BACKUP_FILE == *.gz ]]; then
    echo "Decompressing backup..."
    gunzip -c "$BACKUP_FILE" > /tmp/restore.dump
    BACKUP_FILE="/tmp/restore.dump"
fi

# Restore database
echo "Restoring database..."
pg_restore -h "$DB_HOST" -U "$DB_USER" -d "$DB_NAME" -c "$BACKUP_FILE"

echo "Restore completed successfully!"
```

### Automated Backup (Kubernetes CronJob)

```yaml
apiVersion: batch/v1
kind: CronJob
metadata:
  name: database-backup
spec:
  schedule: "0 2 * * *"  # Daily at 2 AM
  jobTemplate:
    spec:
      template:
        spec:
          containers:
            - name: backup
              image: postgres:15
              command:
                - /bin/bash
                - -c
                - |
                  pg_dump -h timescaledb -U aquacontrol -d aquacontrol_prod -F c | \
                  gzip > /backups/backup_$(date +%Y%m%d_%H%M%S).dump.gz
              env:
                - name: PGPASSWORD
                  valueFrom:
                    secretKeyRef:
                      name: aquacontrol-secrets
                      key: db-password
              volumeMounts:
                - name: backup-storage
                  mountPath: /backups
          volumes:
            - name: backup-storage
              persistentVolumeClaim:
                claimName: backup-pvc
          restartPolicy: OnFailure
```

---

## 10. Performance & Optimization

### Docker Image Optimization

```dockerfile
# Multi-stage build reduces image size
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# ... build steps ...

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
# Only runtime, no SDK

# Use .dockerignore to exclude unnecessary files
# .dockerignore
node_modules
.git
.vscode
*.md
tests
```

### Kubernetes Resource Optimization

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: backend
spec:
  replicas: 3
  template:
    spec:
      containers:
        - name: backend
          resources:
            requests:
              cpu: 500m
              memory: 512Mi
            limits:
              cpu: 1000m
              memory: 1Gi
          livenessProbe:
            httpGet:
              path: /health
              port: 5000
            initialDelaySeconds: 30
            periodSeconds: 10
          readinessProbe:
            httpGet:
              path: /health/ready
              port: 5000
            initialDelaySeconds: 5
            periodSeconds: 5
      affinity:
        podAntiAffinity:
          preferredDuringSchedulingIgnoredDuringExecution:
            - weight: 100
              podAffinityTerm:
                labelSelector:
                  matchExpressions:
                    - key: app
                      operator: In
                      values:
                        - backend
                topologyKey: kubernetes.io/hostname
```

### Horizontal Pod Autoscaling

```yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: backend-hpa
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: backend
  minReplicas: 2
  maxReplicas: 10
  metrics:
    - type: Resource
      resource:
        name: cpu
        target:
          type: Utilization
          averageUtilization: 70
    - type: Resource
      resource:
        name: memory
        target:
          type: Utilization
          averageUtilization: 80
  behavior:
    scaleDown:
      stabilizationWindowSeconds: 300
      policies:
        - type: Percent
          value: 50
          periodSeconds: 60
    scaleUp:
      stabilizationWindowSeconds: 0
      policies:
        - type: Percent
          value: 100
          periodSeconds: 30
```

---

## 11. Troubleshooting & Debugging

### Common Commands

```bash
# Docker
docker ps -a                          # List all containers
docker logs -f <container_name>       # Follow logs
docker exec -it <container> /bin/bash # Shell into container
docker stats                          # Resource usage
docker system prune -a                # Clean up

# Docker Compose
docker compose ps                     # List services
docker compose logs -f backend        # Follow backend logs
docker compose restart backend        # Restart service
docker compose down -v                # Stop and remove volumes

# Kubernetes
kubectl get pods                      # List pods
kubectl logs -f <pod_name>           # Follow logs
kubectl describe pod <pod_name>      # Detailed pod info
kubectl exec -it <pod_name> -- /bin/bash  # Shell into pod
kubectl get events --sort-by='.lastTimestamp'  # Recent events
kubectl top pods                      # Resource usage

# Database
docker exec -it aquacontrol-timescaledb-dev psql -U aquacontrol -d aquacontrol_dev
\dt                                   # List tables
\d+ users                            # Describe table
SELECT * FROM users;                 # Query data

# Check health
curl http://localhost:5000/health    # Backend health
curl http://localhost:5173           # Frontend
```

### Debugging Scenarios

**Scenario 1: Container won't start**
```bash
# Check logs
docker logs aquacontrol-backend-dev

# Check if port is already in use
netstat -tuln | grep 5000

# Inspect container
docker inspect aquacontrol-backend-dev

# Check resource constraints
docker stats
```

**Scenario 2: Database connection issues**
```bash
# Check if database is running
docker ps | grep timescaledb

# Check database health
docker exec aquacontrol-timescaledb-dev pg_isready -U aquacontrol

# Test connection
docker exec aquacontrol-timescaledb-dev psql -U aquacontrol -d aquacontrol_dev -c "SELECT 1"

# Check connection string in backend
docker exec aquacontrol-backend-dev env | grep ConnectionStrings
```

**Scenario 3: High memory usage**
```bash
# Check container stats
docker stats --no-stream

# Check application metrics
curl http://localhost:5000/metrics

# Analyze memory dump (if enabled)
docker exec backend dotnet-dump collect -p 1
```

---

## 12. Common Interview Questions

### Q1: "Explain your containerization strategy"
**Answer**: "I use Docker for containerization with multi-stage builds to minimize image size. Development uses docker-compose.dev.yml with hot-reload volumes, while production uses docker-compose.prod.yml with optimized images, resource limits, and health checks. All services run in a custom bridge network for isolation and service discovery."

### Q2: "How do you handle secrets in different environments?"
**Answer**: "I use a layered approach:
- **Development**: Environment variables with defaults in docker-compose
- **Production**: Docker secrets for docker-compose, AWS Secrets Manager for EKS
- **CI/CD**: GitHub Secrets for pipeline credentials
- **Never**: Commit secrets to Git, use .gitignore for secret files"

### Q3: "Describe your CI/CD pipeline"
**Answer**: "GitHub Actions workflow with:
1. **Test Stage**: Parallel backend and frontend tests
2. **Build Stage**: Multi-platform Docker builds with BuildKit
3. **Push Stage**: Images to GitHub Container Registry
4. **Deploy Stage**: Rolling updates to Kubernetes
5. **Verify Stage**: Health checks and smoke tests

Pipeline only deploys on main branch, PRs only run tests."

### Q4: "How do you ensure zero-downtime deployments?"
**Answer**: "Multiple strategies:
- **Rolling Updates**: Kubernetes gradually replaces pods
- **Health Checks**: Readiness probes prevent traffic to unhealthy pods
- **Multiple Replicas**: Always have healthy pods serving traffic
- **Pod Disruption Budgets**: Ensure minimum availability during updates
- **Blue-Green Deployments**: For major changes, deploy to new environment first"

### Q5: "Explain your monitoring setup"
**Answer**: "Prometheus scrapes metrics from all services every 15 seconds. Grafana visualizes metrics with dashboards for request rate, response time, errors, and resource usage. Alert rules notify via email/Slack for critical issues like high error rate, service down, or resource exhaustion. All logs centralized with structured logging."

### Q6: "How do you handle database migrations in production?"
**Answer**: "EF Core migrations with:
1. Generate migration locally
2. Review SQL in migration file
3. Test on staging environment
4. Run migration as init container in Kubernetes
5. Backup database before migration
6. Rollback plan if migration fails
7. Monitor application after migration"

### Q7: "What's your disaster recovery strategy?"
**Answer**: "Multi-layered approach:
- **Backups**: Daily automated database backups to S3
- **Retention**: 30-day retention with point-in-time recovery
- **Testing**: Monthly restore tests to verify backups
- **Replication**: Multi-AZ RDS for high availability
- **Infrastructure**: Terraform state allows quick rebuild
- **Documentation**: Runbooks for common disaster scenarios"

### Q8: "How do you optimize Docker image sizes?"
**Answer**: "Several techniques:
- **Multi-stage builds**: Separate build and runtime stages
- **Minimal base images**: Alpine Linux where possible
- **Layer caching**: Order commands to maximize cache hits
- **.dockerignore**: Exclude unnecessary files
- **Combine commands**: Reduce layer count
- **Remove build dependencies**: Only keep runtime dependencies"

### Q9: "Explain your network security"
**Answer**: "Defense in depth:
- **Network Isolation**: Separate VPCs for different environments
- **Security Groups**: Whitelist only necessary ports
- **Private Subnets**: Databases in private subnets, no public access
- **TLS/SSL**: HTTPS for all external traffic
- **Secrets Encryption**: Encrypted at rest and in transit
- **WAF**: Web Application Firewall for API protection"

### Q10: "How would you improve the DevOps setup?"
**Answer**: "Several areas:
- **Service Mesh**: Istio for advanced traffic management
- **GitOps**: ArgoCD for declarative Kubernetes deployments
- **Observability**: Distributed tracing with Jaeger
- **Cost Optimization**: Spot instances for non-critical workloads
- **Multi-region**: Deploy to multiple AWS regions for disaster recovery
- **Chaos Engineering**: Regularly test failure scenarios"

---

## Key Takeaways

### Technologies Mastered:
- **Containerization**: Docker, Docker Compose
- **Orchestration**: Kubernetes, EKS
- **CI/CD**: GitHub Actions
- **Infrastructure**: Terraform, AWS
- **Monitoring**: Prometheus, Grafana
- **Databases**: TimescaleDB, Redis
- **Networking**: Nginx, ALB, Ingress

### Best Practices Implemented:
- Multi-stage Docker builds
- Health checks and readiness probes
- Resource limits and requests
- Automated backups
- Secrets management
- Monitoring and alerting
- Infrastructure as Code
- Zero-downtime deployments

### DevOps Principles:
- **Automation**: Everything automated via scripts/pipelines
- **Consistency**: Same process across all environments
- **Observability**: Comprehensive monitoring and logging
- **Security**: Defense in depth, least privilege
- **Scalability**: Horizontal scaling with load balancing
- **Reliability**: High availability, disaster recovery

---

**Remember**: You've built a production-ready DevOps pipeline with industry best practices. Be confident and specific with examples! 🚀

