# AquaControl Platform

A comprehensive aquaculture management system built with modern web technologies, featuring real-time monitoring, analytics, and production-ready security.

## Overview

AquaControl Platform is a full-stack aquaculture management solution designed for fish farming operations. It provides real-time monitoring of tank conditions, sensor management, comprehensive analytics, and a professional dashboard for operational oversight.

## Architecture

### Technology Stack

**Backend (.NET 8)**
- ASP.NET Core Web API with Clean Architecture
- Entity Framework Core with TimescaleDB for time-series data
- CQRS pattern with MediatR
- JWT authentication with refresh token rotation
- SignalR for real-time communication
- Comprehensive logging with Serilog
- Health checks and monitoring endpoints

**Frontend (Vue.js 3)**
- Vue.js 3 with Composition API and TypeScript
- Pinia for state management with persistence
- Element Plus UI component library
- Vite build system with hot module replacement
- PWA capabilities with service worker
- Responsive design with mobile support

**Infrastructure**
- Docker containerization with multi-stage builds
- TimescaleDB for time-series sensor data
- Redis for caching and session storage
- Kafka for event streaming (optional)
- Nginx reverse proxy with security headers
- Prometheus and Grafana for monitoring

## Features

### Core Functionality
- **Tank Management**: Complete CRUD operations for aquaculture tanks
- **Sensor Monitoring**: Real-time sensor data collection and visualization
- **Alert System**: Intelligent alerting with severity levels and resolution tracking
- **Analytics Dashboard**: Comprehensive insights into water quality, production, and performance
- **User Management**: Secure authentication with role-based access control

### Security Features
- JWT authentication with automatic token refresh
- Account lockout protection against brute force attacks
- Secure password hashing with salt
- Token blacklisting for secure logout
- IP address tracking and audit trails
- Security headers and CORS configuration
- Input validation and sanitization

### Monitoring & Observability
- Health checks for all services
- Structured logging with correlation IDs
- Performance monitoring and metrics
- Error tracking and reporting
- Real-time system status monitoring

## Quick Start

### Prerequisites
- Docker Engine 20.10+
- Docker Compose 2.0+
- 4GB+ RAM available
- Ports 5000, 5173, 5432, 6381 available

### Running with Docker

1. **Clone the repository**
   ```bash
   git clone https://github.com/saidulIslam1602/Aqua_Control_Platform.git
   cd Aqua_Control_Platform/AquaControl-Platform
   ```

2. **Start the services**
   ```bash
   # Start infrastructure services
   docker compose up -d timescaledb redis zookeeper kafka
   
   # Wait for services to be ready (30 seconds)
   sleep 30
   
   # Start application services
   docker compose up -d backend frontend
   ```

3. **Access the application**
   - Frontend: http://localhost:5173
   - Backend API: http://localhost:5000
   - API Documentation: http://localhost:5000/swagger

### Default Credentials
- **Username**: `admin`
- **Password**: `admin123`

## Service Endpoints

| Service | URL | Description |
|---------|-----|-------------|
| Frontend Application | http://localhost:5173 | Main web interface |
| Backend API | http://localhost:5000 | REST API endpoints |
| API Documentation | http://localhost:5000/swagger | Interactive API docs |
| Health Check | http://localhost:5000/health | Service health status |
| Database | localhost:5432 | TimescaleDB instance |
| Redis Cache | localhost:6381 | Redis cache server |

## API Endpoints

### Authentication
- `POST /api/auth/login` - User authentication
- `POST /api/auth/refresh` - Token refresh
- `POST /api/auth/logout` - User logout
- `GET /api/auth/validate` - Token validation
- `GET /api/auth/profile` - User profile

### Tank Management
- `GET /api/tanks` - List all tanks with pagination
- `POST /api/tanks` - Create new tank
- `GET /api/tanks/{id}` - Get tank details
- `PUT /api/tanks/{id}` - Update tank
- `DELETE /api/tanks/{id}` - Delete tank
- `POST /api/tanks/{id}/activate` - Activate tank
- `POST /api/tanks/{id}/deactivate` - Deactivate tank
- `POST /api/tanks/{id}/schedule-maintenance` - Schedule maintenance

## Project Structure

```
AquaControl-Platform/
├── backend/                 # .NET Core API
│   ├── src/
│   │   ├── AquaControl.API/           # Web API layer
│   │   ├── AquaControl.Application/   # Application services
│   │   ├── AquaControl.Domain/        # Domain models
│   │   └── AquaControl.Infrastructure/ # Data access
│   ├── Dockerfile.dev       # Development container
│   └── Dockerfile.prod      # Production container
├── frontend/                # Vue.js application
│   ├── src/
│   │   ├── components/      # Reusable components
│   │   ├── views/          # Page components
│   │   ├── stores/         # Pinia state management
│   │   ├── services/       # API services
│   │   └── types/          # TypeScript definitions
│   ├── Dockerfile.dev      # Development container
│   └── Dockerfile.prod     # Production container
├── docker/                 # Docker configurations
├── scripts/                # Utility scripts
├── secrets/                # Secret management (examples)
├── tests/                  # Test suites
├── docker-compose.yml      # Development orchestration
└── docker-compose.prod.yml # Production orchestration
```

## Development

### Local Development Setup

1. **Backend Development**
   ```bash
   cd backend/src/AquaControl.API
   dotnet restore
   dotnet run
   ```

2. **Frontend Development**
   ```bash
   cd frontend
   npm install
   npm run dev
   ```

3. **Database Setup**
   ```bash
   docker compose up -d timescaledb
   # Database will be automatically initialized
   ```

### Building for Production

1. **Build Docker Images**
   ```bash
   docker compose -f docker-compose.prod.yml build
   ```

2. **Deploy to Production**
   ```bash
   # Configure secrets first (see secrets/*.example files)
   docker compose -f docker-compose.prod.yml up -d
   ```

## Configuration

### Environment Variables

**Backend Configuration:**
- `ASPNETCORE_ENVIRONMENT`: Development/Production
- `ConnectionStrings__DefaultConnection`: Database connection
- `ConnectionStrings__Redis`: Redis connection
- `JwtSettings__SecretKey`: JWT signing key
- `JwtSettings__Issuer`: JWT issuer
- `JwtSettings__Audience`: JWT audience

**Frontend Configuration:**
- `VITE_API_BASE_URL`: Backend API URL
- `VITE_ENABLE_ANALYTICS`: Enable analytics features
- `VITE_ENABLE_REAL_TIME`: Enable real-time features

### Database Configuration

The application uses TimescaleDB (PostgreSQL extension) for:
- Time-series sensor data storage
- Automatic data compression and retention
- Optimized queries for time-based analytics
- Hypertable creation for sensor readings

## Monitoring

### Health Checks
- Application health: `/health`
- Database connectivity
- Redis connectivity
- External service dependencies

### Logging
- Structured logging with Serilog
- Request/response logging with correlation IDs
- Performance monitoring with execution times
- Error tracking with stack traces

## Security

### Authentication & Authorization
- JWT tokens with RS256 signing
- Refresh token rotation
- Account lockout protection
- Role-based access control
- IP address tracking

### Security Headers
- Content Security Policy (CSP)
- HTTP Strict Transport Security (HSTS)
- X-Frame-Options, X-Content-Type-Options
- Referrer Policy and Permissions Policy

## Testing

### Test Structure
```
tests/
├── unit/
│   ├── backend/           # .NET unit tests
│   └── frontend/          # Vue component tests
├── integration/
│   ├── api/              # API integration tests
│   └── database/         # Database tests
└── e2e/                  # End-to-end tests
```

### Running Tests
```bash
# Backend tests
cd backend && dotnet test

# Frontend tests
cd frontend && npm run test

# E2E tests
npm run test:e2e
```

## Deployment

### Development Deployment
```bash
# Quick start with Docker
docker compose up -d

# Manual setup
./scripts/dev-setup.sh
```

### Production Deployment
```bash
# Configure secrets
cp secrets/*.example secrets/
# Edit secret files with actual values

# Deploy with production configuration
docker compose -f docker-compose.prod.yml up -d
```

## Troubleshooting

### Common Issues

**Port Conflicts:**
```bash
# Check port usage
netstat -tulpn | grep :5173
netstat -tulpn | grep :5000
```

**Service Health:**
```bash
# Check service status
docker compose ps

# View logs
docker compose logs backend
docker compose logs frontend
```

**Database Issues:**
```bash
# Check database connectivity
docker compose exec timescaledb pg_isready -U aquacontrol

# Access database
docker compose exec timescaledb psql -U aquacontrol -d aquacontrol_dev
```

## Contributing

### Development Workflow
1. Create feature branch from main
2. Implement changes with tests
3. Ensure all tests pass
4. Update documentation if needed
5. Submit pull request

### Code Standards
- Follow Clean Architecture principles
- Use TypeScript for type safety
- Implement comprehensive error handling
- Add unit tests for new features
- Follow established naming conventions

## License

This project is licensed under the MIT License.

## Technical Specifications

### System Requirements
- **Minimum RAM**: 4GB
- **Recommended RAM**: 8GB+
- **Storage**: 20GB+ available space
- **Network**: Internet connection for external dependencies

### Supported Platforms
- **Development**: Windows, macOS, Linux
- **Production**: Linux (Docker containers)
- **Browsers**: Chrome 90+, Firefox 88+, Safari 14+, Edge 90+

### Performance Characteristics
- **API Response Time**: <200ms average
- **Frontend Load Time**: <3s initial load
- **Database Query Performance**: Optimized with indexes
- **Real-time Updates**: <100ms latency via SignalR

---

**Version**: 2.0.0  
**Last Updated**: November 2025
