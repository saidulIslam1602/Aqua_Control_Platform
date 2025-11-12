package com.aquacontrol.streams;

import org.apache.kafka.common.serialization.Serdes;
import org.apache.kafka.streams.KafkaStreams;
import org.apache.kafka.streams.StreamsBuilder;
import org.apache.kafka.streams.StreamsConfig;
import org.apache.kafka.streams.kstream.*;
import org.apache.kafka.streams.state.Stores;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

import java.time.Duration;
import java.util.Properties;
import java.util.concurrent.CountDownLatch;

/**
 * Kafka Streams processor for real-time sensor data processing
 * Handles anomaly detection, aggregations, and data quality monitoring
 */
public class SensorDataProcessor {
    private static final Logger logger = LoggerFactory.getLogger(SensorDataProcessor.class);
    
    public static void main(String[] args) {
        Properties props = new Properties();
        props.put(StreamsConfig.APPLICATION_ID_CONFIG, "aquacontrol-sensor-processor");
        props.put(StreamsConfig.BOOTSTRAP_SERVERS_CONFIG, "localhost:9092");
        props.put(StreamsConfig.DEFAULT_KEY_SERDE_CLASS_CONFIG, Serdes.String().getClass());
        props.put(StreamsConfig.DEFAULT_VALUE_SERDE_CLASS_CONFIG, Serdes.String().getClass());
        props.put(StreamsConfig.PROCESSING_GUARANTEE_CONFIG, StreamsConfig.EXACTLY_ONCE_V2);
        props.put(StreamsConfig.COMMIT_INTERVAL_MS_CONFIG, 1000);
        
        StreamsBuilder builder = new StreamsBuilder();
        
        // Create state stores for maintaining sensor state
        builder.addStateStore(
            Stores.keyValueStoreBuilder(
                Stores.persistentKeyValueStore("sensor-state-store"),
                Serdes.String(),
                Serdes.String()
            )
        );
        
        // Input stream from sensor readings topic
        KStream<String, String> sensorReadings = builder.stream("sensor-readings");
        
        // Parse and validate sensor data
        KStream<String, SensorReading> validReadings = sensorReadings
            .mapValues(SensorReading::fromJson)
            .filter((key, reading) -> reading != null && reading.isValid())
            .peek((key, reading) -> logger.debug("Processing reading: {}", reading));
        
        // Real-time anomaly detection using transformer
        KStream<String, AnomalyAlert> anomalies = validReadings
            .transform(AnomalyDetectionTransformer::new, "sensor-state-store")
            .filter((key, alert) -> alert != null);
        
        // Send anomalies to alerts topic
        anomalies.to("sensor-anomalies", Produced.with(Serdes.String(), new JsonSerde<>(AnomalyAlert.class)));
        
        // Aggregate readings by sensor and time window (5-minute windows, 1-minute advance)
        KTable<Windowed<String>, SensorAggregation> sensorAggregations = validReadings
            .groupBy((key, reading) -> reading.getSensorId())
            .windowedBy(TimeWindows.of(Duration.ofMinutes(5)).advanceBy(Duration.ofMinutes(1)))
            .aggregate(
                SensorAggregation::new,
                (key, reading, aggregation) -> aggregation.add(reading),
                Materialized.<String, SensorAggregation>as("sensor-aggregations")
                    .withKeySerde(Serdes.String())
                    .withValueSerde(new JsonSerde<>(SensorAggregation.class))
            );
        
        // Convert windowed aggregations to stream and send to topic
        sensorAggregations
            .toStream()
            .map((windowedKey, aggregation) -> {
                String key = windowedKey.key() + "@" + windowedKey.window().start();
                return KeyValue.pair(key, aggregation);
            })
            .to("sensor-aggregations", Produced.with(Serdes.String(), new JsonSerde<>(SensorAggregation.class)));
        
        // Tank-level aggregations (15-minute windows, 5-minute advance)
        KTable<Windowed<String>, TankMetrics> tankMetrics = validReadings
            .groupBy((key, reading) -> reading.getTankId())
            .windowedBy(TimeWindows.of(Duration.ofMinutes(15)).advanceBy(Duration.ofMinutes(5)))
            .aggregate(
                TankMetrics::new,
                (key, reading, metrics) -> metrics.addReading(reading),
                Materialized.<String, TankMetrics>as("tank-metrics")
                    .withKeySerde(Serdes.String())
                    .withValueSerde(new JsonSerde<>(TankMetrics.class))
            );
        
        // Send tank metrics to topic
        tankMetrics
            .toStream()
            .map((windowedKey, metrics) -> {
                String key = windowedKey.key() + "@" + windowedKey.window().start();
                return KeyValue.pair(key, metrics);
            })
            .to("tank-metrics", Produced.with(Serdes.String(), new JsonSerde<>(TankMetrics.class)));
        
        // Data quality monitoring (hourly windows)
        KStream<String, DataQualityMetric> qualityMetrics = validReadings
            .groupByKey()
            .windowedBy(TimeWindows.of(Duration.ofHours(1)))
            .aggregate(
                DataQualityMetric::new,
                (key, reading, metric) -> metric.addReading(reading),
                Materialized.<String, DataQualityMetric>as("quality-metrics")
                    .withKeySerde(Serdes.String())
                    .withValueSerde(new JsonSerde<>(DataQualityMetric.class))
            )
            .toStream()
            .map((windowedKey, metric) -> {
                String key = windowedKey.key() + "@" + windowedKey.window().start();
                return KeyValue.pair(key, metric);
            });
        
        qualityMetrics.to("data-quality-metrics", Produced.with(Serdes.String(), new JsonSerde<>(DataQualityMetric.class)));
        
        // Build and start the streams application
        KafkaStreams streams = new KafkaStreams(builder.build(), props);
        
        // Add shutdown hook for graceful shutdown
        CountDownLatch latch = new CountDownLatch(1);
        Runtime.getRuntime().addShutdownHook(new Thread("streams-shutdown-hook") {
            @Override
            public void run() {
                logger.info("Shutting down Kafka Streams...");
                streams.close();
                latch.countDown();
            }
        });
        
        try {
            streams.start();
            logger.info("Kafka Streams application started");
            latch.await();
        } catch (Throwable e) {
            logger.error("Error running Kafka Streams application", e);
            System.exit(1);
        }
        System.exit(0);
    }
}

