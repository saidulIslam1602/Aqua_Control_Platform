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

