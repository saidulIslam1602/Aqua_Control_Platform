#!/bin/bash

# AquaControl Platform Database Backup Script
set -e

# Configuration
DB_HOST="timescaledb"
DB_PORT="5432"
DB_NAME="aquacontrol_prod"
DB_USER="aquacontrol"
BACKUP_DIR="/backups"
DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_FILE="aquacontrol_backup_${DATE}.sql"
RETENTION_DAYS=30

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Logging function
log() {
    echo -e "${GREEN}[$(date +'%Y-%m-%d %H:%M:%S')]${NC} $1"
}

error() {
    echo -e "${RED}[$(date +'%Y-%m-%d %H:%M:%S')] ERROR:${NC} $1" >&2
}

warning() {
    echo -e "${YELLOW}[$(date +'%Y-%m-%d %H:%M:%S')] WARNING:${NC} $1"
}

# Wait for database to be ready
wait_for_db() {
    log "Waiting for database to be ready..."
    until pg_isready -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME"; do
        sleep 5
    done
    log "Database is ready"
}

# Create backup
create_backup() {
    log "Starting backup of database: $DB_NAME"
    
    # Create backup directory if it doesn't exist
    mkdir -p "$BACKUP_DIR"
    
    # Create the backup
    pg_dump -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" \
        --verbose \
        --clean \
        --if-exists \
        --create \
        --format=custom \
        --compress=9 \
        --file="$BACKUP_DIR/$BACKUP_FILE.custom"
    
    # Also create a plain SQL backup for easier inspection
    pg_dump -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" -d "$DB_NAME" \
        --verbose \
        --clean \
        --if-exists \
        --create \
        --format=plain \
        > "$BACKUP_DIR/$BACKUP_FILE"
    
    # Compress the plain SQL backup
    gzip "$BACKUP_DIR/$BACKUP_FILE"
    
    log "Backup completed: $BACKUP_FILE.gz and $BACKUP_FILE.custom"
}

# Verify backup
verify_backup() {
    log "Verifying backup integrity..."
    
    # Check if files exist and are not empty
    if [[ -f "$BACKUP_DIR/$BACKUP_FILE.gz" && -s "$BACKUP_DIR/$BACKUP_FILE.gz" ]]; then
        log "Plain SQL backup verified"
    else
        error "Plain SQL backup verification failed"
        exit 1
    fi
    
    if [[ -f "$BACKUP_DIR/$BACKUP_FILE.custom" && -s "$BACKUP_DIR/$BACKUP_FILE.custom" ]]; then
        log "Custom format backup verified"
    else
        error "Custom format backup verification failed"
        exit 1
    fi
    
    # Test the custom backup can be read
    if pg_restore --list "$BACKUP_DIR/$BACKUP_FILE.custom" > /dev/null 2>&1; then
        log "Custom backup format is valid"
    else
        error "Custom backup format validation failed"
        exit 1
    fi
}

# Cleanup old backups
cleanup_old_backups() {
    log "Cleaning up backups older than $RETENTION_DAYS days..."
    
    find "$BACKUP_DIR" -name "aquacontrol_backup_*.sql.gz" -mtime +$RETENTION_DAYS -delete
    find "$BACKUP_DIR" -name "aquacontrol_backup_*.custom" -mtime +$RETENTION_DAYS -delete
    
    log "Cleanup completed"
}

# Generate backup report
generate_report() {
    local backup_size_gz=$(du -h "$BACKUP_DIR/$BACKUP_FILE.gz" | cut -f1)
    local backup_size_custom=$(du -h "$BACKUP_DIR/$BACKUP_FILE.custom" | cut -f1)
    local total_backups=$(ls -1 "$BACKUP_DIR"/aquacontrol_backup_*.gz 2>/dev/null | wc -l)
    
    log "=== Backup Report ==="
    log "Date: $(date)"
    log "Database: $DB_NAME"
    log "Backup files:"
    log "  - $BACKUP_FILE.gz ($backup_size_gz)"
    log "  - $BACKUP_FILE.custom ($backup_size_custom)"
    log "Total backups in directory: $total_backups"
    log "===================="
}

# Send notification (if configured)
send_notification() {
    local status=$1
    local message=$2
    
    # This could be extended to send notifications via email, Slack, etc.
    if [[ -n "$WEBHOOK_URL" ]]; then
        curl -X POST "$WEBHOOK_URL" \
            -H "Content-Type: application/json" \
            -d "{\"text\":\"AquaControl Backup $status: $message\"}" \
            > /dev/null 2>&1 || warning "Failed to send notification"
    fi
}

# Main execution
main() {
    log "Starting AquaControl database backup process"
    
    # Check if running in production mode
    if [[ "$ASPNETCORE_ENVIRONMENT" != "Production" ]]; then
        warning "Not running in production environment"
    fi
    
    # Wait for database
    wait_for_db
    
    # Create backup
    if create_backup; then
        verify_backup
        cleanup_old_backups
        generate_report
        send_notification "SUCCESS" "Database backup completed successfully"
        log "Backup process completed successfully"
    else
        error "Backup process failed"
        send_notification "FAILED" "Database backup failed"
        exit 1
    fi
}

# Handle script interruption
trap 'error "Backup process interrupted"; exit 1' INT TERM

# Run main function
main "$@"
