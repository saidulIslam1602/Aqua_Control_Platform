<template>
  <div class="modern-sensor-detail-view">
    <!-- Loading State -->
    <div v-if="isLoading" class="loading-container">
      <el-icon class="is-loading" :size="48"><Loading /></el-icon>
      <p>Loading sensor details...</p>
    </div>

    <!-- Error State -->
    <div v-else-if="!sensor" class="error-container">
      <el-icon :size="64"><WarningFilled /></el-icon>
      <h2>Sensor Not Found</h2>
      <p>The sensor you're looking for doesn't exist or has been removed.</p>
      <button class="btn btn-primary" @click="router.push('/sensors')">
        <el-icon><ArrowLeft /></el-icon>
        <span>Back to Sensors</span>
      </button>
    </div>

    <!-- Sensor Detail Content -->
    <div v-else>
      <!-- Page Header -->
      <section class="page-header section--sm">
        <div class="container">
          <div class="header-content">
            <div class="header-left">
              <button class="btn btn-text" @click="router.push('/sensors')">
                <el-icon><ArrowLeft /></el-icon>
                <span>Back to Sensors</span>
              </button>
              <div class="header-text">
                <h1 class="modern-heading modern-heading--h1">{{ sensor.sensorType }} Sensor</h1>
                <p class="modern-text modern-text--lead">
                  {{ sensor.model }} - {{ sensor.serialNumber }}
                </p>
              </div>
            </div>
            <div class="header-actions">
              <el-tag :type="getStatusType(sensor.status)" size="large">
                {{ sensor.status }}
              </el-tag>
            </div>
          </div>
        </div>
      </section>

      <!-- Sensor Overview -->
      <section class="section bg--secondary">
        <div class="container">
          <div class="sensor-overview-grid">
            <!-- Sensor Info Card -->
            <ModernCard variant="glass">
              <template #header>
                <h3>Sensor Information</h3>
              </template>
              <div class="info-grid">
                <div class="info-item">
                  <span class="label">Type</span>
                  <span class="value">{{ sensor.sensorType }}</span>
                </div>
                <div class="info-item">
                  <span class="label">Model</span>
                  <span class="value">{{ sensor.model }}</span>
                </div>
                <div class="info-item">
                  <span class="label">Manufacturer</span>
                  <span class="value">{{ sensor.manufacturer }}</span>
                </div>
                <div class="info-item">
                  <span class="label">Serial Number</span>
                  <span class="value">{{ sensor.serialNumber }}</span>
                </div>
                <div class="info-item">
                  <span class="label">Tank</span>
                  <span class="value">
                    <router-link :to="`/tanks/${sensor.tankId}`" class="link">
                      {{ getTankName(sensor.tankId) }}
                    </router-link>
                  </span>
                </div>
                <div class="info-item">
                  <span class="label">Status</span>
                  <el-tag :type="getStatusType(sensor.status)">{{ sensor.status }}</el-tag>
                </div>
                <div class="info-item">
                  <span class="label">Installation Date</span>
                  <span class="value">{{ formatDate(sensor.installationDate) }}</span>
                </div>
                <div class="info-item">
                  <span class="label">Last Calibration</span>
                  <span class="value">{{ formatDate(sensor.calibrationDate) || 'Never' }}</span>
                </div>
              </div>
            </ModernCard>

            <!-- Quick Actions Card -->
            <ModernCard variant="glass">
              <template #header>
                <h3>Quick Actions</h3>
              </template>
              <div class="actions-grid">
                <button class="action-card" @click="testSensor">
                  <el-icon :size="32"><Connection /></el-icon>
                  <span>Test Connection</span>
                </button>
                <button class="action-card" @click="showCalibrationModal = true">
                  <el-icon :size="32"><Setting /></el-icon>
                  <span>Calibrate</span>
                </button>
                <button class="action-card" @click="exportData">
                  <el-icon :size="32"><Download /></el-icon>
                  <span>Export Data</span>
                </button>
                <button class="action-card danger" @click="confirmDelete">
                  <el-icon :size="32"><Delete /></el-icon>
                  <span>Remove Sensor</span>
                </button>
              </div>
            </ModernCard>
          </div>
        </div>
      </section>

      <!-- Sensor Readings -->
      <section class="section">
        <div class="container">
          <ModernCard>
            <template #header>
              <div class="card-header-with-actions">
                <h3>Recent Readings</h3>
                <div class="header-actions">
                  <el-button @click="refreshReadings" :loading="loadingReadings">
                    <el-icon><Refresh /></el-icon>
                    Refresh
                  </el-button>
                </div>
              </div>
            </template>
            <div class="readings-placeholder">
              <el-icon :size="64"><TrendCharts /></el-icon>
              <p>Real-time sensor readings will be displayed here</p>
              <small>Connect to sensor data stream to view live readings</small>
            </div>
          </ModernCard>
        </div>
      </section>
    </div>

    <!-- Calibration Modal -->
    <CalibrationModal
      v-model="showCalibrationModal"
      :sensor="sensor"
      @calibrated="handleCalibrated"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useSensorStore } from '@/stores/sensorStore'
import { useTankStore } from '@/stores/tankStore'
import ModernCard from '@/components/common/ModernCard.vue'
import CalibrationModal from '@/components/modals/CalibrationModal.vue'
import {
  Loading,
  WarningFilled,
  ArrowLeft,
  Connection,
  Setting,
  Download,
  Delete,
  Refresh,
  TrendCharts
} from '@element-plus/icons-vue'
import { ElMessage, ElMessageBox } from 'element-plus'

interface Props {
  id: string
}

const props = defineProps<Props>()
const router = useRouter()
const sensorStore = useSensorStore()
const tankStore = useTankStore()

const isLoading = ref(true)
const loadingReadings = ref(false)
const showCalibrationModal = ref(false)

const sensor = computed(() => sensorStore.selectedSensor)

const getTankName = (tankId: string) => {
  const tank = tankStore.tanks.find(t => t.id === tankId)
  return tank ? tank.name : 'Unknown Tank'
}

const getStatusType = (status: string) => {
  const statusMap: Record<string, any> = {
    Online: 'success',
    Offline: 'info',
    Error: 'danger',
    Calibrating: 'warning',
    Maintenance: 'warning'
  }
  return statusMap[status] || 'info'
}

const formatDate = (dateString?: string) => {
  if (!dateString) return null
  return new Date(dateString).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  })
}

const testSensor = async () => {
  if (!sensor.value) return
  await sensorStore.testSensor(sensor.value.id)
}

const exportData = async () => {
  if (!sensor.value) return
  
  const endDate = new Date()
  const startDate = new Date()
  startDate.setDate(startDate.getDate() - 30) // Last 30 days
  
  await sensorStore.exportSensorData(sensor.value.id, startDate, endDate, 'csv')
}

const confirmDelete = async () => {
  if (!sensor.value) return
  
  try {
    await ElMessageBox.confirm(
      'Are you sure you want to remove this sensor? This action cannot be undone.',
      'Confirm Removal',
      {
        confirmButtonText: 'Remove',
        cancelButtonText: 'Cancel',
        type: 'warning',
      }
    )
    
    const success = await sensorStore.deleteSensor(sensor.value.id)
    if (success) {
      router.push('/sensors')
    }
  } catch {
    // User cancelled
  }
}

const refreshReadings = async () => {
  if (!sensor.value) return
  loadingReadings.value = true
  try {
    await sensorStore.fetchSensorReadings(sensor.value.id)
  } finally {
    loadingReadings.value = false
  }
}

const handleCalibrated = () => {
  ElMessage.success('Sensor calibrated successfully')
  // Refresh sensor data
  if (sensor.value) {
    sensorStore.fetchSensorById(sensor.value.id)
  }
}

onMounted(async () => {
  isLoading.value = true
  try {
    await Promise.all([
      sensorStore.fetchSensorById(props.id),
      tankStore.fetchTanks()
    ])
  } finally {
    isLoading.value = false
  }
})
</script>

<style lang="scss" scoped>
@import '@/styles/design-system/index.scss';

.modern-sensor-detail-view {
  min-height: 100vh;
  background: var(--color-neutral-50);
}

.loading-container,
.error-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: 60vh;
  text-align: center;
  padding: var(--space-10);
  
  .el-icon {
    margin-bottom: var(--space-6);
    color: var(--color-text-tertiary);
  }
  
  h2 {
    font-size: var(--font-size-2xl);
    color: var(--color-text-primary);
    margin: var(--space-4) 0;
  }
  
  p {
    color: var(--color-text-secondary);
    margin-bottom: var(--space-6);
  }
}

.page-header {
  background: linear-gradient(135deg, #1e3a8a 0%, #3b82f6 100%);
  
  .header-content {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: var(--space-6);
    
    @media (max-width: 768px) {
      flex-direction: column;
      align-items: flex-start;
    }
  }
  
  .header-left {
    display: flex;
    flex-direction: column;
    gap: var(--space-4);
  }
  
  .header-text {
    h1 {
      color: white !important;
      margin-bottom: var(--space-2);
    }
    
    p {
      color: rgba(255, 255, 255, 0.9) !important;
    }
  }
}

.sensor-overview-grid {
  display: grid;
  grid-template-columns: 2fr 1fr;
  gap: var(--space-6);
  
  @media (max-width: 1024px) {
    grid-template-columns: 1fr;
  }
}

.info-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: var(--space-5);
  
  @media (max-width: 768px) {
    grid-template-columns: 1fr;
  }
}

.info-item {
  display: flex;
  flex-direction: column;
  gap: var(--space-2);
  
  .label {
    font-size: var(--font-size-sm);
    color: #6b7280;
    font-weight: var(--font-weight-medium);
  }
  
  .value {
    font-size: var(--font-size-base);
    color: #111827;
    font-weight: var(--font-weight-semibold);
  }
  
  .link {
    color: var(--color-primary-600);
    text-decoration: none;
    
    &:hover {
      text-decoration: underline;
    }
  }
}

.actions-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: var(--space-4);
}

.action-card {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--space-3);
  padding: var(--space-6);
  background: var(--color-neutral-50);
  border: 2px solid var(--color-neutral-200);
  border-radius: var(--border-radius-lg);
  cursor: pointer;
  transition: all var(--transition-base);
  
  &:hover {
    background: white;
    border-color: var(--color-primary-500);
    transform: translateY(-2px);
    box-shadow: var(--shadow-md);
  }
  
  &.danger:hover {
    border-color: var(--color-danger);
    color: var(--color-danger);
  }
  
  span {
    font-size: var(--font-size-sm);
    font-weight: var(--font-weight-semibold);
  }
}

.card-header-with-actions {
  display: flex;
  align-items: center;
  justify-content: space-between;
  
  h3 {
    font-size: var(--font-size-xl);
    font-weight: var(--font-weight-semibold);
    color: #111827;
    margin: 0;
  }
}

.readings-placeholder {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: var(--space-20);
  text-align: center;
  color: var(--color-text-tertiary);
  
  .el-icon {
    margin-bottom: var(--space-4);
  }
  
  p {
    font-size: var(--font-size-lg);
    margin-bottom: var(--space-2);
  }
  
  small {
    font-size: var(--font-size-sm);
  }
}
</style>

