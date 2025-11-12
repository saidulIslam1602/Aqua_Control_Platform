import pandas as pd
import numpy as np
from typing import Dict, List, Optional, Tuple
from datetime import datetime, timedelta
import logging
from dataclasses import dataclass
from sklearn.preprocessing import StandardScaler, RobustScaler
from sklearn.impute import SimpleImputer
import psycopg2
from sqlalchemy import create_engine
import redis
import json

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

@dataclass
class FeatureConfig:
    """Configuration for feature engineering pipeline"""
    lookback_hours: int = 24
    aggregation_windows: List[str] = None
    sensor_types: List[str] = None
    include_seasonal: bool = True
    include_lag_features: bool = True
    max_lag_hours: int = 6
    
    def __post_init__(self):
        if self.aggregation_windows is None:
            self.aggregation_windows = ['1H', '3H', '6H', '12H', '24H']
        if self.sensor_types is None:
            self.sensor_types = ['Temperature', 'pH', 'DissolvedOxygen', 'Salinity']

class AquacultureFeatureEngineer:
    """Advanced feature engineering for aquaculture ML models"""
    
    def __init__(self, db_config: Dict, redis_config: Dict, feature_config: FeatureConfig):
        self.db_config = db_config
        self.redis_config = redis_config
        self.config = feature_config
        
        # Database connection
        self.engine = create_engine(
            f"postgresql://{db_config['user']}:{db_config['password']}@"
            f"{db_config['host']}:{db_config['port']}/{db_config['database']}"
        )
        
        # Redis connection for caching
        self.redis_client = redis.Redis(
            host=redis_config['host'],
            port=redis_config['port'],
            password=redis_config.get('password'),
            decode_responses=True
        )
        
        # Scalers for different feature types
        self.scalers = {
            'temperature': RobustScaler(),
            'ph': StandardScaler(),
            'oxygen': RobustScaler(),
            'salinity': StandardScaler()
        }
        
        logger.info("Feature engineering pipeline initialized")
    
    def extract_sensor_data(self, tank_id: str, start_time: datetime, end_time: datetime) -> pd.DataFrame:
        """Extract sensor data for feature engineering"""
        query = """
        SELECT 
            r.time,
            r.sensor_id,
            r.tank_id,
            r.sensor_type,
            r.value,
            r.quality_score,
            s.model,
            s.manufacturer,
            s.accuracy,
            s.min_value,
            s.max_value
        FROM sensor_data.readings r
        JOIN sensor_data.sensors s ON r.sensor_id = s.id
        WHERE r.tank_id = %s
        AND r.time BETWEEN %s AND %s
        AND r.quality_score >= 0.7
        ORDER BY r.time, r.sensor_type
        """
        
        df = pd.read_sql_query(
            query, 
            self.engine, 
            params=[tank_id, start_time, end_time],
            parse_dates=['time']
        )
        
        logger.info(f"Extracted {len(df)} sensor readings for tank {tank_id}")
        return df
    
    def create_time_features(self, df: pd.DataFrame) -> pd.DataFrame:
        """Create time-based features"""
        df = df.copy()
        
        # Basic time features
        df['hour'] = df['time'].dt.hour
        df['day_of_week'] = df['time'].dt.dayofweek
        df['month'] = df['time'].dt.month
        df['quarter'] = df['time'].dt.quarter
        
        # Cyclical encoding for time features
        df['hour_sin'] = np.sin(2 * np.pi * df['hour'] / 24)
        df['hour_cos'] = np.cos(2 * np.pi * df['hour'] / 24)
        df['day_sin'] = np.sin(2 * np.pi * df['day_of_week'] / 7)
        df['day_cos'] = np.cos(2 * np.pi * df['day_of_week'] / 7)
        df['month_sin'] = np.sin(2 * np.pi * df['month'] / 12)
        df['month_cos'] = np.cos(2 * np.pi * df['month'] / 12)
        
        # Business logic features
        df['is_weekend'] = df['day_of_week'].isin([5, 6])
        df['is_night'] = df['hour'].isin(list(range(22, 24)) + list(range(0, 6)))
        df['is_feeding_time'] = df['hour'].isin([8, 12, 18])  # Typical feeding hours
        
        if self.config.include_seasonal:
            # Seasonal features for aquaculture
            df['season'] = df['month'].map({
                12: 'winter', 1: 'winter', 2: 'winter',
                3: 'spring', 4: 'spring', 5: 'spring',
                6: 'summer', 7: 'summer', 8: 'summer',
                9: 'autumn', 10: 'autumn', 11: 'autumn'
            })
            
            # Temperature season adjustment (Northern Hemisphere)
            df['temp_season_factor'] = df['month'].map({
                12: 0.2, 1: 0.1, 2: 0.3,  # Winter - lower temps expected
                3: 0.6, 4: 0.8, 5: 0.9,   # Spring - rising temps
                6: 1.0, 7: 1.0, 8: 1.0,   # Summer - peak temps
                9: 0.8, 10: 0.6, 11: 0.4  # Autumn - falling temps
            })
        
        return df
    
    def create_sensor_features(self, df: pd.DataFrame) -> pd.DataFrame:
        """Create sensor-specific features"""
        features_df = pd.DataFrame()
        
        for sensor_type in self.config.sensor_types:
            sensor_data = df[df['sensor_type'] == sensor_type].copy()
            if sensor_data.empty:
                continue
            
            # Pivot to get sensor readings as columns
            sensor_pivot = sensor_data.pivot_table(
                index='time',
                values='value',
                aggfunc='mean'
            ).fillna(method='ffill').fillna(method='bfill')
            
            if sensor_pivot.empty:
                continue
            
            sensor_name = sensor_type.lower()
            
            # Basic statistical features for different time windows
            for window in self.config.aggregation_windows:
                rolling = sensor_pivot.rolling(window)
                
                features_df[f'{sensor_name}_mean_{window}'] = rolling.mean()
                features_df[f'{sensor_name}_std_{window}'] = rolling.std()
                features_df[f'{sensor_name}_min_{window}'] = rolling.min()
                features_df[f'{sensor_name}_max_{window}'] = rolling.max()
                features_df[f'{sensor_name}_range_{window}'] = (
                    rolling.max() - rolling.min()
                )
                
                # Percentiles
                features_df[f'{sensor_name}_q25_{window}'] = rolling.quantile(0.25)
                features_df[f'{sensor_name}_q75_{window}'] = rolling.quantile(0.75)
                features_df[f'{sensor_name}_iqr_{window}'] = (
                    rolling.quantile(0.75) - rolling.quantile(0.25)
                )
                
                # Trend features
                features_df[f'{sensor_name}_trend_{window}'] = (
                    sensor_pivot - rolling.mean()
                )
                
                # Stability features
                features_df[f'{sensor_name}_cv_{window}'] = (
                    rolling.std() / rolling.mean()
                )
            
            # Rate of change features
            features_df[f'{sensor_name}_roc_1h'] = sensor_pivot.diff(periods=12)  # 5min intervals
            features_df[f'{sensor_name}_roc_3h'] = sensor_pivot.diff(periods=36)
            features_df[f'{sensor_name}_roc_6h'] = sensor_pivot.diff(periods=72)
            
            # Acceleration (second derivative)
            features_df[f'{sensor_name}_acceleration'] = (
                features_df[f'{sensor_name}_roc_1h'].diff()
            )
            
            # Lag features
            if self.config.include_lag_features:
                for lag_hours in range(1, self.config.max_lag_hours + 1):
                    lag_periods = lag_hours * 12  # 5-minute intervals
                    features_df[f'{sensor_name}_lag_{lag_hours}h'] = (
                        sensor_pivot.shift(lag_periods)
                    )
            
            # Anomaly indicators (Z-score based)
            rolling_24h = sensor_pivot.rolling('24H')
            z_scores = (sensor_pivot - rolling_24h.mean()) / rolling_24h.std()
            features_df[f'{sensor_name}_zscore'] = z_scores
            features_df[f'{sensor_name}_is_anomaly'] = (np.abs(z_scores) > 3).astype(int)
            
            # Sensor-specific features
            if sensor_type == 'Temperature':
                # Temperature shock indicators
                features_df['temp_shock_risk'] = (
                    np.abs(features_df[f'{sensor_name}_roc_1h']) > 2.0
                ).astype(int)
                
                # Thermal stratification risk
                features_df['thermal_stratification'] = (
                    features_df[f'{sensor_name}_std_3H'] > 1.5
                ).astype(int)
            
            elif sensor_type == 'pH':
                # pH stability score
                features_df['ph_stability'] = 1 / (1 + features_df[f'{sensor_name}_std_6H'])
                
                # pH stress indicators
                features_df['ph_stress_low'] = (sensor_pivot < 6.5).astype(int)
                features_df['ph_stress_high'] = (sensor_pivot > 8.5).astype(int)
            
            elif sensor_type == 'DissolvedOxygen':
                # Oxygen depletion risk
                features_df['oxygen_depletion_risk'] = (sensor_pivot < 4.0).astype(int)
                features_df['oxygen_critical'] = (sensor_pivot < 2.0).astype(int)
                
                # Oxygen trend (important for early warning)
                features_df['oxygen_declining_trend'] = (
                    features_df[f'{sensor_name}_trend_3H'] < -0.5
                ).astype(int)
        
        # Cross-sensor interaction features
        if 'temperature' in [s.lower() for s in self.config.sensor_types] and \
           'dissolvedoxygen' in [s.lower() for s in self.config.sensor_types]:
            
            # Temperature-oxygen relationship (inverse correlation expected)
            temp_cols = [c for c in features_df.columns if c.startswith('temperature_mean')]
            oxygen_cols = [c for c in features_df.columns if c.startswith('dissolvedoxygen_mean')]
            
            if temp_cols and oxygen_cols:
                for temp_col, oxygen_col in zip(temp_cols, oxygen_cols):
                    window = temp_col.split('_')[-1]
                    features_df[f'temp_oxygen_ratio_{window}'] = (
                        features_df[temp_col] / (features_df[oxygen_col] + 1e-6)
                    )
        
        # Reset index to include time
        features_df = features_df.reset_index()
        
        return features_df
    
    def create_tank_context_features(self, tank_id: str, df: pd.DataFrame) -> pd.DataFrame:
        """Create tank-specific context features"""
        # Get tank metadata
        tank_query = """
        SELECT 
            capacity_value,
            capacity_unit,
            tank_type,
            location,
            optimal_parameters
        FROM sensor_data.tanks
        WHERE id = %s
        """
        
        tank_info = pd.read_sql_query(tank_query, self.engine, params=[tank_id])
        
        if tank_info.empty:
            return df
        
        tank_data = tank_info.iloc[0]
        
        # Add tank metadata as features
        df['tank_capacity'] = tank_data['capacity_value']
        df['tank_type_encoded'] = hash(tank_data['tank_type']) % 1000  # Simple encoding
        
        # Location-based features
        location = json.loads(tank_data['location']) if tank_data['location'] else {}
        df['building_encoded'] = hash(location.get('building', '')) % 100
        df['room_encoded'] = hash(location.get('room', '')) % 100
        
        # Optimal parameters deviation
        if tank_data['optimal_parameters']:
            optimal = json.loads(tank_data['optimal_parameters'])
            
            for param, optimal_value in optimal.items():
                if param in df.columns:
                    df[f'{param}_deviation'] = np.abs(df[param] - optimal_value)
                    df[f'{param}_within_optimal'] = (
                        np.abs(df[param] - optimal_value) < optimal_value * 0.1
                    ).astype(int)
        
        return df
    
    def engineer_features(self, tank_id: str, target_time: datetime) -> pd.DataFrame:
        """Main feature engineering pipeline"""
        logger.info(f"Engineering features for tank {tank_id} at {target_time}")
        
        # Check cache first
        cache_key = f"features:{tank_id}:{target_time.isoformat()}"
        cached_features = self.redis_client.get(cache_key)
        
        if cached_features:
            logger.info("Using cached features")
            return pd.read_json(cached_features)
        
        # Define time window
        start_time = target_time - timedelta(hours=self.config.lookback_hours)
        end_time = target_time
        
        # Extract raw sensor data
        raw_data = self.extract_sensor_data(tank_id, start_time, end_time)
        
        if raw_data.empty:
            logger.warning(f"No sensor data found for tank {tank_id}")
            return pd.DataFrame()
        
        # Create time features
        df_with_time = self.create_time_features(raw_data)
        
        # Create sensor features
        sensor_features = self.create_sensor_features(df_with_time)
        
        # Add tank context features
        final_features = self.create_tank_context_features(tank_id, sensor_features)
        
        # Handle missing values
        imputer = SimpleImputer(strategy='median')
        numeric_columns = final_features.select_dtypes(include=[np.number]).columns
        final_features[numeric_columns] = imputer.fit_transform(final_features[numeric_columns])
        
        # Add metadata
        final_features['tank_id'] = tank_id
        final_features['feature_timestamp'] = target_time
        final_features['feature_version'] = 'v1.0'
        
        # Cache features for 1 hour
        self.redis_client.setex(
            cache_key, 
            3600, 
            final_features.to_json()
        )
        
        logger.info(f"Generated {len(final_features.columns)} features for tank {tank_id}")
        
        return final_features
    
    def batch_engineer_features(self, tank_ids: List[str], start_time: datetime, 
                              end_time: datetime, interval_hours: int = 1) -> pd.DataFrame:
        """Batch feature engineering for multiple tanks and time periods"""
        all_features = []
        
        current_time = start_time
        while current_time <= end_time:
            for tank_id in tank_ids:
                try:
                    features = self.engineer_features(tank_id, current_time)
                    if not features.empty:
                        all_features.append(features)
                except Exception as e:
                    logger.error(f"Error engineering features for tank {tank_id} at {current_time}: {e}")
            
            current_time += timedelta(hours=interval_hours)
        
        if all_features:
            return pd.concat(all_features, ignore_index=True)
        else:
            return pd.DataFrame()
    
    def save_features_to_db(self, features_df: pd.DataFrame):
        """Save engineered features to database"""
        if features_df.empty:
            return
        
        # Prepare features JSON
        feature_columns = [col for col in features_df.columns 
                          if col not in ['tank_id', 'feature_timestamp', 'feature_version']]
        
        records = []
        for _, row in features_df.iterrows():
            features_json = {col: row[col] for col in feature_columns if pd.notna(row[col])}
            
            records.append({
                'feature_timestamp': row['feature_timestamp'],
                'tank_id': row['tank_id'],
                'features': json.dumps(features_json),
                'feature_version': row['feature_version']
            })
        
        # Insert into database
        insert_query = """
        INSERT INTO ml_features.tank_features 
        (feature_timestamp, tank_id, features, feature_version)
        VALUES (%(feature_timestamp)s, %(tank_id)s, %(features)s, %(feature_version)s)
        ON CONFLICT (feature_timestamp, tank_id) 
        DO UPDATE SET 
            features = EXCLUDED.features,
            feature_version = EXCLUDED.feature_version
        """
        
        with self.engine.connect() as conn:
            conn.execute(insert_query, records)
            conn.commit()
        
        logger.info(f"Saved {len(records)} feature records to database")

# Example usage and configuration
if __name__ == "__main__":
    # Configuration
    db_config = {
        'host': 'localhost',
        'port': 5432,
        'database': 'aquacontrol_timeseries',
        'user': 'aquacontrol',
        'password': 'AquaControl123!'
    }
    
    redis_config = {
        'host': 'localhost',
        'port': 6379,
        'password': 'AquaControl123!'
    }
    
    feature_config = FeatureConfig(
        lookback_hours=48,
        aggregation_windows=['1H', '3H', '6H', '12H', '24H'],
        sensor_types=['Temperature', 'pH', 'DissolvedOxygen', 'Salinity'],
        include_seasonal=True,
        include_lag_features=True,
        max_lag_hours=6
    )
    
    # Initialize feature engineer
    feature_engineer = AquacultureFeatureEngineer(db_config, redis_config, feature_config)
    
    # Example: Engineer features for a specific tank
    tank_id = "550e8400-e29b-41d4-a716-446655440000"
    target_time = datetime.now()
    
    features = feature_engineer.engineer_features(tank_id, target_time)
    print(f"Generated features shape: {features.shape}")
    print(f"Feature columns: {list(features.columns)}")
    
    # Save to database
    feature_engineer.save_features_to_db(features)

