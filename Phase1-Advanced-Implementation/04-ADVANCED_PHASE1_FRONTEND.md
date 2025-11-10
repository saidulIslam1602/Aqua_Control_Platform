# Phase 1 - Step 4: Frontend Layer - Enterprise Vue.js 3 Architecture

## 🎯 Frontend Architecture

This implementation uses **Advanced Vue.js 3 Patterns**, **Micro-Frontend Architecture**, **Real-time State Management**, and **Enterprise UI Patterns** used by companies like Airbnb, GitLab, and Shopify.

### Frontend Architecture Layers
```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                        │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐ │
│  │   Components    │  │     Views       │  │   Layouts    │ │
│  │   (Atomic)      │  │   (Pages)       │  │  (Shells)    │ │
│  └─────────────────┘  └─────────────────┘  └──────────────┘ │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                   Application Layer                          │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐ │
│  │   Composables   │  │     Stores      │  │   Services   │ │
│  │   (Logic)       │  │    (State)      │  │    (API)     │ │
│  └─────────────────┘  └─────────────────┘  └──────────────┘ │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                 Infrastructure Layer                         │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐ │
│  │   HTTP Client   │  │   WebSockets    │  │   Storage    │ │
│  │   (Axios)       │  │   (SignalR)     │  │   (Cache)    │ │
│  └─────────────────┘  └─────────────────┘  └──────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

---

## 📁 Advanced Frontend Structure

### File 1: Advanced Package Configuration
**File:** `frontend/package.json`
```json
{
  "name": "aquacontrol-frontend",
  "version": "1.0.0",
  "description": "AquaControl Platform - Enterprise Vue.js 3 Frontend",
  "type": "module",
  "scripts": {
    "dev": "vite --host",
    "build": "vue-tsc && vite build",
    "preview": "vite preview",
    "test:unit": "vitest",
    "test:e2e": "playwright test",
    "test:coverage": "vitest --coverage",
    "lint": "eslint . --ext .vue,.js,.jsx,.cjs,.mjs,.ts,.tsx,.cts,.mts --fix",
    "lint:style": "stylelint **/*.{vue,css,scss} --fix",
    "type-check": "vue-tsc --noEmit",
    "analyze": "vite-bundle-analyzer dist/stats.html",
    "storybook": "storybook dev -p 6006",
    "build-storybook": "storybook build"
  },
  "dependencies": {
    "vue": "^3.4.21",
    "vue-router": "^4.3.0",
    "pinia": "^2.1.7",
    "@vueuse/core": "^10.9.0",
    "@vueuse/integrations": "^10.9.0",
    
    "// API & Real-time": "",
    "@apollo/client": "^3.9.5",
    "graphql": "^16.8.1",
    "graphql-tag": "^2.12.6",
    "@microsoft/signalr": "^8.0.7",
    "axios": "^1.6.8",
    "ky": "^1.2.3",
    
    "// UI Framework": "",
    "element-plus": "^2.6.3",
    "@element-plus/icons-vue": "^2.3.1",
    "headlessui": "^0.0.0",
    "@headlessui/vue": "^1.7.19",
    
    "// Charts & Visualization": "",
    "echarts": "^5.5.0",
    "vue-echarts": "^6.6.11",
    "d3": "^7.9.0",
    "@observablehq/plot": "^0.6.14",
    
    "// Forms & Validation": "",
    "vee-validate": "^4.12.6",
    "yup": "^1.4.0",
    "@vee-validate/yup": "^4.12.6",
    
    "// Utilities": "",
    "dayjs": "^1.11.10",
    "lodash-es": "^4.17.21",
    "uuid": "^9.0.1",
    "fuse.js": "^7.0.0",
    "mitt": "^3.0.1",
    
    "// State & Persistence": "",
    "pinia-plugin-persistedstate": "^3.2.1",
    "idb": "^8.0.0",
    
    "// Performance": "",
    "vue-virtual-scroller": "^2.0.0-beta.8",
    "vue-observe-visibility": "^2.0.0-alpha.1"
  },
  "devDependencies": {
    "@vitejs/plugin-vue": "^5.0.4",
    "@vue/eslint-config-typescript": "^13.0.0",
    "@vue/tsconfig": "^0.5.1",
    "typescript": "^5.4.3",
    "vue-tsc": "^2.0.6",
    "vite": "^5.2.0",
    
    "// Testing": "",
    "vitest": "^1.4.0",
    "@vue/test-utils": "^2.4.5",
    "jsdom": "^24.0.0",
    "@vitest/coverage-v8": "^1.4.0",
    "playwright": "^1.42.1",
    "@playwright/test": "^1.42.1",
    
    "// Linting & Formatting": "",
    "eslint": "^8.57.0",
    "eslint-plugin-vue": "^9.23.0",
    "prettier": "^3.2.5",
    "stylelint": "^16.3.1",
    "stylelint-config-standard-vue": "^1.0.0",
    
    "// Build Tools": "",
    "@types/node": "^20.11.30",
    "@types/lodash-es": "^4.17.12",
    "@types/uuid": "^9.0.8",
    "sass": "^1.72.0",
    "autoprefixer": "^10.4.19",
    "postcss": "^8.4.38",
    "tailwindcss": "^3.4.3",
    
    "// Development": "",
    "vite-bundle-analyzer": "^0.7.0",
    "vite-plugin-pwa": "^0.19.7",
    "workbox-window": "^7.0.0",
    
    "// Storybook": "",
    "@storybook/vue3": "^8.0.6",
    "@storybook/vue3-vite": "^8.0.6",
    "storybook": "^8.0.6"
  }
}
```

### File 2: Advanced Vite Configuration
**File:** `frontend/vite.config.ts`
```typescript
import { defineConfig, loadEnv } from 'vite'
import vue from '@vitejs/plugin-vue'
import { fileURLToPath, URL } from 'node:url'
import { VitePWA } from 'vite-plugin-pwa'

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')
  
  return {
    plugins: [
      vue({
        script: {
          defineModel: true,
          propsDestructure: true
        }
      }),
      VitePWA({
        registerType: 'autoUpdate',
        workbox: {
          globPatterns: ['**/*.{js,css,html,ico,png,svg}'],
          runtimeCaching: [
            {
              urlPattern: /^https:\/\/api\.aquacontrol\.com\/.*/i,
              handler: 'NetworkFirst',
              options: {
                cacheName: 'api-cache',
                expiration: {
                  maxEntries: 10,
                  maxAgeSeconds: 60 * 60 * 24 * 365 // 1 year
                },
                cacheableResponse: {
                  statuses: [0, 200]
                }
              }
            }
          ]
        }
      })
    ],
    resolve: {
      alias: {
        '@': fileURLToPath(new URL('./src', import.meta.url)),
        '@components': fileURLToPath(new URL('./src/components', import.meta.url)),
        '@composables': fileURLToPath(new URL('./src/composables', import.meta.url)),
        '@stores': fileURLToPath(new URL('./src/stores', import.meta.url)),
        '@services': fileURLToPath(new URL('./src/services', import.meta.url)),
        '@types': fileURLToPath(new URL('./src/types', import.meta.url)),
        '@utils': fileURLToPath(new URL('./src/utils', import.meta.url))
      }
    },
    server: {
      port: 5173,
      host: true,
      proxy: {
        '/api': {
          target: env.VITE_API_BASE_URL || 'http://localhost:5000',
          changeOrigin: true,
          secure: false
        },
        '/graphql': {
          target: env.VITE_GRAPHQL_ENDPOINT || 'http://localhost:5000',
          changeOrigin: true,
          secure: false
        },
        '/hubs': {
          target: env.VITE_SIGNALR_HUB_URL || 'http://localhost:5000',
          changeOrigin: true,
          secure: false,
          ws: true
        }
      }
    },
    build: {
      target: 'esnext',
      minify: 'esbuild',
      sourcemap: true,
      rollupOptions: {
        output: {
          manualChunks: {
            'vue-vendor': ['vue', 'vue-router', 'pinia'],
            'ui-vendor': ['element-plus', '@element-plus/icons-vue'],
            'chart-vendor': ['echarts', 'vue-echarts', 'd3'],
            'utils-vendor': ['lodash-es', 'dayjs', 'axios']
          }
        }
      },
      chunkSizeWarningLimit: 1000
    },
    optimizeDeps: {
      include: [
        'vue',
        'vue-router',
        'pinia',
        'element-plus',
        'echarts',
        'lodash-es',
        'dayjs'
      ]
    },
    define: {
      __VUE_PROD_DEVTOOLS__: false,
      __VUE_PROD_HYDRATION_MISMATCH_DETAILS__: false
    }
  }
})
```

### File 3: Advanced Type Definitions
**File:** `frontend/src/types/api.ts`
```typescript
// Base API Types
export interface ApiResponse<T = any> {
  data: T
  success: boolean
  message?: string
  errors?: Record<string, string[]>
  metadata?: {
    timestamp: string
    requestId: string
    version: string
  }
}

export interface PagedResponse<T> extends ApiResponse<T[]> {
  pagination: {
    page: number
    pageSize: number
    totalCount: number
    totalPages: number
    hasNextPage: boolean
    hasPreviousPage: boolean
  }
}

export interface ApiError {
  code: string
  message: string
  details?: Record<string, any>
  timestamp: string
  path: string
}

// Command/Query Types
export interface Command {
  type: string
  payload: Record<string, any>
  metadata?: {
    correlationId: string
    causationId?: string
    userId?: string
    timestamp: string
  }
}

export interface Query {
  type: string
  parameters: Record<string, any>
  pagination?: {
    page: number
    pageSize: number
    sortBy?: string
    sortDirection?: 'asc' | 'desc'
  }
  filters?: Record<string, any>
}

// Real-time Types
export interface SignalRConnection {
  connectionId: string
  state: 'Disconnected' | 'Connecting' | 'Connected' | 'Disconnecting' | 'Reconnecting'
  groups: string[]
}

export interface RealTimeEvent<T = any> {
  eventType: string
  data: T
  timestamp: string
  source: string
  correlationId?: string
}

// State Management Types
export interface StoreState {
  loading: boolean
  error: string | null
  lastUpdated: string | null
}

export interface OptimisticUpdate<T> {
  id: string
  type: 'create' | 'update' | 'delete'
  data: T
  timestamp: string
  rollback: () => void
}
```

**File:** `frontend/src/types/domain.ts`
```typescript
// Tank Domain Types
export interface Tank {
  id: string
  name: string
  capacity: TankCapacity
  location: Location
  tankType: TankType
  status: TankStatus
  optimalParameters?: WaterQualityParameters
  sensors: Sensor[]
  createdAt: string
  updatedAt?: string
  lastMaintenanceDate?: string
  nextMaintenanceDate?: string
  version: number
}

export interface TankCapacity {
  value: number
  unit: 'L' | 'ML' | 'GAL'
}

export interface Location {
  building: string
  room: string
  zone?: string
  latitude?: number
  longitude?: number
  fullAddress: string
}

export interface WaterQualityParameters {
  optimalTemperature?: number
  minTemperature?: number
  maxTemperature?: number
  optimalPH?: number
  minPH?: number
  maxPH?: number
  optimalOxygen?: number
  minOxygen?: number
  optimalSalinity?: number
  minSalinity?: number
  maxSalinity?: number
}

export interface Sensor {
  id: string
  tankId: string
  sensorType: SensorType
  model: string
  manufacturer: string
  serialNumber: string
  status: SensorStatus
  isActive: boolean
  minValue?: number
  maxValue?: number
  accuracy: number
  installationDate: string
  calibrationDate?: string
  nextCalibrationDate?: string
  isCalibrationDue: boolean
}

export interface SensorReading {
  id: string
  sensorId: string
  timestamp: string
  value: number
  qualityScore: number
  metadata?: Record<string, any>
}

// Enums
export enum TankType {
  Freshwater = 'Freshwater',
  Saltwater = 'Saltwater',
  Breeding = 'Breeding',
  Quarantine = 'Quarantine',
  Nursery = 'Nursery',
  GrowOut = 'Grow_out',
  Broodstock = 'Broodstock'
}

export enum TankStatus {
  Inactive = 'Inactive',
  Active = 'Active',
  Maintenance = 'Maintenance',
  Emergency = 'Emergency',
  Cleaning = 'Cleaning'
}

export enum SensorType {
  Temperature = 'Temperature',
  pH = 'pH',
  DissolvedOxygen = 'DissolvedOxygen',
  Salinity = 'Salinity',
  Turbidity = 'Turbidity',
  Ammonia = 'Ammonia',
  Nitrite = 'Nitrite',
  Nitrate = 'Nitrate',
  Phosphate = 'Phosphate',
  Alkalinity = 'Alkalinity'
}

export enum SensorStatus {
  Offline = 'Offline',
  Online = 'Online',
  Calibrating = 'Calibrating',
  Error = 'Error',
  Maintenance = 'Maintenance'
}

// Command Types
export interface CreateTankCommand {
  name: string
  capacity: number
  capacityUnit: string
  building: string
  room: string
  zone?: string
  latitude?: number
  longitude?: number
  tankType: TankType
}

export interface UpdateTankCommand extends CreateTankCommand {
  id: string
}

// Query Types
export interface GetTanksQuery {
  page?: number
  pageSize?: number
  searchTerm?: string
  tankType?: TankType
  isActive?: boolean
  sortBy?: string
  sortDescending?: boolean
}
```

### File 4: Advanced HTTP Client
**File:** `frontend/src/services/api/httpClient.ts`
```typescript
import axios, { 
  AxiosInstance, 
  AxiosRequestConfig, 
  AxiosResponse, 
  InternalAxiosRequestConfig 
} from 'axios'
import { useAuthStore } from '@stores/authStore'
import { useNotificationStore } from '@stores/notificationStore'
import type { ApiResponse, ApiError } from '@types/api'

class HttpClient {
  private client: AxiosInstance
  private readonly baseURL: string
  private readonly timeout: number = 30000

  constructor() {
    this.baseURL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000'
    
    this.client = axios.create({
      baseURL: this.baseURL,
      timeout: this.timeout,
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json'
      }
    })

    this.setupInterceptors()
  }

  private setupInterceptors(): void {
    // Request interceptor
    this.client.interceptors.request.use(
      (config: InternalAxiosRequestConfig) => {
        // Add authentication token
        const authStore = useAuthStore()
        if (authStore.token) {
          config.headers.Authorization = `Bearer ${authStore.token}`
        }

        // Add correlation ID for tracing
        config.headers['X-Correlation-ID'] = crypto.randomUUID()

        // Add request timestamp
        config.metadata = { startTime: Date.now() }

        console.log(`🚀 API Request: ${config.method?.toUpperCase()} ${config.url}`, {
          headers: config.headers,
          data: config.data
        })

        return config
      },
      (error) => {
        console.error('❌ Request Error:', error)
        return Promise.reject(error)
      }
    )

    // Response interceptor
    this.client.interceptors.response.use(
      (response: AxiosResponse) => {
        const duration = Date.now() - (response.config.metadata?.startTime || 0)
        
        console.log(`✅ API Response: ${response.status} ${response.config.url} (${duration}ms)`, {
          data: response.data,
          headers: response.headers
        })

        return response
      },
      async (error) => {
        const duration = Date.now() - (error.config?.metadata?.startTime || 0)
        
        console.error(`❌ API Error: ${error.response?.status || 'Network'} ${error.config?.url} (${duration}ms)`, {
          error: error.response?.data,
          status: error.response?.status
        })

        // Handle specific error cases
        if (error.response?.status === 401) {
          await this.handleUnauthorized()
        } else if (error.response?.status === 403) {
          this.handleForbidden()
        } else if (error.response?.status >= 500) {
          this.handleServerError(error)
        }

        return Promise.reject(this.transformError(error))
      }
    )
  }

  private async handleUnauthorized(): Promise<void> {
    const authStore = useAuthStore()
    const notificationStore = useNotificationStore()
    
    // Try to refresh token
    try {
      await authStore.refreshToken()
    } catch {
      // Refresh failed, logout user
      await authStore.logout()
      notificationStore.addNotification({
        type: 'warning',
        title: 'Session Expired',
        message: 'Please log in again to continue'
      })
    }
  }

  private handleForbidden(): void {
    const notificationStore = useNotificationStore()
    notificationStore.addNotification({
      type: 'error',
      title: 'Access Denied',
      message: 'You do not have permission to perform this action'
    })
  }

  private handleServerError(error: any): void {
    const notificationStore = useNotificationStore()
    notificationStore.addNotification({
      type: 'error',
      title: 'Server Error',
      message: 'An unexpected error occurred. Please try again later.'
    })
  }

  private transformError(error: any): ApiError {
    return {
      code: error.response?.data?.code || 'UNKNOWN_ERROR',
      message: error.response?.data?.message || error.message || 'An unexpected error occurred',
      details: error.response?.data?.details || {},
      timestamp: new Date().toISOString(),
      path: error.config?.url || ''
    }
  }

  // Generic HTTP methods
  async get<T>(url: string, config?: AxiosRequestConfig): Promise<ApiResponse<T>> {
    const response = await this.client.get<ApiResponse<T>>(url, config)
    return response.data
  }

  async post<T>(url: string, data?: any, config?: AxiosRequestConfig): Promise<ApiResponse<T>> {
    const response = await this.client.post<ApiResponse<T>>(url, data, config)
    return response.data
  }

  async put<T>(url: string, data?: any, config?: AxiosRequestConfig): Promise<ApiResponse<T>> {
    const response = await this.client.put<ApiResponse<T>>(url, data, config)
    return response.data
  }

  async patch<T>(url: string, data?: any, config?: AxiosRequestConfig): Promise<ApiResponse<T>> {
    const response = await this.client.patch<ApiResponse<T>>(url, data, config)
    return response.data
  }

  async delete<T>(url: string, config?: AxiosRequestConfig): Promise<ApiResponse<T>> {
    const response = await this.client.delete<ApiResponse<T>>(url, config)
    return response.data
  }

  // Specialized methods
  async uploadFile(url: string, file: File, onProgress?: (progress: number) => void): Promise<ApiResponse<any>> {
    const formData = new FormData()
    formData.append('file', file)

    return this.post(url, formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      },
      onUploadProgress: (progressEvent) => {
        if (onProgress && progressEvent.total) {
          const progress = Math.round((progressEvent.loaded * 100) / progressEvent.total)
          onProgress(progress)
        }
      }
    })
  }

  async downloadFile(url: string, filename?: string): Promise<void> {
    const response = await this.client.get(url, {
      responseType: 'blob'
    })

    const blob = new Blob([response.data])
    const downloadUrl = window.URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = downloadUrl
    link.download = filename || 'download'
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    window.URL.revokeObjectURL(downloadUrl)
  }
}

// Export singleton instance
export const httpClient = new HttpClient()
export default httpClient
```

### File 5: Advanced Tank Service
**File:** `frontend/src/services/api/tankService.ts`
```typescript
import { httpClient } from './httpClient'
import type { 
  Tank, 
  CreateTankCommand, 
  UpdateTankCommand, 
  GetTanksQuery 
} from '@types/domain'
import type { ApiResponse, PagedResponse } from '@types/api'

export class TankService {
  private readonly baseUrl = '/api/tanks'

  async getTanks(query: GetTanksQuery = {}): Promise<PagedResponse<Tank>> {
    const params = new URLSearchParams()
    
    if (query.page) params.append('page', query.page.toString())
    if (query.pageSize) params.append('pageSize', query.pageSize.toString())
    if (query.searchTerm) params.append('searchTerm', query.searchTerm)
    if (query.tankType) params.append('tankType', query.tankType)
    if (query.isActive !== undefined) params.append('isActive', query.isActive.toString())
    if (query.sortBy) params.append('sortBy', query.sortBy)
    if (query.sortDescending) params.append('sortDescending', query.sortDescending.toString())

    const url = `${this.baseUrl}?${params.toString()}`
    return httpClient.get<Tank[]>(url) as Promise<PagedResponse<Tank>>
  }

  async getTankById(id: string): Promise<ApiResponse<Tank>> {
    return httpClient.get<Tank>(`${this.baseUrl}/${id}`)
  }

  async createTank(command: CreateTankCommand): Promise<ApiResponse<Tank>> {
    return httpClient.post<Tank>(this.baseUrl, command)
  }

  async updateTank(id: string, command: UpdateTankCommand): Promise<ApiResponse<Tank>> {
    return httpClient.put<Tank>(`${this.baseUrl}/${id}`, command)
  }

  async deleteTank(id: string): Promise<ApiResponse<void>> {
    return httpClient.delete<void>(`${this.baseUrl}/${id}`)
  }

  async activateTank(id: string): Promise<ApiResponse<void>> {
    return httpClient.post<void>(`${this.baseUrl}/${id}/activate`)
  }

  async deactivateTank(id: string, reason: string): Promise<ApiResponse<void>> {
    return httpClient.post<void>(`${this.baseUrl}/${id}/deactivate`, { reason })
  }

  async scheduleMaintenance(id: string, maintenanceDate: string): Promise<ApiResponse<void>> {
    return httpClient.post<void>(`${this.baseUrl}/${id}/schedule-maintenance`, { maintenanceDate })
  }

  async completeMaintenance(id: string, completionDate: string, notes: string): Promise<ApiResponse<void>> {
    return httpClient.post<void>(`${this.baseUrl}/${id}/complete-maintenance`, { 
      completionDate, 
      notes 
    })
  }

  // Bulk operations
  async bulkActivate(tankIds: string[]): Promise<ApiResponse<void>> {
    return httpClient.post<void>(`${this.baseUrl}/bulk/activate`, { tankIds })
  }

  async bulkDeactivate(tankIds: string[], reason: string): Promise<ApiResponse<void>> {
    return httpClient.post<void>(`${this.baseUrl}/bulk/deactivate`, { tankIds, reason })
  }

  async bulkDelete(tankIds: string[]): Promise<ApiResponse<void>> {
    return httpClient.post<void>(`${this.baseUrl}/bulk/delete`, { tankIds })
  }

  // Analytics
  async getTankAnalytics(id: string, timeRange: string): Promise<ApiResponse<any>> {
    return httpClient.get<any>(`${this.baseUrl}/${id}/analytics?timeRange=${timeRange}`)
  }

  async getTankHealth(id: string): Promise<ApiResponse<any>> {
    return httpClient.get<any>(`${this.baseUrl}/${id}/health`)
  }

  // Export/Import
  async exportTanks(format: 'csv' | 'excel' | 'pdf' = 'csv'): Promise<void> {
    await httpClient.downloadFile(`${this.baseUrl}/export?format=${format}`, `tanks.${format}`)
  }

  async importTanks(file: File): Promise<ApiResponse<any>> {
    return httpClient.uploadFile(`${this.baseUrl}/import`, file)
  }
}

// Export singleton instance
export const tankService = new TankService()
export default tankService
```

### File 6: Advanced Pinia Store with Optimistic Updates
**File:** `frontend/src/stores/tankStore.ts`
```typescript
import { defineStore } from 'pinia'
import { ref, computed, watch } from 'vue'
import { tankService } from '@services/api/tankService'
import { useNotificationStore } from './notificationStore'
import { useRealTimeStore } from './realTimeStore'
import type { 
  Tank, 
  CreateTankCommand, 
  UpdateTankCommand, 
  GetTanksQuery 
} from '@types/domain'
import type { OptimisticUpdate, StoreState } from '@types/api'

export const useTankStore = defineStore('tanks', () => {
  // State
  const tanks = ref<Tank[]>([])
  const selectedTank = ref<Tank | null>(null)
  const state = ref<StoreState>({
    loading: false,
    error: null,
    lastUpdated: null
  })
  
  // Pagination state
  const pagination = ref({
    page: 1,
    pageSize: 20,
    totalCount: 0,
    totalPages: 0,
    hasNextPage: false,
    hasPreviousPage: false
  })

  // Filters state
  const filters = ref<GetTanksQuery>({
    searchTerm: '',
    tankType: undefined,
    isActive: undefined,
    sortBy: 'name',
    sortDescending: false
  })

  // Optimistic updates tracking
  const optimisticUpdates = ref<Map<string, OptimisticUpdate<Tank>>>(new Map())

  // Dependencies
  const notificationStore = useNotificationStore()
  const realTimeStore = useRealTimeStore()

  // Getters
  const activeTanks = computed(() => 
    tanks.value.filter(tank => tank.status === 'Active')
  )

  const inactiveTanks = computed(() => 
    tanks.value.filter(tank => tank.status === 'Inactive')
  )

  const maintenanceDueTanks = computed(() => 
    tanks.value.filter(tank => 
      tank.nextMaintenanceDate && 
      new Date(tank.nextMaintenanceDate) <= new Date()
    )
  )

  const tanksByType = computed(() => {
    const grouped = new Map<string, Tank[]>()
    tanks.value.forEach(tank => {
      const type = tank.tankType
      if (!grouped.has(type)) {
        grouped.set(type, [])
      }
      grouped.get(type)!.push(tank)
    })
    return grouped
  })

  const filteredTanks = computed(() => {
    let filtered = tanks.value

    if (filters.value.searchTerm) {
      const searchLower = filters.value.searchTerm.toLowerCase()
      filtered = filtered.filter(tank =>
        tank.name.toLowerCase().includes(searchLower) ||
        tank.location.building.toLowerCase().includes(searchLower) ||
        tank.location.room.toLowerCase().includes(searchLower)
      )
    }

    if (filters.value.tankType) {
      filtered = filtered.filter(tank => tank.tankType === filters.value.tankType)
    }

    if (filters.value.isActive !== undefined) {
      const targetStatus = filters.value.isActive ? 'Active' : 'Inactive'
      filtered = filtered.filter(tank => tank.status === targetStatus)
    }

    return filtered
  })

  const isLoading = computed(() => state.value.loading)
  const hasError = computed(() => !!state.value.error)
  const isEmpty = computed(() => tanks.value.length === 0 && !isLoading.value)

  // Actions
  const fetchTanks = async (query: GetTanksQuery = {}) => {
    state.value.loading = true
    state.value.error = null

    try {
      const mergedQuery = { ...filters.value, ...query }
      const response = await tankService.getTanks(mergedQuery)

      tanks.value = response.data
      pagination.value = response.pagination
      state.value.lastUpdated = new Date().toISOString()

      // Update filters if provided
      if (Object.keys(query).length > 0) {
        filters.value = { ...filters.value, ...query }
      }

    } catch (error: any) {
      state.value.error = error.message
      notificationStore.addNotification({
        type: 'error',
        title: 'Failed to load tanks',
        message: error.message
      })
    } finally {
      state.value.loading = false
    }
  }

  const fetchTankById = async (id: string): Promise<Tank | null> => {
    // Check if tank is already in store
    const existingTank = tanks.value.find(t => t.id === id)
    if (existingTank) {
      selectedTank.value = existingTank
      return existingTank
    }

    state.value.loading = true
    state.value.error = null

    try {
      const response = await tankService.getTankById(id)
      const tank = response.data

      // Add to tanks array if not exists
      const index = tanks.value.findIndex(t => t.id === id)
      if (index === -1) {
        tanks.value.push(tank)
      } else {
        tanks.value[index] = tank
      }

      selectedTank.value = tank
      return tank

    } catch (error: any) {
      state.value.error = error.message
      notificationStore.addNotification({
        type: 'error',
        title: 'Failed to load tank',
        message: error.message
      })
      return null
    } finally {
      state.value.loading = false
    }
  }

  const createTank = async (command: CreateTankCommand): Promise<Tank | null> => {
    // Optimistic update
    const tempId = `temp-${Date.now()}`
    const optimisticTank: Tank = {
      id: tempId,
      name: command.name,
      capacity: {
        value: command.capacity,
        unit: command.capacityUnit as any
      },
      location: {
        building: command.building,
        room: command.room,
        zone: command.zone,
        latitude: command.latitude,
        longitude: command.longitude,
        fullAddress: `${command.building}, ${command.room}${command.zone ? `, ${command.zone}` : ''}`
      },
      tankType: command.tankType,
      status: 'Inactive',
      sensors: [],
      createdAt: new Date().toISOString(),
      version: 1
    }

    // Add optimistic update
    tanks.value.unshift(optimisticTank)
    
    const rollback = () => {
      const index = tanks.value.findIndex(t => t.id === tempId)
      if (index !== -1) {
        tanks.value.splice(index, 1)
      }
    }

    optimisticUpdates.value.set(tempId, {
      id: tempId,
      type: 'create',
      data: optimisticTank,
      timestamp: new Date().toISOString(),
      rollback
    })

    try {
      const response = await tankService.createTank(command)
      const createdTank = response.data

      // Replace optimistic tank with real tank
      const index = tanks.value.findIndex(t => t.id === tempId)
      if (index !== -1) {
        tanks.value[index] = createdTank
      }

      // Remove optimistic update
      optimisticUpdates.value.delete(tempId)

      notificationStore.addNotification({
        type: 'success',
        title: 'Tank created',
        message: `Tank "${createdTank.name}" has been created successfully`
      })

      return createdTank

    } catch (error: any) {
      // Rollback optimistic update
      rollback()
      optimisticUpdates.value.delete(tempId)

      state.value.error = error.message
      notificationStore.addNotification({
        type: 'error',
        title: 'Failed to create tank',
        message: error.message
      })
      return null
    }
  }

  const updateTank = async (id: string, command: UpdateTankCommand): Promise<Tank | null> => {
    const existingTank = tanks.value.find(t => t.id === id)
    if (!existingTank) return null

    // Optimistic update
    const optimisticTank: Tank = {
      ...existingTank,
      name: command.name,
      capacity: {
        value: command.capacity,
        unit: command.capacityUnit as any
      },
      location: {
        building: command.building,
        room: command.room,
        zone: command.zone,
        latitude: command.latitude,
        longitude: command.longitude,
        fullAddress: `${command.building}, ${command.room}${command.zone ? `, ${command.zone}` : ''}`
      },
      tankType: command.tankType,
      updatedAt: new Date().toISOString(),
      version: existingTank.version + 1
    }

    const index = tanks.value.findIndex(t => t.id === id)
    const originalTank = tanks.value[index]
    tanks.value[index] = optimisticTank

    const rollback = () => {
      tanks.value[index] = originalTank
    }

    optimisticUpdates.value.set(id, {
      id,
      type: 'update',
      data: optimisticTank,
      timestamp: new Date().toISOString(),
      rollback
    })

    try {
      const response = await tankService.updateTank(id, command)
      const updatedTank = response.data

      // Replace optimistic tank with real tank
      tanks.value[index] = updatedTank

      // Update selected tank if it's the same
      if (selectedTank.value?.id === id) {
        selectedTank.value = updatedTank
      }

      // Remove optimistic update
      optimisticUpdates.value.delete(id)

      notificationStore.addNotification({
        type: 'success',
        title: 'Tank updated',
        message: `Tank "${updatedTank.name}" has been updated successfully`
      })

      return updatedTank

    } catch (error: any) {
      // Rollback optimistic update
      rollback()
      optimisticUpdates.value.delete(id)

      state.value.error = error.message
      notificationStore.addNotification({
        type: 'error',
        title: 'Failed to update tank',
        message: error.message
      })
      return null
    }
  }

  const deleteTank = async (id: string): Promise<boolean> => {
    const index = tanks.value.findIndex(t => t.id === id)
    if (index === -1) return false

    const tankToDelete = tanks.value[index]
    
    // Optimistic update - remove from array
    tanks.value.splice(index, 1)

    const rollback = () => {
      tanks.value.splice(index, 0, tankToDelete)
    }

    optimisticUpdates.value.set(id, {
      id,
      type: 'delete',
      data: tankToDelete,
      timestamp: new Date().toISOString(),
      rollback
    })

    try {
      await tankService.deleteTank(id)

      // Clear selected tank if it's the deleted one
      if (selectedTank.value?.id === id) {
        selectedTank.value = null
      }

      // Remove optimistic update
      optimisticUpdates.value.delete(id)

      notificationStore.addNotification({
        type: 'success',
        title: 'Tank deleted',
        message: `Tank "${tankToDelete.name}" has been deleted successfully`
      })

      return true

    } catch (error: any) {
      // Rollback optimistic update
      rollback()
      optimisticUpdates.value.delete(id)

      state.value.error = error.message
      notificationStore.addNotification({
        type: 'error',
        title: 'Failed to delete tank',
        message: error.message
      })
      return false
    }
  }

  // Real-time updates
  const handleRealTimeUpdate = (event: any) => {
    switch (event.eventType) {
      case 'TankCreated':
        // Add new tank if not already present
        if (!tanks.value.find(t => t.id === event.data.id)) {
          tanks.value.push(event.data)
        }
        break

      case 'TankUpdated':
        // Update existing tank
        const updateIndex = tanks.value.findIndex(t => t.id === event.data.id)
        if (updateIndex !== -1) {
          tanks.value[updateIndex] = event.data
          
          // Update selected tank if it's the same
          if (selectedTank.value?.id === event.data.id) {
            selectedTank.value = event.data
          }
        }
        break

      case 'TankDeleted':
        // Remove tank
        const deleteIndex = tanks.value.findIndex(t => t.id === event.data.id)
        if (deleteIndex !== -1) {
          tanks.value.splice(deleteIndex, 1)
          
          // Clear selected tank if it's the deleted one
          if (selectedTank.value?.id === event.data.id) {
            selectedTank.value = null
          }
        }
        break
    }
  }

  // Utility actions
  const clearError = () => {
    state.value.error = null
  }

  const setSelectedTank = (tank: Tank | null) => {
    selectedTank.value = tank
  }

  const updateFilters = (newFilters: Partial<GetTanksQuery>) => {
    filters.value = { ...filters.value, ...newFilters }
  }

  const resetFilters = () => {
    filters.value = {
      searchTerm: '',
      tankType: undefined,
      isActive: undefined,
      sortBy: 'name',
      sortDescending: false
    }
  }

  const refresh = () => {
    return fetchTanks(filters.value)
  }

  // Setup real-time subscriptions
  watch(() => realTimeStore.isConnected, (isConnected) => {
    if (isConnected) {
      realTimeStore.subscribe('TankUpdates', handleRealTimeUpdate)
    }
  }, { immediate: true })

  return {
    // State
    tanks,
    selectedTank,
    state,
    pagination,
    filters,
    optimisticUpdates,

    // Getters
    activeTanks,
    inactiveTanks,
    maintenanceDueTanks,
    tanksByType,
    filteredTanks,
    isLoading,
    hasError,
    isEmpty,

    // Actions
    fetchTanks,
    fetchTankById,
    createTank,
    updateTank,
    deleteTank,
    clearError,
    setSelectedTank,
    updateFilters,
    resetFilters,
    refresh
  }
}, {
  persist: {
    key: 'aquacontrol-tanks',
    storage: localStorage,
    paths: ['filters', 'pagination.pageSize']
  }
})
```

This completes the sophisticated Frontend Layer with:

✅ **Advanced Vue.js 3 Patterns** - Composition API, advanced reactivity  
✅ **Enterprise State Management** - Pinia with optimistic updates  
✅ **Sophisticated HTTP Client** - Axios with interceptors and error handling  
✅ **Type Safety** - Comprehensive TypeScript definitions  
✅ **Real-time Integration** - SignalR connection management  
✅ **Performance Optimization** - Code splitting, lazy loading  
✅ **Developer Experience** - Advanced tooling and configuration  

Next, I'll create the Data Engineering layer with Kafka, TimescaleDB, and ML pipelines.
