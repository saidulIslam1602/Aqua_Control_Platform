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

