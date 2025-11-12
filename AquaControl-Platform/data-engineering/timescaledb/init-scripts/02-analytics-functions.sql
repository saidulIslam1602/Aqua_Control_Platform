-- Real-time anomaly detection function
CREATE OR REPLACE FUNCTION sensor_data.detect_anomalies(
    p_sensor_id UUID,
    p_window_hours INTEGER DEFAULT 24,
    p_threshold_multiplier DOUBLE PRECISION DEFAULT 3.0
)
RETURNS TABLE(
    time TIMESTAMPTZ,
    value DOUBLE PRECISION,
    z_score DOUBLE PRECISION,
    is_anomaly BOOLEAN
) AS $$
BEGIN
    RETURN QUERY
    WITH stats AS (
        SELECT 
            AVG(r.value) as mean_value,
            STDDEV(r.value) as stddev_value
        FROM sensor_data.readings r
        WHERE r.sensor_id = p_sensor_id
        AND r.time >= NOW() - (p_window_hours || ' hours')::INTERVAL
        AND r.quality_score >= 0.7
    ),
    readings_with_zscore AS (
        SELECT 
            r.time,
            r.value,
            CASE 
                WHEN s.stddev_value > 0 THEN 
                    ABS(r.value - s.mean_value) / s.stddev_value
                ELSE 0
            END as z_score
        FROM sensor_data.readings r
        CROSS JOIN stats s
        WHERE r.sensor_id = p_sensor_id
        AND r.time >= NOW() - INTERVAL '1 hour'
        AND r.quality_score >= 0.7
    )
    SELECT 
        rz.time,
        rz.value,
        rz.z_score,
        rz.z_score > p_threshold_multiplier as is_anomaly
    FROM readings_with_zscore rz
    ORDER BY rz.time DESC;
END;
$$ LANGUAGE plpgsql;

-- Tank health scoring function
CREATE OR REPLACE FUNCTION analytics.calculate_tank_health_score(
    p_tank_id UUID,
    p_hours INTEGER DEFAULT 24
)
RETURNS DOUBLE PRECISION AS $$
DECLARE
    health_score DOUBLE PRECISION := 0;
    temp_score DOUBLE PRECISION := 0;
    ph_score DOUBLE PRECISION := 0;
    oxygen_score DOUBLE PRECISION := 0;
    quality_score DOUBLE PRECISION := 0;
    alert_penalty DOUBLE PRECISION := 0;
BEGIN
    -- Temperature stability score (0-25 points)
    SELECT 
        GREATEST(0, 25 - (STDDEV(value) * 5))
    INTO temp_score
    FROM sensor_data.readings r
    WHERE r.tank_id = p_tank_id
    AND r.sensor_type = 'Temperature'
    AND r.time >= NOW() - (p_hours || ' hours')::INTERVAL
    AND r.quality_score >= 0.7;

    -- pH stability score (0-25 points)
    SELECT 
        GREATEST(0, 25 - (STDDEV(value) * 10))
    INTO ph_score
    FROM sensor_data.readings r
    WHERE r.tank_id = p_tank_id
    AND r.sensor_type = 'pH'
    AND r.time >= NOW() - (p_hours || ' hours')::INTERVAL
    AND r.quality_score >= 0.7;

    -- Oxygen sufficiency score (0-25 points)
    SELECT 
        CASE 
            WHEN MIN(value) >= 6.0 THEN 25
            WHEN MIN(value) >= 4.0 THEN 15
            WHEN MIN(value) >= 2.0 THEN 5
            ELSE 0
        END
    INTO oxygen_score
    FROM sensor_data.readings r
    WHERE r.tank_id = p_tank_id
    AND r.sensor_type = 'DissolvedOxygen'
    AND r.time >= NOW() - (p_hours || ' hours')::INTERVAL
    AND r.quality_score >= 0.7;

    -- Data quality score (0-25 points)
    SELECT 
        AVG(quality_score) * 25
    INTO quality_score
    FROM sensor_data.readings r
    WHERE r.tank_id = p_tank_id
    AND r.time >= NOW() - (p_hours || ' hours')::INTERVAL;

    -- Alert penalty (subtract points for recent alerts)
    SELECT 
        COUNT(*) * 5
    INTO alert_penalty
    FROM sensor_data.alerts a
    WHERE a.tank_id = p_tank_id
    AND a.created_at >= NOW() - (p_hours || ' hours')::INTERVAL
    AND NOT a.is_resolved;

    health_score := GREATEST(0, 
        COALESCE(temp_score, 0) + 
        COALESCE(ph_score, 0) + 
        COALESCE(oxygen_score, 0) + 
        COALESCE(quality_score, 0) - 
        COALESCE(alert_penalty, 0)
    );

    RETURN LEAST(100, health_score);
END;
$$ LANGUAGE plpgsql;

-- Predictive maintenance scoring
CREATE OR REPLACE FUNCTION analytics.calculate_maintenance_score(
    p_tank_id UUID,
    p_days INTEGER DEFAULT 7
)
RETURNS JSONB AS $$
DECLARE
    result JSONB := '{}';
    sensor_degradation DOUBLE PRECISION := 0;
    reading_frequency DOUBLE PRECISION := 0;
    anomaly_frequency DOUBLE PRECISION := 0;
    maintenance_score DOUBLE PRECISION := 0;
BEGIN
    -- Calculate sensor degradation (quality score trend)
    WITH quality_trend AS (
        SELECT 
            date_trunc('day', time) as day,
            AVG(quality_score) as daily_quality
        FROM sensor_data.readings r
        WHERE r.tank_id = p_tank_id
        AND r.time >= NOW() - (p_days || ' days')::INTERVAL
        GROUP BY date_trunc('day', time)
        ORDER BY day
    ),
    trend_analysis AS (
        SELECT 
            regr_slope(daily_quality, extract(epoch from day)) as slope
        FROM quality_trend
    )
    SELECT COALESCE(ABS(slope) * 1000000, 0) INTO sensor_degradation FROM trend_analysis;

    -- Calculate reading frequency issues
    WITH expected_readings AS (
        SELECT COUNT(*) as expected_count
        FROM generate_series(
            NOW() - (p_days || ' days')::INTERVAL,
            NOW(),
            INTERVAL '5 minutes'
        ) as expected_time
    ),
    actual_readings AS (
        SELECT COUNT(*) as actual_count
        FROM sensor_data.readings r
        WHERE r.tank_id = p_tank_id
        AND r.time >= NOW() - (p_days || ' days')::INTERVAL
    )
    SELECT 
        GREATEST(0, 100 - (actual_count::DOUBLE PRECISION / expected_count::DOUBLE PRECISION * 100))
    INTO reading_frequency
    FROM expected_readings, actual_readings;

    -- Calculate anomaly frequency
    SELECT 
        COUNT(*) * 10.0 / p_days
    INTO anomaly_frequency
    FROM sensor_data.readings r
    WHERE r.tank_id = p_tank_id
    AND r.time >= NOW() - (p_days || ' days')::INTERVAL
    AND EXISTS (
        SELECT 1 FROM sensor_data.detect_anomalies(r.sensor_id, 24, 2.5) da
        WHERE da.is_anomaly = true
    );

    -- Calculate overall maintenance score
    maintenance_score := LEAST(100, 
        sensor_degradation * 0.4 + 
        reading_frequency * 0.3 + 
        anomaly_frequency * 0.3
    );

    result := jsonb_build_object(
        'maintenance_score', maintenance_score,
        'sensor_degradation', sensor_degradation,
        'reading_frequency_issues', reading_frequency,
        'anomaly_frequency', anomaly_frequency,
        'recommendation', 
        CASE 
            WHEN maintenance_score > 80 THEN 'Immediate maintenance required'
            WHEN maintenance_score > 60 THEN 'Schedule maintenance soon'
            WHEN maintenance_score > 40 THEN 'Monitor closely'
            ELSE 'System operating normally'
        END,
        'calculated_at', NOW()
    );

    RETURN result;
END;
$$ LANGUAGE plpgsql;

