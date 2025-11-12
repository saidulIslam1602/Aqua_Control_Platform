import { createApp } from 'vue'
import { createPinia } from 'pinia'
import piniaPluginPersistedstate from 'pinia-plugin-persistedstate'
import ElementPlus from 'element-plus'
import 'element-plus/dist/index.css'
import * as ElementPlusIconsVue from '@element-plus/icons-vue'

import App from './App.vue'
import router from './router'
import { setupErrorReporting } from './utils/errorReporting'
import { setupPerformanceMonitoring } from './utils/performanceMonitoring'

// Environment configuration
const isDevelopment = import.meta.env.DEV
const isProduction = import.meta.env.PROD

// Create app
const app = createApp(App)

// Create pinia store with persistence
const pinia = createPinia()
pinia.use(piniaPluginPersistedstate)

// Use plugins
app.use(pinia)
app.use(router)
app.use(ElementPlus, {
  // Element Plus global configuration
  size: 'default',
  zIndex: 3000,
})

// Register Element Plus icons globally
for (const [key, component] of Object.entries(ElementPlusIconsVue)) {
  app.component(key, component)
}

// Global properties
app.config.globalProperties.$isDev = isDevelopment
app.config.globalProperties.$isProd = isProduction

// Production optimizations
if (isProduction) {
  // Setup error reporting
  setupErrorReporting()
  
  // Setup performance monitoring
  setupPerformanceMonitoring()
}

// Enhanced global error handler
app.config.errorHandler = (err, instance, info) => {
  console.error('Global error:', {
    error: err,
    component: instance?.$?.type?.name || 'Unknown',
    info,
    timestamp: new Date().toISOString(),
    url: window.location.href,
    userAgent: navigator.userAgent
  })
  
  // Send to error reporting service in production
  if (isProduction && window.gtag) {
    window.gtag('event', 'exception', {
      description: (err as Error).message,
      fatal: false
    })
  }
}

// Global warning handler for development
if (isDevelopment) {
  app.config.warnHandler = (msg, instance, trace) => {
    console.warn('Vue warning:', {
      message: msg,
      component: instance?.$?.type?.name || 'Unknown',
      trace,
      timestamp: new Date().toISOString()
    })
  }
}

// Performance monitoring
if (isProduction) {
  // Monitor app initialization time
  const startTime = performance.now()
  
  app.mount('#app')
  
  const endTime = performance.now()
  console.log(`App initialization took ${endTime - startTime} milliseconds`)
  
  // Report to analytics
  if (window.gtag) {
    window.gtag('event', 'timing_complete', {
      name: 'app_initialization',
      value: Math.round(endTime - startTime)
    })
  }
} else {
  // Development mount
  app.mount('#app')
}

// Service Worker registration for production
if (isProduction && 'serviceWorker' in navigator) {
  window.addEventListener('load', () => {
    navigator.serviceWorker.register('/sw.js')
      .then((registration) => {
        console.log('SW registered: ', registration)
      })
      .catch((registrationError) => {
        console.log('SW registration failed: ', registrationError)
      })
  })
}
