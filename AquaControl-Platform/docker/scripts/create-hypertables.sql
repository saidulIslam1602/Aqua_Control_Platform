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

