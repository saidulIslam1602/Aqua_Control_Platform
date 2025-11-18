<template>
  <div class="modern-sensors-view">
    <!-- Page Header -->
    <section class="page-header section--sm">
      <div class="container">
        <div class="header-content">
          <div class="header-text">
            <h1 class="modern-heading modern-heading--h1">Sensor Monitoring</h1>
            <p class="modern-text modern-text--lead">
              Real-time monitoring and management of all sensor devices
            </p>
          </div>
          <button class="btn btn-primary btn-large" @click="addNewSensor">
            <el-icon><Plus /></el-icon>
            <span>Add Sensor</span>
          </button>
        </div>
      </div>
    </section>

    <!-- Filters and Stats -->
    <section class="filters-section bg--secondary">
      <div class="container">
        <!-- Filters -->
        <div class="filters-wrapper">
          <div class="filter-item filter-search">
            <el-input
              v-model="searchTerm"
              placeholder="Search sensors..."
              :prefix-icon="Search"
              size="large"
              clearable
            />
          </div>

          <div class="filter-item">
            <el-select
              v-model="selectedType"
              placeholder="Sensor Type"
              size="large"
              clearable
            >
              <el-option label="All Types" value="" />
              <el-option label="Temperature" value="Temperature" />
              <el-option label="pH" value="pH" />
              <el-option label="Oxygen" value="Oxygen" />
              <el-option label="Salinity" value="Salinity" />
              <el-option label="Turbidity" value="Turbidity" />
            </el-select>
          </div>

          <div class="filter-item">
            <el-select
              v-model="selectedStatus"
              placeholder="Status"
              size="large"
              clearable
            >
              <el-option label="All Status" value="" />
              <el-option label="Active" value="Active" />
              <el-option label="Warning" value="Warning" />
              <el-option label="Error" value="Error" />
              <el-option label="Offline" value="Offline" />
            </el-select>
          </div>

          <div class="filter-item">
            <el-select
              v-model="selectedTank"
              placeholder="Tank"
              size="large"
              clearable
            >
              <el-option label="All Tanks" value="" />
              <el-option 
                v-for="tank in tanks" 
                :key="tank.id"
                :label="tank.name" 
                :value="tank.id" 
              />
            </el-select>
          </div>
        </div>

        <!-- Stats Overview -->
        <div class="stats-overview">
          <div class="stat-card">
            <div class="stat-icon" style="--stat-color: var(--color-primary-500)">
              <el-icon :size="28"><Monitor /></el-icon>
            </div>
            <div class="stat-content">
              <div class="stat-value">{{ totalSensors }}</div>
              <div class="stat-label">Total Sensors</div>
            </div>
          </div>
          <div class="stat-card">
            <div class="stat-icon" style="--stat-color: var(--color-success)">
              <el-icon :size="28"><CircleCheck /></el-icon>
            </div>
            <div class="stat-content">
              <div class="stat-value">{{ activeSensors }}</div>
              <div class="stat-label">Active</div>
            </div>
          </div>
          <div class="stat-card">
            <div class="stat-icon" style="--stat-color: var(--color-warning)">
              <el-icon :size="28"><Warning /></el-icon>
            </div>
            <div class="stat-content">
              <div class="stat-value">{{ warningSensors }}</div>
              <div class="stat-label">Warnings</div>
            </div>
          </div>
          <div class="stat-card">
            <div class="stat-icon" style="--stat-color: var(--color-danger)">
              <el-icon :size="28"><CircleClose /></el-icon>
            </div>
            <div class="stat-content">
              <div class="stat-value">{{ offlineSensors }}</div>
              <div class="stat-label">Offline</div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- Sensors Grid -->
    <section class="section">
      <div class="container">
        <!-- Loading State -->
        <div v-if="isLoading" class="loading-state">
          <el-icon class="is-loading" :size="48"><Loading /></el-icon>
          <p>Loading sensors...</p>
        </div>

        <!-- Empty State -->
        <div v-else-if="filteredSensors.length === 0" class="empty-state">
          <el-icon :size="64"><Box /></el-icon>
          <h3>No sensors found</h3>
          <p>{{ searchTerm ? 'Try adjusting your filters' : 'Get started by adding your first sensor' }}</p>
          <button v-if="!searchTerm" class="btn btn-primary" @click="addNewSensor">
            <el-icon><Plus /></el-icon>
            <span>Add Sensor</span>
          </button>
        </div>

        <!-- Sensors Grid -->
        <div v-else class="sensors-grid grid grid--3">
          <ModernCard
            v-for="sensor in filteredSensors"
            :key="sensor.id"
            variant="glass"
            :hover="true"
            class="sensor-card scroll-animate"
          >
            <div class="sensor-header">
              <div class="sensor-icon-wrapper" :class="`sensor-type-${sensor.sensorType.toLowerCase()}`">
                <el-icon :size="32">
                  <component :is="getSensorIcon(sensor.sensorType)" />
                </el-icon>
              </div>
              <div class="sensor-status-badge" :class="`status-${sensor.status.toLowerCase()}`">
                <span class="status-dot"></span>
                <span>{{ sensor.status }}</span>
              </div>
            </div>

            <div class="sensor-info">
              <h3 class="sensor-name">{{ sensor.sensorType }}</h3>
              <p class="sensor-model">{{ sensor.model }}</p>
              <p class="sensor-location">
                <el-icon><Location /></el-icon>
                <span>{{ getTankName(sensor.tankId) }}</span>
              </p>
            </div>

            <div class="sensor-reading">
              <div class="no-data">
                <el-icon><Monitor /></el-icon>
                <span>Ready to monitor</span>
              </div>
            </div>

            <template #footer>
              <div class="sensor-actions">
                <button 
                  class="action-btn" 
                  @click="viewSensorDetails(sensor.id)"
                  title="View Details"
                >
                  <el-icon><View /></el-icon>
                </button>
                <button 
                  class="action-btn" 
                  @click="viewSensorHistory(sensor.id)"
                  title="View History"
                >
                  <el-icon><TrendCharts /></el-icon>
                </button>
                <button 
                  class="action-btn" 
                  @click="calibrateSensor(sensor.id)"
                  title="Calibrate"
                >
                  <el-icon><Setting /></el-icon>
                </button>
                <button 
                  class="action-btn action-btn--danger" 
                  @click="removeSensor(sensor.id)"
                  title="Remove"
                >
                  <el-icon><Delete /></el-icon>
                </button>
              </div>
            </template>
          </ModernCard>
        </div>
      </div>
    </section>

    <!-- Add Sensor Modal -->
    <el-dialog
      v-model="showAddModal"
      title="Add New Sensor"
      width="600px"
      :close-on-click-modal="false"
    >
      <el-form
        ref="sensorFormRef"
        :model="sensorForm"
        :rules="sensorFormRules"
        label-position="top"
      >
        <el-form-item label="Sensor Type" prop="sensorType">
          <el-select
            v-model="sensorForm.sensorType"
            placeholder="Select sensor type"
            size="large"
            style="width: 100%"
          >
            <el-option label="Temperature" value="Temperature" />
            <el-option label="pH" value="pH" />
            <el-option label="Oxygen" value="Oxygen" />
            <el-option label="Salinity" value="Salinity" />
            <el-option label="Turbidity" value="Turbidity" />
          </el-select>
        </el-form-item>

        <el-form-item label="Model" prop="model">
          <el-input
            v-model="sensorForm.model"
            placeholder="Enter sensor model"
            size="large"
          />
        </el-form-item>

        <el-form-item label="Tank" prop="tankId">
          <el-select
            v-model="sensorForm.tankId"
            placeholder="Select tank"
            size="large"
            style="width: 100%"
          >
            <el-option
              v-for="tank in tanks"
              :key="tank.id"
              :label="tank.name"
              :value="tank.id"
            />
          </el-select>
        </el-form-item>

        <el-form-item label="Serial Number" prop="serialNumber">
          <el-input
            v-model="sensorForm.serialNumber"
            placeholder="Enter serial number"
            size="large"
          />
        </el-form-item>

        <div class="form-row">
          <el-form-item label="Min Value" prop="minValue">
            <el-input
              v-model.number="sensorForm.minValue"
              type="number"
              placeholder="Minimum value"
              size="large"
            />
          </el-form-item>

          <el-form-item label="Max Value" prop="maxValue">
            <el-input
              v-model.number="sensorForm.maxValue"
              type="number"
              placeholder="Maximum value"
              size="large"
            />
          </el-form-item>
        </div>

        <el-form-item label="Calibration Date">
          <el-date-picker
            v-model="sensorForm.calibrationDate"
            type="date"
            placeholder="Select calibration date"
            size="large"
            style="width: 100%"
          />
        </el-form-item>
      </el-form>

      <template #footer>
        <div class="dialog-footer">
          <button class="btn btn-outline" @click="closeAddModal">Cancel</button>
          <button class="btn btn-primary" @click="addSensor" :disabled="isAdding">
            <el-icon v-if="isAdding" class="is-loading"><Loading /></el-icon>
            <span>{{ isAdding ? 'Adding...' : 'Add Sensor' }}</span>
          </button>
        </div>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useTankStore } from '@/stores/tankStore'
import { useSensorStore } from '@/stores/sensorStore'
import ModernCard from '@/components/common/ModernCard.vue'
import {
  Plus,
  Search,
  Monitor,
  CircleCheck,
  Warning,
  CircleClose,
  Loading,
  Box,
  Location,
  Clock,
  WarningFilled,
  View,
  TrendCharts,
  Setting,
  Delete,
  Odometer,
  Aim,
  Histogram,
  Compass
} from '@element-plus/icons-vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import type { FormInstance } from 'element-plus'

// Router and Store
const router = useRouter()
const tankStore = useTankStore()
const sensorStore = useSensorStore()

// State
const searchTerm = ref('')
const selectedType = ref('')
const selectedStatus = ref('')
const selectedTank = ref('')
const showAddModal = ref(false)
const isAdding = ref(false)
const sensorFormRef = ref()

// Sensor form data
const sensorForm = ref({
  sensorType: '',
  model: '',
  tankId: '',
  serialNumber: '',
  minValue: 0,
  maxValue: 100,
  unit: '',
  calibrationDate: new Date(),
  manufacturer: '',
  description: ''
})

// Form validation rules
const sensorFormRules = {
  sensorType: [
    { required: true, message: 'Please select sensor type', trigger: 'change' }
  ],
  model: [
    { required: true, message: 'Please enter model', trigger: 'blur' },
    { min: 2, max: 50, message: 'Model should be 2-50 characters', trigger: 'blur' }
  ],
  tankId: [
    { required: true, message: 'Please select tank', trigger: 'change' }
  ],
  serialNumber: [
    { required: true, message: 'Please enter serial number', trigger: 'blur' }
  ],
  minValue: [
    { required: true, message: 'Please enter minimum value', trigger: 'blur' },
    { type: 'number', message: 'Must be a number', trigger: 'blur' }
  ],
  maxValue: [
    { required: true, message: 'Please enter maximum value', trigger: 'blur' },
    { type: 'number', message: 'Must be a number', trigger: 'blur' }
  ]
}

// Computed
const tanks = computed(() => tankStore.tanks)
const isLoading = computed(() => sensorStore.isLoading)

// Apply local filters on top of store filters
const filteredSensors = computed(() => {
  let sensors = sensorStore.sensors

  if (searchTerm.value) {
    const search = searchTerm.value.toLowerCase()
    sensors = sensors.filter(sensor =>
      sensor.sensorType.toLowerCase().includes(search) ||
      sensor.model.toLowerCase().includes(search)
    )
  }

  if (selectedType.value) {
    sensors = sensors.filter(sensor => sensor.sensorType === selectedType.value)
  }

  if (selectedStatus.value) {
    sensors = sensors.filter(sensor => sensor.status === selectedStatus.value)
  }

  if (selectedTank.value) {
    sensors = sensors.filter(sensor => sensor.tankId === selectedTank.value)
  }

  return sensors
})

const totalSensors = computed(() => sensorStore.sensors.length)
const activeSensors = computed(() => sensorStore.activeSensors.length)
const warningSensors = computed(() => sensorStore.sensors.filter(s => s.status === 'Error').length)
const offlineSensors = computed(() => sensorStore.offlineSensors.length)

// Methods
const getSensorIcon = (type: string) => {
  const iconMap: Record<string, any> = {
    Temperature: Odometer,
    pH: Odometer,
    Oxygen: Aim,
    Salinity: Compass,
    Turbidity: Histogram,
    default: Monitor
  }
  return iconMap[type] || iconMap.default
}

const getTankName = (tankId: string) => {
  const tank = tanks.value.find(t => t.id === tankId)
  return tank ? tank.name : 'Unknown Tank'
}

const formatRelativeTime = (timestamp: string) => {
  const date = new Date(timestamp)
  const now = new Date()
  const diffMs = now.getTime() - date.getTime()
  const diffMins = Math.floor(diffMs / 60000)
  const diffHours = Math.floor(diffMs / 3600000)
  const diffDays = Math.floor(diffMs / 86400000)

  if (diffMins < 1) return 'Just now'
  if (diffMins < 60) return `${diffMins}m ago`
  if (diffHours < 24) return `${diffHours}h ago`
  return `${diffDays}d ago`
}

const addNewSensor = () => {
  showAddModal.value = true
}

const closeAddModal = () => {
  showAddModal.value = false
  sensorFormRef.value?.resetFields()
  sensorForm.value = {
    sensorType: '',
    model: '',
    tankId: '',
    serialNumber: '',
    minValue: 0,
    maxValue: 100,
    unit: '',
    calibrationDate: new Date(),
    manufacturer: '',
    description: ''
  }
}

const addSensor = async () => {
  if (!sensorFormRef.value) return

  await sensorFormRef.value.validate(async (valid: boolean) => {
    if (valid) {
      isAdding.value = true
      try {
        // Determine unit based on sensor type
        let unit = ''
        switch (sensorForm.value.sensorType) {
          case 'Temperature':
            unit = '°C'
            break
          case 'pH':
            unit = 'pH'
            break
          case 'Oxygen':
          case 'DissolvedOxygen':
            unit = 'mg/L'
            break
          case 'Salinity':
            unit = 'ppt'
            break
          case 'Turbidity':
            unit = 'NTU'
            break
          default:
            unit = 'unit'
        }

        await sensorStore.createSensor({
          sensorType: sensorForm.value.sensorType,
          model: sensorForm.value.model,
          tankId: sensorForm.value.tankId,
          serialNumber: sensorForm.value.serialNumber,
          minValue: sensorForm.value.minValue,
          maxValue: sensorForm.value.maxValue,
          unit: unit,
          calibrationDate: sensorForm.value.calibrationDate,
          manufacturer: sensorForm.value.manufacturer,
          description: sensorForm.value.description
        })
        
        closeAddModal()
      } catch (error: any) {
        ElMessage.error(error.message || 'Failed to add sensor')
      } finally {
        isAdding.value = false
      }
    }
  })
}

const viewSensorDetails = (id: string) => {
  router.push(`/sensors/${id}`)
}

const viewSensorHistory = (id: string) => {
  router.push(`/analytics?sensor=${id}`)
}

const calibrateSensor = async (id: string) => {
  try {
    await ElMessageBox.prompt(
      'Enter calibration value',
      'Calibrate Sensor',
      {
        confirmButtonText: 'Calibrate',
        cancelButtonText: 'Cancel',
        inputPattern: /^-?\d+\.?\d*$/,
        inputErrorMessage: 'Please enter a valid number'
      }
    )

    await sensorStore.calibrateSensor(id, {
      calibrationDate: new Date(),
      calibratedBy: 'Current User',
      calibrationValue: 0,
      notes: ''
    })
  } catch {
    // User cancelled
  }
}

const removeSensor = async (id: string) => {
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
    
    await sensorStore.deleteSensor(id)
  } catch {
    // User cancelled
  }
}

// Lifecycle
onMounted(async () => {
  await Promise.all([
    tankStore.fetchTanks(),
    sensorStore.fetchSensors()
  ])
})
</script>

<style lang="scss" scoped>
@import '@/styles/design-system/index.scss';

.modern-sensors-view {
  min-height: 100vh;
}

.page-header {
  background: linear-gradient(135deg, var(--color-primary-50), var(--color-secondary-50));
  
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

  .header-text {
    flex: 1;
  }
}

.filters-section {
  padding: var(--space-6) 0;
}

.filters-wrapper {
  display: flex;
  gap: var(--space-4);
  flex-wrap: wrap;
  align-items: center;
  margin-bottom: var(--space-6);

  .filter-item {
    flex: 1;
    min-width: 200px;

    &.filter-search {
      flex: 2;
    }
  }
}

.stats-overview {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: var(--space-4);
}

.stat-card {
  display: flex;
  align-items: center;
  gap: var(--space-4);
  padding: var(--space-5);
  background: white;
  border-radius: var(--border-radius-lg);
  box-shadow: var(--shadow-sm);
  transition: all var(--transition-base);

  &:hover {
    transform: translateY(-2px);
    box-shadow: var(--shadow-md);
  }

  .stat-icon {
    width: 56px;
    height: 56px;
    display: flex;
    align-items: center;
    justify-content: center;
    background: linear-gradient(135deg, var(--stat-color), var(--stat-color));
    border-radius: var(--border-radius-lg);
    color: white;
    opacity: 0.9;
  }

  .stat-content {
    flex: 1;
  }

  .stat-value {
    font-size: var(--font-size-2xl);
    font-weight: var(--font-weight-bold);
    font-family: var(--font-family-display);
    color: var(--color-text-primary);
    line-height: 1;
    margin-bottom: var(--space-1);
  }

  .stat-label {
    font-size: var(--font-size-sm);
    color: var(--color-text-tertiary);
  }
}

.loading-state,
.empty-state {
  text-align: center;
  padding: var(--space-20) var(--space-6);
  color: var(--color-text-tertiary);

  .el-icon {
    margin-bottom: var(--space-4);
    color: var(--color-text-tertiary);
  }

  h3 {
    font-size: var(--font-size-xl);
    color: var(--color-text-primary);
    margin: var(--space-4) 0;
  }

  p {
    margin-bottom: var(--space-6);
  }
}

.sensors-grid {
  animation-delay: 0ms;
}

.sensor-card {
  .sensor-header {
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
    margin-bottom: var(--space-4);
  }

  .sensor-icon-wrapper {
    width: 64px;
    height: 64px;
    display: flex;
    align-items: center;
    justify-content: center;
    border-radius: var(--border-radius-xl);
    color: white;

    &.sensor-type-temperature {
      background: linear-gradient(135deg, #ef4444, #f97316);
    }

    &.sensor-type-ph {
      background: linear-gradient(135deg, #8b5cf6, #a855f7);
    }

    &.sensor-type-oxygen {
      background: linear-gradient(135deg, #3b82f6, #2563eb);
    }

    &.sensor-type-salinity {
      background: linear-gradient(135deg, #06b6d4, #0891b2);
    }

    &.sensor-type-turbidity {
      background: linear-gradient(135deg, #64748b, #475569);
    }
  }

  .sensor-status-badge {
    display: flex;
    align-items: center;
    gap: var(--space-2);
    padding: var(--space-2) var(--space-3);
    border-radius: var(--border-radius-full);
    font-size: var(--font-size-xs);
    font-weight: var(--font-weight-semibold);
    text-transform: uppercase;
    letter-spacing: 0.05em;

    .status-dot {
      width: 8px;
      height: 8px;
      border-radius: 50%;
      background: currentColor;
    }

    &.status-active {
      background: var(--color-success-50);
      color: var(--color-success);
      
      .status-dot {
        animation: pulse 2s infinite;
      }
    }

    &.status-warning {
      background: var(--color-warning-50);
      color: var(--color-warning);
    }

    &.status-error {
      background: var(--color-danger-50);
      color: var(--color-danger);
    }

    &.status-offline {
      background: var(--color-neutral-100);
      color: var(--color-neutral-600);
    }
  }

  .sensor-info {
    margin-bottom: var(--space-4);

    .sensor-name {
      font-size: var(--font-size-xl);
      font-weight: var(--font-weight-bold);
      color: var(--color-text-primary);
      margin-bottom: var(--space-2);
    }

    .sensor-model {
      font-size: var(--font-size-sm);
      color: var(--color-text-tertiary);
      margin-bottom: var(--space-3);
    }

    .sensor-location {
      display: flex;
      align-items: center;
      gap: var(--space-2);
      font-size: var(--font-size-sm);
      color: var(--color-text-secondary);
    }
  }

  .sensor-reading {
    padding: var(--space-4);
    background: var(--color-neutral-50);
    border-radius: var(--border-radius-lg);
    margin-bottom: var(--space-4);

    &.empty {
      display: flex;
      align-items: center;
      justify-content: center;
      padding: var(--space-6);

      .no-data {
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: var(--space-2);
        color: var(--color-text-tertiary);
        font-size: var(--font-size-sm);
      }
    }

    .reading-value {
      font-size: var(--font-size-3xl);
      font-weight: var(--font-weight-bold);
      font-family: var(--font-family-display);
      color: var(--color-text-primary);
      line-height: 1;
      margin-bottom: var(--space-2);

      .reading-unit {
        font-size: var(--font-size-lg);
        color: var(--color-text-tertiary);
        margin-left: var(--space-2);
      }
    }

    .reading-time {
      display: flex;
      align-items: center;
      gap: var(--space-2);
      font-size: var(--font-size-xs);
      color: var(--color-text-tertiary);
    }
  }

  .sensor-actions {
    display: flex;
    gap: var(--space-2);
    padding-top: var(--space-2);
    border-top: 1px solid var(--color-neutral-200);
  }

  .action-btn {
    flex: 1;
    display: flex;
    align-items: center;
    justify-content: center;
    padding: var(--space-2);
    background: var(--color-neutral-50);
    border: none;
    border-radius: var(--border-radius-md);
    color: var(--color-text-secondary);
    cursor: pointer;
    transition: all var(--transition-fast);

    &:hover {
      background: var(--color-neutral-100);
      color: var(--color-primary-600);
      transform: translateY(-2px);
    }

    &--danger:hover {
      background: var(--color-danger-50);
      color: var(--color-danger);
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

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--space-4);

  @media (max-width: 768px) {
    grid-template-columns: 1fr;
  }
}

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: var(--space-3);
}
</style>
