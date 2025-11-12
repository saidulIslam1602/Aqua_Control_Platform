-- Enable TimescaleDB extension
CREATE EXTENSION IF NOT EXISTS timescaledb;

-- Create schemas
CREATE SCHEMA IF NOT EXISTS sensor_data;
CREATE SCHEMA IF NOT EXISTS analytics;
CREATE SCHEMA IF NOT EXISTS ml_features;

-- Sensor readings table (hypertable)
CREATE TABLE sensor_data.readings (
    time TIMESTAMPTZ NOT NULL,
    sensor_id UUID NOT NULL,
    tank_id UUID NOT NULL,
    sensor_type TEXT NOT NULL,
    value DOUBLE PRECISION NOT NULL,
    quality_score DOUBLE PRECISION NOT NULL DEFAULT 1.0,
    metadata JSONB,
    created_at TIMESTAMPTZ DEFAULT NOW()
);

-- Convert to hypertable
SELECT create_hypertable('sensor_data.readings', 'time', chunk_time_interval => INTERVAL '1 hour');

-- Create indexes for performance
CREATE INDEX idx_readings_sensor_id_time ON sensor_data.readings (sensor_id, time DESC);
CREATE INDEX idx_readings_tank_id_time ON sensor_data.readings (tank_id, time DESC);
CREATE INDEX idx_readings_sensor_type_time ON sensor_data.readings (sensor_type, time DESC);
CREATE INDEX idx_readings_quality ON sensor_data.readings (quality_score) WHERE quality_score < 0.8;

-- Sensor metadata table
CREATE TABLE sensor_data.sensors (
    id UUID PRIMARY KEY,
    tank_id UUID NOT NULL,
    sensor_type TEXT NOT NULL,
    model TEXT NOT NULL,
    manufacturer TEXT NOT NULL,
    serial_number TEXT UNIQUE NOT NULL,
    installation_date TIMESTAMPTZ NOT NULL,
    calibration_date TIMESTAMPTZ,
    next_calibration_date TIMESTAMPTZ,
    min_value DOUBLE PRECISION,
    max_value DOUBLE PRECISION,
    accuracy DOUBLE PRECISION NOT NULL,
    is_active BOOLEAN DEFAULT true,
    metadata JSONB,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

-- Tank metadata table
CREATE TABLE sensor_data.tanks (
    id UUID PRIMARY KEY,
    name TEXT NOT NULL UNIQUE,
    capacity_value DOUBLE PRECISION NOT NULL,
    capacity_unit TEXT NOT NULL,
    tank_type TEXT NOT NULL,
    location JSONB NOT NULL,
    status TEXT NOT NULL DEFAULT 'Inactive',
    optimal_parameters JSONB,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

-- Alerts table
CREATE TABLE sensor_data.alerts (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tank_id UUID NOT NULL,
    sensor_id UUID,
    alert_type TEXT NOT NULL,
    severity TEXT NOT NULL,
    message TEXT NOT NULL,
    threshold_value DOUBLE PRECISION,
    actual_value DOUBLE PRECISION,
    is_resolved BOOLEAN DEFAULT false,
    resolved_at TIMESTAMPTZ,
    resolved_by TEXT,
    metadata JSONB,
    created_at TIMESTAMPTZ DEFAULT NOW()
);

-- Create hypertable for alerts
SELECT create_hypertable('sensor_data.alerts', 'created_at', chunk_time_interval => INTERVAL '1 day');

-- Analytics aggregation tables
CREATE TABLE analytics.hourly_sensor_stats (
    time TIMESTAMPTZ NOT NULL,
    sensor_id UUID NOT NULL,
    tank_id UUID NOT NULL,
    sensor_type TEXT NOT NULL,
    avg_value DOUBLE PRECISION,
    min_value DOUBLE PRECISION,
    max_value DOUBLE PRECISION,
    stddev_value DOUBLE PRECISION,
    count_readings INTEGER,
    avg_quality DOUBLE PRECISION,
    anomaly_count INTEGER DEFAULT 0
);

SELECT create_hypertable('analytics.hourly_sensor_stats', 'time', chunk_time_interval => INTERVAL '1 day');

CREATE TABLE analytics.daily_tank_summary (
    date DATE NOT NULL,
    tank_id UUID NOT NULL,
    avg_temperature DOUBLE PRECISION,
    avg_ph DOUBLE PRECISION,
    avg_oxygen DOUBLE PRECISION,
    avg_salinity DOUBLE PRECISION,
    min_temperature DOUBLE PRECISION,
    max_temperature DOUBLE PRECISION,
    temperature_variance DOUBLE PRECISION,
    ph_stability_score DOUBLE PRECISION,
    oxygen_critical_events INTEGER DEFAULT 0,
    total_readings INTEGER,
    data_quality_score DOUBLE PRECISION,
    alert_count INTEGER DEFAULT 0,
    maintenance_events INTEGER DEFAULT 0
);

SELECT create_hypertable('analytics.daily_tank_summary', 'date', chunk_time_interval => INTERVAL '1 month');

-- ML Features table
CREATE TABLE ml_features.tank_features (
    feature_timestamp TIMESTAMPTZ NOT NULL,
    tank_id UUID NOT NULL,
    features JSONB NOT NULL,
    feature_version TEXT NOT NULL DEFAULT 'v1.0',
    created_at TIMESTAMPTZ DEFAULT NOW()
);

SELECT create_hypertable('ml_features.tank_features', 'feature_timestamp', chunk_time_interval => INTERVAL '1 day');

-- Model predictions table
CREATE TABLE ml_features.predictions (
    prediction_timestamp TIMESTAMPTZ NOT NULL,
    tank_id UUID NOT NULL,
    model_name TEXT NOT NULL,
    model_version TEXT NOT NULL,
    prediction_type TEXT NOT NULL,
    prediction_value DOUBLE PRECISION,
    confidence_score DOUBLE PRECISION,
    prediction_horizon INTERVAL,
    features_used JSONB,
    created_at TIMESTAMPTZ DEFAULT NOW()
);

SELECT create_hypertable('ml_features.predictions', 'prediction_timestamp', chunk_time_interval => INTERVAL '1 day');

