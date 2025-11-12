# Data Engineering Layer

This directory contains the Data Engineering infrastructure for real-time analytics, time-series data processing, and ML feature engineering.

## Structure

```
data-engineering/
├── kafka/                    # Kafka streaming infrastructure
│   └── docker-compose.kafka.yml
├── timescaledb/              # TimescaleDB time-series database
│   ├── docker-compose.timescale.yml
│   ├── init-scripts/         # SQL initialization scripts
│   │   ├── 01-create-schema.sql
│   │   └── 02-analytics-functions.sql
│   └── config/               # Database configuration
│       ├── postgresql.conf
│       └── redis.conf
├── kafka-streams/            # Kafka Streams processing
│   └── src/main/java/com/aquacontrol/streams/
│       └── SensorDataProcessor.java
└── ml-pipeline/              # ML feature engineering
    └── feature_engineering.py
```

## Components

### 1. Kafka Streaming Infrastructure
- **Zookeeper**: Coordination service
- **Kafka**: Event streaming platform
- **Schema Registry**: Schema management for Avro
- **Kafka Connect**: Connector framework
- **KSQL DB**: Stream processing SQL engine
- **Kafka UI**: Web interface for monitoring

### 2. TimescaleDB Time-Series Database
- **Hypertables**: Optimized for time-series data
- **Analytics Functions**: Real-time anomaly detection, health scoring
- **Redis**: Feature caching layer

### 3. Kafka Streams Processing
- **Real-time Processing**: Stream processing for sensor data
- **Anomaly Detection**: Statistical anomaly detection
- **Aggregations**: Time-windowed aggregations
- **Data Quality**: Quality monitoring

### 4. ML Feature Engineering
- **Feature Pipeline**: 100+ engineered features
- **Time Features**: Cyclical encoding, seasonal features
- **Sensor Features**: Statistical aggregations, trends, lags
- **Cross-sensor Features**: Interaction features
- **Caching**: Redis-based feature caching

## Getting Started

### Start Kafka Infrastructure
```bash
cd kafka
docker-compose -f docker-compose.kafka.yml up -d
```

### Start TimescaleDB
```bash
cd timescaledb
docker-compose -f docker-compose.timescale.yml up -d
```

### Run ML Feature Engineering
```bash
cd ml-pipeline
python feature_engineering.py
```

## Key Features

- **Real-time Streaming**: Kafka for event streaming
- **Time-Series Optimization**: TimescaleDB hypertables
- **Advanced Analytics**: SQL functions for health scoring, anomaly detection
- **ML-Ready Features**: Comprehensive feature engineering pipeline
- **Scalable Architecture**: Production-ready data pipeline

