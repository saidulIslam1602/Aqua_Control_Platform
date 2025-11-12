<template>
  <div class="loading-container" :class="containerClass">
    <div v-if="overlay" class="loading-overlay" />
    
    <div class="loading-content">
      <div class="spinner-wrapper">
        <div v-if="type === 'default'" class="spinner-default">
          <div class="spinner-ring"></div>
          <div class="spinner-ring"></div>
          <div class="spinner-ring"></div>
          <div class="spinner-ring"></div>
        </div>
        
        <div v-else-if="type === 'dots'" class="spinner-dots">
          <div class="dot"></div>
          <div class="dot"></div>
          <div class="dot"></div>
        </div>
        
        <div v-else-if="type === 'pulse'" class="spinner-pulse">
          <div class="pulse-ring"></div>
        </div>
        
        <el-icon v-else-if="type === 'icon'" class="spinner-icon" :size="size">
          <Loading />
        </el-icon>
      </div>
      
      <div v-if="text" class="loading-text" :style="{ fontSize: textSize }">
        {{ text }}
      </div>
      
      <div v-if="description" class="loading-description">
        {{ description }}
      </div>
      
      <div v-if="showProgress && progress !== undefined" class="loading-progress">
        <el-progress 
          :percentage="progress" 
          :stroke-width="4"
          :show-text="false"
        />
        <span class="progress-text">{{ progress }}%</span>
      </div>
      
      <div v-if="cancellable" class="loading-actions">
        <el-button @click="$emit('cancel')" size="small" type="default">
          Cancel
        </el-button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { ElIcon, ElProgress, ElButton } from 'element-plus'
import { Loading } from '@element-plus/icons-vue'

interface Props {
  type?: 'default' | 'dots' | 'pulse' | 'icon'
  size?: 'small' | 'medium' | 'large'
  text?: string
  description?: string
  overlay?: boolean
  fullscreen?: boolean
  progress?: number
  showProgress?: boolean
  cancellable?: boolean
  color?: string
}

const props = withDefaults(defineProps<Props>(), {
  type: 'default',
  size: 'medium',
  overlay: false,
  fullscreen: false,
  showProgress: false,
  cancellable: false,
  color: '#409eff'
})

defineEmits<{
  cancel: []
}>()

const containerClass = computed(() => ({
  'loading-fullscreen': props.fullscreen,
  'loading-overlay-container': props.overlay,
  [`loading-${props.size}`]: true
}))

const textSize = computed(() => {
  switch (props.size) {
    case 'small': return '12px'
    case 'large': return '16px'
    default: return '14px'
  }
})
</script>

<style scoped>
.loading-container {
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
  min-height: 100px;
}

.loading-fullscreen {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  z-index: 9999;
  background-color: rgba(255, 255, 255, 0.9);
}

.loading-overlay-container {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  z-index: 1000;
}

.loading-overlay {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(255, 255, 255, 0.8);
  backdrop-filter: blur(2px);
}

.loading-content {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
  z-index: 1001;
  padding: 20px;
}

.spinner-wrapper {
  display: flex;
  align-items: center;
  justify-content: center;
}

/* Default Ring Spinner */
.spinner-default {
  display: inline-block;
  position: relative;
  width: 40px;
  height: 40px;
}

.loading-small .spinner-default {
  width: 24px;
  height: 24px;
}

.loading-large .spinner-default {
  width: 56px;
  height: 56px;
}

.spinner-ring {
  box-sizing: border-box;
  display: block;
  position: absolute;
  width: 100%;
  height: 100%;
  border: 3px solid transparent;
  border-top-color: v-bind(color);
  border-radius: 50%;
  animation: spin 1.2s cubic-bezier(0.5, 0, 0.5, 1) infinite;
}

.spinner-ring:nth-child(1) { animation-delay: -0.45s; }
.spinner-ring:nth-child(2) { animation-delay: -0.3s; }
.spinner-ring:nth-child(3) { animation-delay: -0.15s; }

/* Dots Spinner */
.spinner-dots {
  display: flex;
  gap: 4px;
}

.dot {
  width: 8px;
  height: 8px;
  background-color: v-bind(color);
  border-radius: 50%;
  animation: dot-bounce 1.4s ease-in-out infinite both;
}

.loading-small .dot {
  width: 6px;
  height: 6px;
}

.loading-large .dot {
  width: 12px;
  height: 12px;
}

.dot:nth-child(1) { animation-delay: -0.32s; }
.dot:nth-child(2) { animation-delay: -0.16s; }

/* Pulse Spinner */
.spinner-pulse {
  position: relative;
  width: 40px;
  height: 40px;
}

.loading-small .spinner-pulse {
  width: 24px;
  height: 24px;
}

.loading-large .spinner-pulse {
  width: 56px;
  height: 56px;
}

.pulse-ring {
  position: absolute;
  width: 100%;
  height: 100%;
  border: 3px solid v-bind(color);
  border-radius: 50%;
  animation: pulse 2s ease-in-out infinite;
}

/* Icon Spinner */
.spinner-icon {
  animation: spin 1s linear infinite;
  color: v-bind(color);
}

/* Text and Description */
.loading-text {
  font-weight: 500;
  color: #606266;
  text-align: center;
}

.loading-description {
  font-size: 12px;
  color: #909399;
  text-align: center;
  max-width: 300px;
}

/* Progress */
.loading-progress {
  width: 200px;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.progress-text {
  text-align: center;
  font-size: 12px;
  color: #909399;
}

/* Actions */
.loading-actions {
  margin-top: 8px;
}

/* Animations */
@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

@keyframes dot-bounce {
  0%, 80%, 100% {
    transform: scale(0);
  }
  40% {
    transform: scale(1);
  }
}

@keyframes pulse {
  0% {
    transform: scale(0);
    opacity: 1;
  }
  100% {
    transform: scale(1);
    opacity: 0;
  }
}

/* Responsive Design */
@media (max-width: 768px) {
  .loading-content {
    padding: 15px;
  }
  
  .loading-progress {
    width: 150px;
  }
  
  .loading-description {
    max-width: 250px;
  }
}
</style>
