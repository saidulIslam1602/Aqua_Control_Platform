interface ErrorReport {
  message: string
  stack?: string
  url: string
  lineNumber?: number
  columnNumber?: number
  timestamp: string
  userAgent: string
  userId?: string
  sessionId: string
}

class ErrorReporter {
  private endpoint = '/api/errors'
  private sessionId: string
  private userId?: string
  private maxRetries = 3
  private retryDelay = 1000

  constructor() {
    this.sessionId = this.generateSessionId()
    this.setupGlobalErrorHandlers()
  }

  private generateSessionId(): string {
    return `session_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`
  }

  private setupGlobalErrorHandlers(): void {
    // Handle JavaScript errors
    window.addEventListener('error', (event) => {
      this.reportError({
        message: event.message,
        stack: event.error?.stack,
        url: event.filename || window.location.href,
        lineNumber: event.lineno,
        columnNumber: event.colno,
        timestamp: new Date().toISOString(),
        userAgent: navigator.userAgent,
        userId: this.userId,
        sessionId: this.sessionId
      })
    })

    // Handle unhandled promise rejections
    window.addEventListener('unhandledrejection', (event) => {
      this.reportError({
        message: `Unhandled Promise Rejection: ${event.reason}`,
        stack: event.reason?.stack,
        url: window.location.href,
        timestamp: new Date().toISOString(),
        userAgent: navigator.userAgent,
        userId: this.userId,
        sessionId: this.sessionId
      })
    })

    // Handle resource loading errors
    window.addEventListener('error', (event) => {
      if (event.target !== window) {
        const target = event.target as HTMLElement
        this.reportError({
          message: `Resource loading error: ${target.tagName} - ${target.getAttribute('src') || target.getAttribute('href')}`,
          url: window.location.href,
          timestamp: new Date().toISOString(),
          userAgent: navigator.userAgent,
          userId: this.userId,
          sessionId: this.sessionId
        })
      }
    }, true)
  }

  setUserId(userId: string): void {
    this.userId = userId
  }

  async reportError(errorReport: ErrorReport, retryCount = 0): Promise<void> {
    try {
      // Don't report errors in development
      if (import.meta.env.DEV) {
        console.warn('Error would be reported in production:', errorReport)
        return
      }

      const response = await fetch(this.endpoint, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(errorReport)
      })

      if (!response.ok) {
        throw new Error(`HTTP ${response.status}: ${response.statusText}`)
      }

      console.log('Error reported successfully')
    } catch (error) {
      console.error('Failed to report error:', error)

      // Retry logic
      if (retryCount < this.maxRetries) {
        setTimeout(() => {
          this.reportError(errorReport, retryCount + 1)
        }, this.retryDelay * Math.pow(2, retryCount)) // Exponential backoff
      } else {
        // Store in localStorage as fallback
        this.storeErrorLocally(errorReport)
      }
    }
  }

  private storeErrorLocally(errorReport: ErrorReport): void {
    try {
      const storedErrors = JSON.parse(localStorage.getItem('pendingErrors') || '[]')
      storedErrors.push(errorReport)
      
      // Keep only the last 10 errors to prevent storage overflow
      if (storedErrors.length > 10) {
        storedErrors.splice(0, storedErrors.length - 10)
      }
      
      localStorage.setItem('pendingErrors', JSON.stringify(storedErrors))
    } catch (error) {
      console.error('Failed to store error locally:', error)
    }
  }

  async sendPendingErrors(): Promise<void> {
    try {
      const pendingErrors = JSON.parse(localStorage.getItem('pendingErrors') || '[]')
      
      if (pendingErrors.length === 0) return

      for (const errorReport of pendingErrors) {
        await this.reportError(errorReport)
      }

      // Clear pending errors after successful submission
      localStorage.removeItem('pendingErrors')
    } catch (error) {
      console.error('Failed to send pending errors:', error)
    }
  }

  reportCustomError(message: string, context?: Record<string, any>): void {
    this.reportError({
      message: `Custom Error: ${message}`,
      stack: new Error().stack,
      url: window.location.href,
      timestamp: new Date().toISOString(),
      userAgent: navigator.userAgent,
      userId: this.userId,
      sessionId: this.sessionId,
      ...context
    })
  }
}

// Create singleton instance
const errorReporter = new ErrorReporter()

export function setupErrorReporting(): void {
  // Send any pending errors from previous sessions
  errorReporter.sendPendingErrors()
}

export function setUserId(userId: string): void {
  errorReporter.setUserId(userId)
}

export function reportCustomError(message: string, context?: Record<string, any>): void {
  errorReporter.reportCustomError(message, context)
}

export { errorReporter }
