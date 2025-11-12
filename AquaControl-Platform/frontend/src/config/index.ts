interface AppConfig {
  api: {
    baseUrl: string
    timeout: number
    retryAttempts: number
    retryDelay: number
  }
  auth: {
    tokenKey: string
    refreshTokenKey: string
    tokenExpirationBuffer: number
  }
  features: {
    enableAnalytics: boolean
    enableRealTime: boolean
    enableNotifications: boolean
    enableOfflineMode: boolean
  }
  ui: {
    theme: 'light' | 'dark' | 'auto'
    language: string
    pageSize: number
    animationDuration: number
  }
  monitoring: {
    enableErrorReporting: boolean
    enablePerformanceMonitoring: boolean
    sampleRate: number
  }
  cache: {
    defaultTtl: number
    maxSize: number
    enablePersistence: boolean
  }
}

const developmentConfig: AppConfig = {
  api: {
    baseUrl: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000',
    timeout: 30000,
    retryAttempts: 3,
    retryDelay: 1000
  },
  auth: {
    tokenKey: 'aquacontrol_token',
    refreshTokenKey: 'aquacontrol_refresh_token',
    tokenExpirationBuffer: 300000 // 5 minutes
  },
  features: {
    enableAnalytics: false,
    enableRealTime: true,
    enableNotifications: true,
    enableOfflineMode: false
  },
  ui: {
    theme: 'light',
    language: 'en',
    pageSize: 20,
    animationDuration: 300
  },
  monitoring: {
    enableErrorReporting: false,
    enablePerformanceMonitoring: false,
    sampleRate: 1.0
  },
  cache: {
    defaultTtl: 300000, // 5 minutes
    maxSize: 100,
    enablePersistence: true
  }
}

const productionConfig: AppConfig = {
  api: {
    baseUrl: import.meta.env.VITE_API_BASE_URL || 'https://api.aquacontrol.com',
    timeout: 15000,
    retryAttempts: 3,
    retryDelay: 2000
  },
  auth: {
    tokenKey: 'aquacontrol_token',
    refreshTokenKey: 'aquacontrol_refresh_token',
    tokenExpirationBuffer: 300000 // 5 minutes
  },
  features: {
    enableAnalytics: true,
    enableRealTime: true,
    enableNotifications: true,
    enableOfflineMode: true
  },
  ui: {
    theme: 'auto',
    language: 'en',
    pageSize: 50,
    animationDuration: 200
  },
  monitoring: {
    enableErrorReporting: true,
    enablePerformanceMonitoring: true,
    sampleRate: 0.1 // 10% sampling
  },
  cache: {
    defaultTtl: 600000, // 10 minutes
    maxSize: 500,
    enablePersistence: true
  }
}

const testConfig: AppConfig = {
  ...developmentConfig,
  api: {
    ...developmentConfig.api,
    baseUrl: 'http://localhost:5001'
  },
  features: {
    ...developmentConfig.features,
    enableAnalytics: false,
    enableRealTime: false
  },
  monitoring: {
    enableErrorReporting: false,
    enablePerformanceMonitoring: false,
    sampleRate: 0
  }
}

function getConfig(): AppConfig {
  const env = import.meta.env.MODE

  switch (env) {
    case 'production':
      return productionConfig
    case 'test':
      return testConfig
    case 'development':
    default:
      return developmentConfig
  }
}

// Create and export the configuration
export const config = getConfig()

// Export individual sections for convenience
export const apiConfig = config.api
export const authConfig = config.auth
export const featureConfig = config.features
export const uiConfig = config.ui
export const monitoringConfig = config.monitoring
export const cacheConfig = config.cache

// Validation function
export function validateConfig(cfg: AppConfig): boolean {
  try {
    // Validate required fields
    if (!cfg.api.baseUrl) {
      throw new Error('API base URL is required')
    }

    if (cfg.api.timeout <= 0) {
      throw new Error('API timeout must be positive')
    }

    if (cfg.auth.tokenExpirationBuffer <= 0) {
      throw new Error('Token expiration buffer must be positive')
    }

    if (cfg.cache.defaultTtl <= 0) {
      throw new Error('Cache TTL must be positive')
    }

    if (cfg.monitoring.sampleRate < 0 || cfg.monitoring.sampleRate > 1) {
      throw new Error('Sample rate must be between 0 and 1')
    }

    return true
  } catch (error) {
    console.error('Configuration validation failed:', error)
    return false
  }
}

// Validate the current configuration
if (!validateConfig(config)) {
  console.error('Invalid configuration detected!')
}

// Environment helpers
export const isDevelopment = import.meta.env.DEV
export const isProduction = import.meta.env.PROD
export const isTest = import.meta.env.MODE === 'test'

// Debug helper
if (isDevelopment) {
  console.log('App Configuration:', config)
}

export default config
