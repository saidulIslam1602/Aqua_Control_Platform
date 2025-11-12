# AquaControl Platform - Production Deployment Guide

## Overview

This guide covers the production deployment of the AquaControl Platform, a comprehensive aquaculture management system built with industry-standard practices and security measures.

## Architecture

### Technology Stack

**Backend:**
- .NET 8.0 with ASP.NET Core
- Clean Architecture with CQRS pattern
- Entity Framework Core with TimescaleDB
- SignalR for real-time communication
- JWT authentication with refresh tokens
- Comprehensive logging with Serilog
- Health checks and monitoring
- Rate limiting and security headers

**Frontend:**
- Vue.js 3 with TypeScript
- Pinia for state management
- Element Plus UI components
- PWA capabilities
- Error boundaries and performance monitoring
- Responsive design with accessibility

**Infrastructure:**
- Docker containerization
- Nginx reverse proxy with security headers
- TimescaleDB for time-series data
- Redis for caching and sessions
- Prometheus and Grafana for monitoring
- Automated backups

## Production Features

### Security
- ✅ HTTPS enforcement with HSTS
- ✅ Security headers (CSP, CSRF protection)
- ✅ Rate limiting on API endpoints
- ✅ Input validation and sanitization
- ✅ JWT tokens with secure configuration
- ✅ Secrets management
- ✅ Non-root container execution

### Performance
- ✅ Database connection pooling
- ✅ Redis caching layer
- ✅ Gzip compression
- ✅ Static file optimization
- ✅ Database indexing and optimization
- ✅ CDN-ready asset structure

### Reliability
- ✅ Health checks for all services
- ✅ Graceful degradation
- ✅ Circuit breaker patterns
- ✅ Retry mechanisms with exponential backoff
- ✅ Comprehensive error handling
- ✅ Automated database backups

### Monitoring & Observability
- ✅ Structured logging with correlation IDs
- ✅ Performance metrics collection
- ✅ Error tracking and reporting
- ✅ Real-time dashboards
- ✅ Alerting on critical metrics
- ✅ Distributed tracing support

## Prerequisites

### System Requirements
- Docker Engine 20.10+
- Docker Compose 2.0+
- Minimum 4GB RAM
- 20GB available disk space
- SSL certificates (for HTTPS)

### Domain Setup
- Primary domain: `aquacontrol.com`
- API subdomain: `api.aquacontrol.com`
- Monitoring: `monitoring.aquacontrol.com`

## Deployment Steps

### 1. Environment Preparation

```bash
# Clone the repository
git clone https://github.com/your-org/aquacontrol-platform.git
cd aquacontrol-platform

# Create production environment
cp .env.example .env.production
```

### 2. Secrets Configuration

```bash
# Create actual secret files from examples
cp secrets/db_password.txt.example secrets/db_password.txt
cp secrets/redis_password.txt.example secrets/redis_password.txt
cp secrets/jwt_secret.txt.example secrets/jwt_secret.txt
cp secrets/grafana_password.txt.example secrets/grafana_password.txt

# Generate secure passwords
openssl rand -base64 32 > secrets/db_password.txt
openssl rand -base64 32 > secrets/redis_password.txt
openssl rand -base64 64 > secrets/jwt_secret.txt
openssl rand -base64 32 > secrets/grafana_password.txt

# Secure the secrets directory
chmod 600 secrets/*.txt
```

### 3. SSL Certificate Setup

```bash
# Create SSL directory
mkdir -p ssl

# Copy your SSL certificates
cp /path/to/your/certificate.crt ssl/
cp /path/to/your/private.key ssl/
cp /path/to/your/ca-bundle.crt ssl/

# Or use Let's Encrypt
certbot certonly --standalone -d aquacontrol.com -d api.aquacontrol.com
cp /etc/letsencrypt/live/aquacontrol.com/* ssl/
```

### 4. Database Initialization

```bash
# Start only the database first
docker-compose -f docker-compose.prod.yml up -d timescaledb

# Wait for database to be ready
docker-compose -f docker-compose.prod.yml exec timescaledb pg_isready -U aquacontrol

# Run database migrations (if needed)
docker-compose -f docker-compose.prod.yml run --rm backend dotnet ef database update
```

### 5. Production Deployment

```bash
# Build and start all services
docker-compose -f docker-compose.prod.yml up -d

# Verify all services are healthy
docker-compose -f docker-compose.prod.yml ps

# Check logs
docker-compose -f docker-compose.prod.yml logs -f
```

### 6. Verification

```bash
# Test API health
curl -f https://api.aquacontrol.com/health

# Test frontend
curl -f https://aquacontrol.com/health

# Test authentication
curl -X POST https://api.aquacontrol.com/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}'
```

## Configuration

### Environment Variables

**Backend (.env.production):**
```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=https://+:5001;http://+:5000
ConnectionStrings__DefaultConnection=Host=timescaledb;Database=aquacontrol_prod;Username=aquacontrol
JwtSettings__Issuer=AquaControl.API
JwtSettings__Audience=AquaControl.Client
```

**Frontend (env.production):**
```bash
VITE_API_BASE_URL=https://api.aquacontrol.com
VITE_ENABLE_ANALYTICS=true
VITE_ENABLE_ERROR_REPORTING=true
```

### Database Configuration

**TimescaleDB Production Settings:**
- Shared buffers: 512MB (25% of RAM)
- Effective cache size: 2GB (75% of RAM)
- Work memory: 32MB
- Maintenance work memory: 256MB
- Max connections: 200

### Nginx Configuration

Key security headers and optimizations:
- HSTS with 1-year max-age
- Content Security Policy
- Rate limiting (10 req/s for API, 1 req/s for auth)
- Gzip compression for static assets
- SSL/TLS configuration with modern ciphers

## Monitoring

### Prometheus Metrics

Available at `https://monitoring.aquacontrol.com:9090`

**Key Metrics:**
- HTTP request duration and count
- Database connection pool status
- Memory and CPU usage
- Error rates by endpoint
- Cache hit/miss ratios

### Grafana Dashboards

Available at `https://monitoring.aquacontrol.com:3000`

**Default Dashboards:**
- Application Overview
- Database Performance
- Infrastructure Metrics
- Error Tracking
- User Activity

### Alerting Rules

**Critical Alerts:**
- API response time > 2 seconds
- Error rate > 5%
- Database connections > 80%
- Disk usage > 85%
- Memory usage > 90%

## Backup & Recovery

### Automated Backups

Backups run daily at 2 AM UTC:
```bash
# Manual backup
docker-compose -f docker-compose.prod.yml run --rm backup

# Restore from backup
docker-compose -f docker-compose.prod.yml exec timescaledb \
  pg_restore -U aquacontrol -d aquacontrol_prod /backups/backup_file.custom
```

### Backup Strategy
- Daily full database backups
- 30-day retention policy
- Compressed storage (gzip)
- Both custom and SQL formats
- Automatic cleanup of old backups

## Security Considerations

### Network Security
- All services run in isolated Docker network
- No direct database access from outside
- API rate limiting enabled
- CORS properly configured

### Application Security
- Input validation on all endpoints
- SQL injection prevention
- XSS protection headers
- CSRF tokens for state-changing operations
- Secure JWT configuration

### Infrastructure Security
- Non-root containers
- Read-only file systems where possible
- Secrets stored securely
- Regular security updates

## Performance Optimization

### Database Optimization
```sql
-- Create indexes for common queries
CREATE INDEX CONCURRENTLY idx_tanks_status ON tanks(status);
CREATE INDEX CONCURRENTLY idx_sensor_readings_timestamp ON sensor_readings(timestamp DESC);

-- Analyze tables regularly
ANALYZE tanks;
ANALYZE sensor_readings;
```

### Caching Strategy
- Redis for session storage
- API response caching (5-minute TTL)
- Static asset caching (1-year TTL)
- Database query result caching

### Frontend Optimization
- Code splitting and lazy loading
- Service worker for offline functionality
- Image optimization and lazy loading
- Bundle size monitoring

## Troubleshooting

### Common Issues

**Database Connection Issues:**
```bash
# Check database status
docker-compose -f docker-compose.prod.yml exec timescaledb pg_isready

# Check connection string
docker-compose -f docker-compose.prod.yml logs backend | grep -i connection
```

**High Memory Usage:**
```bash
# Check container memory usage
docker stats

# Restart services if needed
docker-compose -f docker-compose.prod.yml restart backend
```

**SSL Certificate Issues:**
```bash
# Verify certificate validity
openssl x509 -in ssl/certificate.crt -text -noout

# Check certificate expiration
openssl x509 -in ssl/certificate.crt -noout -dates
```

### Log Analysis

**Backend Logs:**
```bash
# View structured logs
docker-compose -f docker-compose.prod.yml logs backend | jq '.'

# Filter error logs
docker-compose -f docker-compose.prod.yml logs backend | grep -i error
```

**Database Logs:**
```bash
# View slow queries
docker-compose -f docker-compose.prod.yml exec timescaledb \
  tail -f /var/lib/postgresql/data/log/postgresql-*.log
```

## Maintenance

### Regular Tasks

**Daily:**
- Monitor system health dashboards
- Check backup completion
- Review error logs

**Weekly:**
- Update security patches
- Analyze performance metrics
- Review and rotate logs

**Monthly:**
- Update dependencies
- Review and update monitoring alerts
- Capacity planning review

### Updates and Rollbacks

**Application Updates:**
```bash
# Pull latest images
docker-compose -f docker-compose.prod.yml pull

# Rolling update
docker-compose -f docker-compose.prod.yml up -d --no-deps backend
docker-compose -f docker-compose.prod.yml up -d --no-deps frontend

# Rollback if needed
docker-compose -f docker-compose.prod.yml down
docker-compose -f docker-compose.prod.yml up -d
```

## Support

### Documentation
- API Documentation: `https://api.aquacontrol.com/swagger`
- User Guide: `https://docs.aquacontrol.com`
- Architecture Decision Records: `/docs/adr/`

### Contact Information
- Technical Support: support@aquacontrol.com
- Emergency Contact: +1-555-0123
- Status Page: https://status.aquacontrol.com

---

**Last Updated:** $(date)
**Version:** 2.0.0
**Environment:** Production
