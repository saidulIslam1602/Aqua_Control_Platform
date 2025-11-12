import { ref } from 'vue'
import { ElNotification } from 'element-plus'
import type { AxiosError } from 'axios'

interface ErrorDetails {
  message: string
  code?: string
  statusCode?: number
  timestamp: Date
  requestId?: string
}

interface ValidationError {
  field: string
  message: string
}

export function useErrorHandler() {
  const errors = ref<ErrorDetails[]>([])
  const isLoading = ref(false)

  const handleError = (error: unknown, context?: string): ErrorDetails => {
    const errorDetails: ErrorDetails = {
      message: 'An unexpected error occurred',
      timestamp: new Date()
    }

    if (error instanceof Error) {
      errorDetails.message = error.message
    }

    // Handle Axios errors
    if (isAxiosError(error)) {
      const axiosError = error as AxiosError
      errorDetails.statusCode = axiosError.response?.status
      errorDetails.requestId = axiosError.response?.headers['x-request-id']

      if (axiosError.response?.data) {
        const responseData = axiosError.response.data as any
        errorDetails.message = responseData.message || responseData.title || errorDetails.message
        errorDetails.code = responseData.code || responseData.type
      }

      // Handle specific HTTP status codes
      switch (axiosError.response?.status) {
        case 400:
          errorDetails.message = 'Invalid request. Please check your input.'
          break
        case 401:
          errorDetails.message = 'Authentication required. Please log in.'
          break
        case 403:
          errorDetails.message = 'Access denied. You do not have permission to perform this action.'
          break
        case 404:
          errorDetails.message = 'The requested resource was not found.'
          break
        case 409:
          errorDetails.message = 'Conflict detected. The resource may have been modified by another user.'
          break
        case 422:
          errorDetails.message = 'Validation failed. Please check your input.'
          break
        case 429:
          errorDetails.message = 'Too many requests. Please try again later.'
          break
        case 500:
          errorDetails.message = 'Internal server error. Please try again later.'
          break
        case 502:
        case 503:
        case 504:
          errorDetails.message = 'Service temporarily unavailable. Please try again later.'
          break
      }
    }

    // Add context if provided
    if (context) {
      errorDetails.message = `${context}: ${errorDetails.message}`
    }

    errors.value.push(errorDetails)

    // Show notification
    ElNotification({
      title: 'Error',
      message: errorDetails.message,
      type: 'error',
      duration: 5000,
      showClose: true
    })

    // Log error for debugging
    console.error('Error handled:', {
      ...errorDetails,
      originalError: error,
      context
    })

    return errorDetails
  }

  const handleValidationErrors = (validationErrors: ValidationError[]): void => {
    const messages = validationErrors.map(err => `${err.field}: ${err.message}`).join('\n')
    
    ElNotification({
      title: 'Validation Error',
      message: messages,
      type: 'warning',
      duration: 7000,
      showClose: true
    })
  }

  const clearErrors = (): void => {
    errors.value = []
  }

  const clearError = (index: number): void => {
    errors.value.splice(index, 1)
  }

  const showSuccess = (message: string, title = 'Success'): void => {
    ElNotification({
      title,
      message,
      type: 'success',
      duration: 3000,
      showClose: true
    })
  }

  const showWarning = (message: string, title = 'Warning'): void => {
    ElNotification({
      title,
      message,
      type: 'warning',
      duration: 4000,
      showClose: true
    })
  }

  const showInfo = (message: string, title = 'Information'): void => {
    ElNotification({
      title,
      message,
      type: 'info',
      duration: 3000,
      showClose: true
    })
  }

  return {
    errors: readonly(errors),
    isLoading: readonly(isLoading),
    handleError,
    handleValidationErrors,
    clearErrors,
    clearError,
    showSuccess,
    showWarning,
    showInfo
  }
}

function isAxiosError(error: unknown): error is AxiosError {
  return typeof error === 'object' && error !== null && 'isAxiosError' in error
}

function readonly<T>(ref: any): T {
  return ref as T
}
