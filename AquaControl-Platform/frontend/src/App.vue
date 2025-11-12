<template>
  <div id="app" class="aqua-app">
    <!-- Loading overlay -->
    <div v-if="isInitializing" class="loading-overlay">
      <el-loading-service />
    </div>

    <!-- Main application -->
    <template v-else>
      <!-- Authentication wrapper -->
      <template v-if="authStore.isAuthenticated">
        <AppLayout>
          <router-view />
        </AppLayout>
      </template>
      
      <!-- Login page -->
      <template v-else>
        <LoginView />
      </template>
    </template>

    <!-- Global notifications -->
    <NotificationContainer />
    
    <!-- Real-time connection status -->
    <ConnectionStatus />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, provide } from 'vue'
import { useAuthStore } from '@/stores/authStore'
import { useRealTimeStore } from '@/stores/realTimeStore'
import { useNotificationStore } from '@/stores/notificationStore'
import AppLayout from '@/components/layout/AppLayout.vue'
import LoginView from '@/views/auth/LoginView.vue'
import NotificationContainer from '@/components/common/NotificationContainer.vue'
import ConnectionStatus from '@/components/common/ConnectionStatus.vue'

// Stores
const authStore = useAuthStore()
const realTimeStore = useRealTimeStore()
const notificationStore = useNotificationStore()

// State
const isInitializing = ref(true)

// Provide global services
provide('notificationService', notificationStore)

// Initialize application
onMounted(async () => {
  try {
    // Initialize authentication
    await authStore.initialize()
    
    // Initialize real-time connection if authenticated
    if (authStore.isAuthenticated) {
      await realTimeStore.connect()
    }
    
    // Setup global error handling
    window.addEventListener('unhandledrejection', (event) => {
      console.error('Unhandled promise rejection:', event.reason)
      notificationStore.addNotification({
        type: 'error',
        title: 'Application Error',
        message: 'An unexpected error occurred. Please refresh the page.'
      })
    })
    
  } catch (error) {
    console.error('Failed to initialize application:', error)
    notificationStore.addNotification({
      type: 'error',
      title: 'Initialization Failed',
      message: 'Failed to initialize the application. Please refresh the page.'
    })
  } finally {
    isInitializing.value = false
  }
})
</script>

<style lang="scss">
.aqua-app {
  height: 100vh;
  width: 100vw;
  overflow: hidden;
}

.loading-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(255, 255, 255, 0.9);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 9999;
}
</style>
