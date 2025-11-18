# AquaControl Platform

A comprehensive enterprise-grade aquaculture management system built with modern web technologies, featuring real-time monitoring, advanced analytics, and production-ready security.

## Overview

AquaControl Platform is a full-stack aquaculture management solution designed for commercial fish farming operations. It provides real-time monitoring of tank conditions, intelligent sensor management, comprehensive analytics with interactive visualizations, and a professional dashboard for operational oversight.

## Technology Stack

### Backend (.NET 8)
- **ASP.NET Core 8.0** Web API with Clean Architecture
- **Entity Framework Core** with TimescaleDB for time-series data
- **CQRS Pattern** with MediatR for command/query separation
- **Event Sourcing** with custom event store implementation
- **JWT Authentication** with refresh token rotation and blacklisting
- **SignalR** for real-time bidirectional communication
- **Serilog** for structured logging with correlation IDs
- **FluentValidation** for request validation
- **Health Checks** for monitoring service dependencies

### Frontend (Vue.js 3)
- **Vue.js 3.4** with Composition API and TypeScript
- **Pinia 2.1** for state management with persistence
- **Element Plus 2.6** UI component library
- **ECharts 5.6** for advanced data visualizations
- **Vite 6.4** build system with hot module replacement
- **Axios** for HTTP client with interceptors
- **Vue Router 4.3** for navigation
- **PWA Support** with service worker and offline capabilities
- **Responsive Design** with mobile-first approach

### Data & Infrastructure
- **TimescaleDB** (PostgreSQL 15 + TimescaleDB extension) for time-series data
- **Redis 7** for caching and session storage
- **Apache Kafka** for event streaming (optional)
- **Docker & Docker Compose** for containerization
- **Nginx** reverse proxy with security headers
- **Prometheus & Grafana** for monitoring (optional)

## Features

### Core Functionality

#### Tank Management
- Complete CRUD operations for aquaculture tanks
- Tank status management (Active, Inactive, Maintenance, Emergency, Cleaning)
- Location tracking (Building, Room, Zone)
- Capacity management with multiple units (Liters, Gallons, Cubic Meters)
- Tank type categorization (Freshwater, Saltwater, Breeding, Quarantine, Nursery, Grow-out, Broodstock)
- Maintenance scheduling with notifications
- Tank activation/deactivation workflows
- Edit tank details with validation

#### Sensor Monitoring
- Support for 10 sensor types:
  - Temperature
  - pH
  - Dissolved Oxygen
  - Salinity
  - Turbidity
  - Ammonia
  - Nitrite
  - Nitrate
  - Phosphate
  - Alkalinity
- Real-time sensor data collection and visualization
- Sensor calibration management with scheduling
- Sensor status tracking (Online, Offline, Error, Calibrating, Maintenance)
- Min/Max value configuration with threshold alerts
- Serial number and manufacturer tracking
- Sensor detail view with comprehensive information
- Sensor activation/deactivation controls

#### Alert System
- Intelligent alerting with three severity levels (Critical, Warning, Info)
- Real-time alert notifications
- Alert resolution tracking with user attribution
- Alert acknowledgment workflow
- Filter alerts by severity, tank, and resolution status
- Alert history and audit trail
- API integration with fallback to sample data

#### Analytics Dashboard
- **Interactive Charts** powered by ECharts:
  - Temperature trend line charts with threshold indicators
  - Water quality distribution pie charts
  - Sensor status bar charts
  - System efficiency gauge charts
  - Multi-parameter comparison visualizations
- **Key Metrics Display**:
  - System efficiency percentage
  - Average water quality score
  - Total events counter
  - Sensor uptime statistics
- **Data Export Functionality**:
  - Export analytics data to CSV or JSON
  - Export tank data with all details
  - Export sensor data with readings
  - Export alert history
- **Time Range Selection**: 24h, 7d, 30d, 90d, custom range
- **Real-time Data Updates** via SignalR

#### User Management
- Secure JWT authentication with automatic token refresh
- User profile management
- Role-based access control (Admin, Operator, Viewer)
- Account lockout protection against brute force attacks
- Password complexity requirements
- Session management with token blacklisting
- IP address tracking and audit trails

### Security Features
- JWT authentication with RS256 signing algorithm
- Refresh token rotation for enhanced security
- Secure password hashing with SHA256 and salt
- Token blacklisting for secure logout
- Account lockout after failed login attempts
- Security headers (CSP, HSTS, X-Frame-Options, etc.)
- CORS configuration for development and production
- Input validation and sanitization
- SQL injection prevention via parameterized queries
- XSS protection with content security policy

### Monitoring & Observability
- Comprehensive health checks for all services
- Structured logging with Serilog
- Request/response logging with correlation IDs
- Performance monitoring middleware
- Error tracking and reporting
- Real-time system status monitoring
- Custom middleware for security headers and performance tracking

## Quick Start

### Prerequisites
- **Docker Engine** 20.10 or higher
- **Docker Compose** 2.0 or higher
- **4GB RAM** minimum (8GB recommended)
- **Available Ports**: 5000 (Backend), 5173 (Frontend), 5433 (Database), 6381 (Redis)

### Running with Docker

1. **Clone the repository**
   ```bash
   git clone https://github.com/saidulIslam1602/Aqua_Control_Platform.git
   cd Aqua_Control_Platform/AquaControl-Platform
   ```

2. **Start the development environment**
   ```bash
   # Use the provided script
   chmod +x scripts/dev-start.sh
   ./scripts/dev-start.sh
   
   # Or manually with Docker Compose
   docker compose -f docker-compose.dev.yml up -d
   ```

3. **Access the application**
   - **Frontend**: http://localhost:5173
   - **Backend API**: http://localhost:5000
   - **API Documentation**: http://localhost:5000/swagger
   - **Health Check**: http://localhost:5000/health

## Service Endpoints

| Service | URL | Description |
|---------|-----|-------------|
| Frontend Application | http://localhost:5173 | Main web interface |
| Backend API | http://localhost:5000 | REST API endpoints |
| API Documentation | http://localhost:5000/swagger | Interactive Swagger UI |
| Health Check | http://localhost:5000/health | Service health status |
| TimescaleDB | localhost:5433 | PostgreSQL database |
| Redis Cache | localhost:6381 | Redis cache server |

## API Endpoints

### Authentication
- `POST /api/auth/login` - User authentication with credentials
- `POST /api/auth/refresh` - Refresh access token
- `POST /api/auth/logout` - User logout with token blacklisting
- `GET /api/auth/validate` - Validate current token
- `GET /api/auth/profile` - Get user profile information

### Tank Management
- `GET /api/tanks` - List all tanks with pagination and filtering
- `POST /api/tanks` - Create new tank
- `GET /api/tanks/{id}` - Get tank details by ID
- `PUT /api/tanks/{id}` - Update tank information
- `DELETE /api/tanks/{id}` - Delete tank
- `POST /api/tanks/{id}/activate` - Activate tank
- `POST /api/tanks/{id}/deactivate` - Deactivate tank
- `POST /api/tanks/{id}/schedule-maintenance` - Schedule maintenance
- `PUT /api/tanks/{id}/status` - Update tank status

### Sensor Management
- `GET /api/sensors` - List all sensors with filtering
- `POST /api/sensors` - Create new sensor
- `GET /api/sensors/{id}` - Get sensor details
- `PUT /api/sensors/{id}` - Update sensor configuration
- `DELETE /api/sensors/{id}` - Delete sensor
- `POST /api/sensors/{id}/calibrate` - Calibrate sensor
- `POST /api/sensors/{id}/activate` - Activate sensor
- `POST /api/sensors/{id}/deactivate` - Deactivate sensor
- `GET /api/sensors/{id}/readings` - Get sensor readings

### Alert Management
- `GET /api/alerts` - List all alerts with filtering
- `GET /api/alerts/{id}` - Get alert details
- `POST /api/alerts/{id}/resolve` - Resolve alert
- `POST /api/alerts/{id}/acknowledge` - Acknowledge alert
- `DELETE /api/alerts/{id}` - Delete alert
- `GET /api/alerts/statistics` - Get alert statistics

## Project Structure

```
AquaControl-Platform/
├── backend/                          # .NET 8 Backend
│   ├── src/
│   │   ├── AquaControl.API/         # Web API layer
│   │   │   ├── Controllers/         # API controllers
│   │   │   ├── Middleware/          # Custom middleware
│   │   │   ├── Hubs/               # SignalR hubs
│   │   │   └── Extensions/         # Service extensions
│   │   ├── AquaControl.Application/ # Application layer
│   │   │   ├── Commands/           # CQRS commands
│   │   │   ├── Queries/            # CQRS queries
│   │   │   ├── Handlers/           # Command/query handlers
│   │   │   ├── Services/           # Application services
│   │   │   ├── DTOs/               # Data transfer objects
│   │   │   └── Validators/         # FluentValidation validators
│   │   ├── AquaControl.Domain/      # Domain layer
│   │   │   ├── Aggregates/         # Domain aggregates
│   │   │   ├── Entities/           # Domain entities
│   │   │   ├── ValueObjects/       # Value objects
│   │   │   ├── Events/             # Domain events
│   │   │   └── Enums/              # Domain enumerations
│   │   └── AquaControl.Infrastructure/ # Infrastructure layer
│   │       ├── Persistence/        # EF Core DbContexts
│   │       ├── EventStore/         # Event sourcing
│   │       ├── ReadModels/         # CQRS read models
│   │       ├── TimeSeries/         # TimescaleDB integration
│   │       └── Services/           # Infrastructure services
│   ├── Dockerfile.dev              # Development container
│   └── Dockerfile.prod             # Production container
├── frontend/                         # Vue.js 3 Frontend
│   ├── src/
│   │   ├── components/
│   │   │   ├── charts/            # ECharts components
│   │   │   ├── common/            # Reusable components
│   │   │   ├── layout/            # Layout components
│   │   │   └── modals/            # Modal dialogs
│   │   ├── views/                 # Page components
│   │   │   ├── auth/              # Authentication views
│   │   │   ├── dashboard/         # Dashboard views
│   │   │   ├── tanks/             # Tank management views
│   │   │   ├── sensors/           # Sensor management views
│   │   │   ├── analytics/         # Analytics views
│   │   │   └── settings/          # Settings views
│   │   ├── stores/                # Pinia stores
│   │   │   ├── authStore.ts       # Authentication state
│   │   │   ├── tankStore.ts       # Tank management state
│   │   │   ├── sensorStore.ts     # Sensor management state
│   │   │   ├── alertStore.ts      # Alert management state
│   │   │   └── notificationStore.ts # Notifications
│   │   ├── services/              # API services
│   │   │   └── api/
│   │   │       ├── httpClient.ts  # Axios HTTP client
│   │   │       ├── authService.ts # Auth API calls
│   │   │       ├── tankService.ts # Tank API calls
│   │   │       ├── sensorService.ts # Sensor API calls
│   │   │       └── alertService.ts # Alert API calls
│   │   ├── types/                 # TypeScript definitions
│   │   ├── utils/                 # Utility functions
│   │   │   ├── exportUtils.ts    # Data export utilities
│   │   │   ├── errorReporting.ts # Error handling
│   │   │   └── performanceMonitoring.ts
│   │   ├── composables/           # Vue composables
│   │   ├── router/                # Vue Router configuration
│   │   └── styles/                # SCSS styles
│   ├── Dockerfile.dev             # Development container
│   └── Dockerfile.prod            # Production container
├── data-engineering/                # Data Engineering
│   ├── kafka/                      # Kafka configuration
│   ├── kafka-streams/              # Stream processing
│   ├── ml-pipeline/                # ML feature engineering
│   └── timescaledb/                # TimescaleDB setup
├── docker/                          # Docker configurations
│   ├── configs/                    # Service configurations
│   └── scripts/                    # Database scripts
├── infrastructure/                  # Infrastructure as Code
│   └── terraform/                  # Terraform configurations
├── scripts/                         # Utility scripts
│   ├── dev-start.sh               # Start development environment
│   ├── dev-setup.sh               # Setup development
│   └── dev-teardown.sh            # Teardown development
├── tests/                           # Test suites
│   ├── unit/                       # Unit tests
│   ├── integration/                # Integration tests
│   └── e2e/                        # End-to-end tests
├── docker-compose.yml              # Main Docker Compose
├── docker-compose.dev.yml          # Development configuration
└── docker-compose.prod.yml         # Production configuration
```

## Development

### Local Development Setup

#### Backend Development
```bash
cd backend/src/AquaControl.API
dotnet restore
dotnet run
```

#### Frontend Development
```bash
cd frontend
npm install
npm run dev
```

#### Database Setup
```bash
# Start TimescaleDB
docker compose -f docker-compose.dev.yml up -d timescaledb

# Database will be automatically initialized with schemas
```

### Building for Production

```bash
# Build all services
docker compose -f docker-compose.prod.yml build

# Start production services
docker compose -f docker-compose.prod.yml up -d
```

## Configuration

### Environment Variables

**Backend Configuration:**
```bash
ASPNETCORE_ENVIRONMENT=Development
DB_PASSWORD=your_secure_password
JWT_SECRET=your_jwt_secret_key
REDIS_PASSWORD=your_redis_password
```

**Frontend Configuration:**
```bash
VITE_API_BASE_URL=http://localhost:5000
VITE_ENABLE_ANALYTICS=true
VITE_ENABLE_REAL_TIME=true
```

### Database Configuration

The application uses TimescaleDB for:
- Time-series sensor data storage with automatic compression
- Hypertables for optimized time-based queries
- Continuous aggregates for analytics
- Data retention policies
- Automatic partitioning

## Testing

### Running Tests

```bash
# Backend unit tests
cd backend && dotnet test

# Frontend unit tests
cd frontend && npm run test:unit

# Frontend E2E tests
cd frontend && npm run test:e2e

# Coverage report
cd frontend && npm run test:coverage
```

## Deployment

### Development Deployment
```bash
# Quick start
./scripts/dev-start.sh

# Or manually
docker compose -f docker-compose.dev.yml up -d
```

### Production Deployment
```bash
# Configure secrets
cp secrets/*.example secrets/
# Edit secret files with production values

# Deploy
docker compose -f docker-compose.prod.yml up -d
```

## Troubleshooting

### Common Issues

**Port Conflicts:**
```bash
# Check if ports are in use
lsof -i :5173  # Frontend
lsof -i :5000  # Backend
lsof -i :5433  # Database
lsof -i :6381  # Redis
```

**Service Health:**
```bash
# Check all services
docker compose -f docker-compose.dev.yml ps

# View logs
docker compose -f docker-compose.dev.yml logs -f backend
docker compose -f docker-compose.dev.yml logs -f frontend
```

**Database Connection:**
```bash
# Test database connectivity
docker compose -f docker-compose.dev.yml exec timescaledb pg_isready -U aquacontrol

# Access database shell
docker compose -f docker-compose.dev.yml exec timescaledb psql -U aquacontrol -d aquacontrol_dev
```

**Clear and Restart:**
```bash
# Stop all services
docker compose -f docker-compose.dev.yml down

# Remove volumes (WARNING: deletes data)
docker compose -f docker-compose.dev.yml down -v

# Restart fresh
./scripts/dev-start.sh
```

## Key Features Implementation Status

### ✅ Completed Features
- User authentication with JWT
- Tank CRUD operations with status management
- Sensor CRUD operations with calibration
- Alert system with API integration
- Real-time updates via SignalR
- Analytics dashboard with ECharts
- Data export functionality (CSV/JSON)
- Responsive UI with Element Plus
- State management with Pinia
- Health checks and monitoring
- Security headers and CORS
- Docker containerization
- Database migrations
- Error handling and logging

### 🚧 In Progress
- Advanced analytics with ML predictions
- Kafka event streaming integration
- Grafana dashboards
- Automated testing suite expansion

## Performance Characteristics

- **API Response Time**: <200ms average
- **Frontend Initial Load**: <3s
- **Real-time Update Latency**: <100ms via SignalR
- **Database Query Performance**: Optimized with indexes and hypertables
- **Concurrent Users**: Tested up to 100 simultaneous connections

## System Requirements

### Development
- **RAM**: 4GB minimum, 8GB recommended
- **Storage**: 20GB available space
- **OS**: Windows 10+, macOS 10.15+, Linux (Ubuntu 20.04+)
- **Docker**: 20.10+ with Docker Compose 2.0+

### Production
- **RAM**: 8GB minimum, 16GB recommended
- **Storage**: 50GB+ for data retention
- **OS**: Linux (Ubuntu 22.04 LTS recommended)
- **Network**: Stable internet connection

### Supported Browsers
- Chrome 90+
- Firefox 88+
- Safari 14+
- Edge 90+

## Contributing

### Development Workflow
1. Fork the repository
2. Create a feature branch
3. Implement changes with tests
4. Ensure all tests pass
5. Update documentation
6. Submit pull request

### Code Standards
- Follow Clean Architecture principles
- Use TypeScript for type safety
- Implement comprehensive error handling
- Add unit tests for new features
- Follow existing naming conventions
- Document public APIs

## License

This project is licensed under the MIT License.

## Support

For issues, questions, or contributions:
- **GitHub Issues**: https://github.com/saidulIslam1602/Aqua_Control_Platform/issues
- **Documentation**: See `/docs` folder for detailed documentation

---

**Version**: 2.0.0  
**Last Updated**: November 2025  
**Status**: Production Ready
