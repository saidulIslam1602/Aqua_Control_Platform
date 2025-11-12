<template>
  <div v-if="hasError" class="error-boundary">
    <el-alert
      :title="errorTitle"
      type="error"
      :description="errorMessage"
      show-icon
      :closable="false"
    />
    
    <div class="error-actions">
      <el-button @click="retry" type="primary" :loading="retrying">
        <el-icon><Refresh /></el-icon>
        Retry
      </el-button>
      
      <el-button @click="reportError" type="default">
        <el-icon><Warning /></el-icon>
        Report Issue
      </el-button>
      
      <el-button @click="goHome" type="default">
        <el-icon><HomeFilled /></el-icon>
        Go Home
      </el-button>
    </div>
    
    <el-collapse v-if="showDetails" class="error-details">
      <el-collapse-item title="Error Details" name="details">
        <div class="error-info">
          <p><strong>Error ID:</strong> {{ errorId }}</p>
          <p><strong>Timestamp:</strong> {{ errorTimestamp }}</p>
          <p><strong>Component:</strong> {{ errorComponent }}</p>
          <p v-if="errorStack"><strong>Stack Trace:</strong></p>
          <pre v-if="errorStack" class="error-stack">{{ errorStack }}</pre>
        </div>
      </el-collapse-item>
    </el-collapse>
    
    <div class="error-toggle">
      <el-button 
        @click="showDetails = !showDetails" 
        type="text" 
        size="small"
      >
        {{ showDetails ? 'Hide' : 'Show' }} Technical Details
      </el-button>
    </div>
  </div>
  
  <div v-else>
    <slot />
  </div>
</template>

<script setup lang="ts">
import { ref, onErrorCaptured, computed } from 'vue'
import { useRouter } from 'vue-router'
import { ElAlert, ElButton, ElCollapse, ElCollapseItem, ElIcon } from 'element-plus'
import { Refresh, Warning, HomeFilled } from '@element-plus/icons-vue'

interface Props {
  fallbackComponent?: string
  onError?: (error: Error, errorInfo: string) => void
}

const props = withDefaults(defineProps<Props>(), {
  fallbackComponent: 'ErrorFallback'
})

const router = useRouter()

const hasError = ref(false)
const error = ref<Error | null>(null)
const errorInfo = ref<string>('')
const errorId = ref<string>('')
const retrying = ref(false)
const showDetails = ref(false)

const errorTitle = computed(() => {
  if (!error.value) return 'Something went wrong'
  
  if (error.value.name === 'ChunkLoadError') {
    return 'Application Update Available'
  }
  
  return 'Unexpected Error Occurred'
})

const errorMessage = computed(() => {
  if (!error.value) return 'An unexpected error has occurred.'
  
  if (error.value.name === 'ChunkLoadError') {
    return 'The application has been updated. Please refresh the page to load the latest version.'
  }
  
  return error.value.message || 'An unexpected error has occurred.'
})

const errorComponent = computed(() => {
  return errorInfo.value.split('\n')[0] || 'Unknown'
})

const errorStack = computed(() => {
  return error.value?.stack || ''
})

const errorTimestamp = computed(() => {
  return new Date().toISOString()
})

onErrorCaptured((err: Error, _instance, info: string) => {
  hasError.value = true
  error.value = err
  errorInfo.value = info
  errorId.value = generateErrorId()
  
  // Log error for monitoring
  console.error('Error captured by boundary:', {
    error: err,
    errorInfo: info,
    errorId: errorId.value,
    timestamp: errorTimestamp.value,
    component: errorComponent.value
  })
  
  // Call custom error handler if provided
  if (props.onError) {
    props.onError(err, info)
  }
  
  // Send to error reporting service
  reportErrorToService(err, info)
  
  return false // Prevent the error from propagating further
})

const retry = async () => {
  retrying.value = true
  
  try {
    // Wait a bit before retrying
    await new Promise(resolve => setTimeout(resolve, 1000))
    
    // Reset error state
    hasError.value = false
    error.value = null
    errorInfo.value = ''
    errorId.value = ''
    
    // Force component re-render
    await router.go(0)
  } catch (err) {
    console.error('Retry failed:', err)
  } finally {
    retrying.value = false
  }
}

const reportError = () => {
  // Open error reporting form or send to support
  const subject = encodeURIComponent(`Error Report: ${errorId.value}`)
  const body = encodeURIComponent(`
Error ID: ${errorId.value}
Timestamp: ${errorTimestamp.value}
Component: ${errorComponent.value}
Error Message: ${error.value?.message}
User Agent: ${navigator.userAgent}
URL: ${window.location.href}

Stack Trace:
${errorStack.value}
  `)
  
  window.open(`mailto:support@aquacontrol.com?subject=${subject}&body=${body}`)
}

const goHome = () => {
  router.push('/')
}

const generateErrorId = (): string => {
  return `ERR-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`
}

const reportErrorToService = (error: Error, errorInfo: string) => {
  // Send error to monitoring service (e.g., Sentry, LogRocket, etc.)
  if (typeof window !== 'undefined' && window.gtag) {
    window.gtag('event', 'exception', {
      description: error.message,
      fatal: false
    })
  }
  
  // You can also send to your own error tracking endpoint
  fetch('/api/errors', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      errorId: errorId.value,
      message: error.message,
      stack: error.stack,
      componentInfo: errorInfo,
      timestamp: errorTimestamp.value,
      userAgent: navigator.userAgent,
      url: window.location.href
    })
  }).catch(err => {
    console.error('Failed to report error to service:', err)
  })
}
</script>

<style scoped>
.error-boundary {
  padding: 20px;
  margin: 20px;
  border-radius: 8px;
  background-color: #fef0f0;
  border: 1px solid #fde2e2;
}

.error-actions {
  margin-top: 16px;
  display: flex;
  gap: 12px;
  flex-wrap: wrap;
}

.error-details {
  margin-top: 20px;
}

.error-info {
  font-family: 'Courier New', monospace;
  font-size: 12px;
  background-color: #f8f9fa;
  padding: 12px;
  border-radius: 4px;
  border: 1px solid #e9ecef;
}

.error-stack {
  background-color: #f1f3f4;
  padding: 8px;
  border-radius: 4px;
  font-size: 11px;
  overflow-x: auto;
  white-space: pre-wrap;
  word-break: break-all;
}

.error-toggle {
  margin-top: 12px;
  text-align: center;
}

@media (max-width: 768px) {
  .error-boundary {
    margin: 10px;
    padding: 15px;
  }
  
  .error-actions {
    flex-direction: column;
  }
  
  .error-actions .el-button {
    width: 100%;
  }
}
</style>
