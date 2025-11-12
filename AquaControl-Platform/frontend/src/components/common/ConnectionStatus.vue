<template>
  <el-tooltip
    :content="tooltipText"
    placement="left"
  >
    <div
      class="connection-status"
      :class="statusClass"
      @click="handleReconnect"
    >
      <el-icon>
        <component :is="statusIcon" />
      </el-icon>
    </div>
  </el-tooltip>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRealTimeStore } from '@/stores/realTimeStore'
import { Connection, Close, Refresh } from '@element-plus/icons-vue'

const realTimeStore = useRealTimeStore()

const statusClass = computed(() => {
  if (realTimeStore.isConnected) return 'connected'
  if (realTimeStore.isConnecting) return 'connecting'
  return 'disconnected'
})

const statusIcon = computed(() => {
  if (realTimeStore.isConnected) return Connection
  if (realTimeStore.isConnecting) return Refresh
  return Close
})

const tooltipText = computed(() => {
  if (realTimeStore.isConnected) return 'Real-time connection active'
  if (realTimeStore.isConnecting) return 'Connecting...'
  return 'Real-time connection lost. Click to reconnect.'
})

const handleReconnect = async () => {
  if (!realTimeStore.isConnected && !realTimeStore.isConnecting) {
    await realTimeStore.connect()
  }
}
</script>

<style lang="scss" scoped>
.connection-status {
  position: fixed;
  bottom: 20px;
  right: 20px;
  width: 40px;
  height: 40px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  z-index: 2000;
  transition: all 0.3s ease;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
  
  &.connected {
    background: var(--el-color-success);
    color: white;
    
    &:hover {
      transform: scale(1.1);
    }
  }
  
  &.connecting {
    background: var(--el-color-warning);
    color: white;
    animation: pulse 1.5s infinite;
  }
  
  &.disconnected {
    background: var(--el-color-danger);
    color: white;
    
    &:hover {
      transform: scale(1.1);
    }
  }
}

@keyframes pulse {
  0%, 100% {
    opacity: 1;
  }
  50% {
    opacity: 0.5;
  }
}
</style>

