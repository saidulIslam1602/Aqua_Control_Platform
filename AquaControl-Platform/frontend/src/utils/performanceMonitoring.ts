interface PerformanceMetric {
  name: string
  value: number
  timestamp: string
  url: string
  userAgent: string
  sessionId: string
}

interface NavigationTiming {
  dns: number
  tcp: number
  request: number
  response: number
  dom: number
  load: number
  total: number
}

class PerformanceMonitor {
  private endpoint = '/api/performance'
  private sessionId: string
  private metrics: PerformanceMetric[] = []
  private observer?: PerformanceObserver

  constructor() {
    this.sessionId = this.generateSessionId()
    this.setupPerformanceObserver()
    this.monitorNavigationTiming()
    this.monitorResourceTiming()
  }

  private generateSessionId(): string {
    return `perf_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`
  }

  private setupPerformanceObserver(): void {
    if ('PerformanceObserver' in window) {
      try {
        this.observer = new PerformanceObserver((list) => {
          for (const entry of list.getEntries()) {
            this.processPerformanceEntry(entry)
          }
        })

        // Observe different types of performance entries
        this.observer.observe({ 
          entryTypes: ['navigation', 'resource', 'measure', 'paint', 'largest-contentful-paint'] 
        })
      } catch (error) {
        console.warn('PerformanceObserver not fully supported:', error)
      }
    }
  }

  private processPerformanceEntry(entry: PerformanceEntry): void {
    const metric: PerformanceMetric = {
      name: `${entry.entryType}_${entry.name}`,
      value: entry.duration || entry.startTime,
      timestamp: new Date().toISOString(),
      url: window.location.href,
      userAgent: navigator.userAgent,
      sessionId: this.sessionId
    }

    this.metrics.push(metric)

    // Report critical metrics immediately
    if (this.isCriticalMetric(entry)) {
      this.reportMetric(metric)
    }
  }

  private isCriticalMetric(entry: PerformanceEntry): boolean {
    // Report slow operations immediately
    if (entry.duration > 1000) return true
    
    // Report paint metrics
    if (entry.entryType === 'paint') return true
    
    // Report LCP
    if (entry.entryType === 'largest-contentful-paint') return true
    
    return false
  }

  private monitorNavigationTiming(): void {
    window.addEventListener('load', () => {
      setTimeout(() => {
        const navigation = performance.getEntriesByType('navigation')[0] as PerformanceNavigationTiming
        
        if (navigation) {
          const timing: NavigationTiming = {
            dns: navigation.domainLookupEnd - navigation.domainLookupStart,
            tcp: navigation.connectEnd - navigation.connectStart,
            request: navigation.responseStart - navigation.requestStart,
            response: navigation.responseEnd - navigation.responseStart,
            dom: navigation.domContentLoadedEventEnd - navigation.responseEnd,
            load: navigation.loadEventEnd - navigation.loadEventStart,
            total: navigation.loadEventEnd - navigation.fetchStart
          }

          this.reportNavigationTiming(timing)
        }
      }, 0)
    })
  }

  private monitorResourceTiming(): void {
    // Monitor slow resources
    setInterval(() => {
      const resources = performance.getEntriesByType('resource')
      const slowResources = resources.filter(resource => resource.duration > 2000)
      
      slowResources.forEach(resource => {
        this.reportMetric({
          name: `slow_resource_${resource.name}`,
          value: resource.duration,
          timestamp: new Date().toISOString(),
          url: window.location.href,
          userAgent: navigator.userAgent,
          sessionId: this.sessionId
        })
      })
    }, 30000) // Check every 30 seconds
  }

  private async reportMetric(metric: PerformanceMetric): Promise<void> {
    try {
      if (import.meta.env.DEV) {
        console.log('Performance metric:', metric)
        return
      }

      await fetch(this.endpoint, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(metric)
      })
    } catch (error) {
      console.error('Failed to report performance metric:', error)
    }
  }

  private reportNavigationTiming(timing: NavigationTiming): void {
    Object.entries(timing).forEach(([key, value]) => {
      this.reportMetric({
        name: `navigation_${key}`,
        value,
        timestamp: new Date().toISOString(),
        url: window.location.href,
        userAgent: navigator.userAgent,
        sessionId: this.sessionId
      })
    })

    // Report to Google Analytics if available
    if (window.gtag) {
      window.gtag('event', 'timing_complete', {
        name: 'page_load',
        value: Math.round(timing.total)
      })
    }
  }

  measureCustom(name: string, fn: () => void | Promise<void>): void {
    const startMark = `${name}_start`
    const endMark = `${name}_end`
    const measureName = `${name}_duration`

    performance.mark(startMark)
    
    const result = fn()
    
    if (result instanceof Promise) {
      result.finally(() => {
        performance.mark(endMark)
        performance.measure(measureName, startMark, endMark)
      })
    } else {
      performance.mark(endMark)
      performance.measure(measureName, startMark, endMark)
    }
  }

  reportWebVitals(): void {
    // Core Web Vitals monitoring
    if ('web-vitals' in window) {
      // This would require importing web-vitals library
      // For now, we'll use basic performance API
      this.measureLCP()
      this.measureFID()
      this.measureCLS()
    }
  }

  private measureLCP(): void {
    if ('PerformanceObserver' in window) {
      try {
        const observer = new PerformanceObserver((list) => {
          const entries = list.getEntries()
          const lastEntry = entries[entries.length - 1]
          
          this.reportMetric({
            name: 'lcp',
            value: lastEntry.startTime,
            timestamp: new Date().toISOString(),
            url: window.location.href,
            userAgent: navigator.userAgent,
            sessionId: this.sessionId
          })
        })
        
        observer.observe({ entryTypes: ['largest-contentful-paint'] })
      } catch (error) {
        console.warn('LCP measurement not supported:', error)
      }
    }
  }

  private measureFID(): void {
    if ('PerformanceObserver' in window) {
      try {
        const observer = new PerformanceObserver((list) => {
          for (const entry of list.getEntries()) {
            const perfEntry = entry as any
            if (perfEntry.processingStart && entry.startTime) {
              const fid = perfEntry.processingStart - entry.startTime
              
              this.reportMetric({
                name: 'fid',
                value: fid,
                timestamp: new Date().toISOString(),
                url: window.location.href,
                userAgent: navigator.userAgent,
                sessionId: this.sessionId
              })
            }
          }
        })
        
        observer.observe({ entryTypes: ['first-input'] })
      } catch (error) {
        console.warn('FID measurement not supported:', error)
      }
    }
  }

  private measureCLS(): void {
    let clsValue = 0
    let sessionValue = 0
    let sessionEntries: PerformanceEntry[] = []

    if ('PerformanceObserver' in window) {
      try {
        const observer = new PerformanceObserver((list) => {
          for (const entry of list.getEntries()) {
            if (!(entry as any).hadRecentInput) {
              const firstSessionEntry = sessionEntries[0]
              const lastSessionEntry = sessionEntries[sessionEntries.length - 1]

              if (sessionValue && 
                  entry.startTime - lastSessionEntry.startTime < 1000 &&
                  entry.startTime - firstSessionEntry.startTime < 5000) {
                sessionValue += (entry as any).value
                sessionEntries.push(entry)
              } else {
                sessionValue = (entry as any).value
                sessionEntries = [entry]
              }

              if (sessionValue > clsValue) {
                clsValue = sessionValue
                
                this.reportMetric({
                  name: 'cls',
                  value: clsValue,
                  timestamp: new Date().toISOString(),
                  url: window.location.href,
                  userAgent: navigator.userAgent,
                  sessionId: this.sessionId
                })
              }
            }
          }
        })
        
        observer.observe({ entryTypes: ['layout-shift'] })
      } catch (error) {
        console.warn('CLS measurement not supported:', error)
      }
    }
  }

  disconnect(): void {
    if (this.observer) {
      this.observer.disconnect()
    }
  }
}

// Create singleton instance
const performanceMonitor = new PerformanceMonitor()

export function setupPerformanceMonitoring(): void {
  performanceMonitor.reportWebVitals()
}

export function measureCustom(name: string, fn: () => void | Promise<void>): void {
  performanceMonitor.measureCustom(name, fn)
}

export { performanceMonitor }
