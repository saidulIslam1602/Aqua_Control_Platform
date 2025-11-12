<template>
  <div class="sensors-view">
    <div class="page-header">
      <div class="header-content">
        <h1>Sensor Management</h1>
        <p>Monitor and manage all aquaculture sensors</p>
      </div>
      <div class="header-actions">
        <el-button type="primary" @click="refreshSensors" :loading="isLoading">
          <el-icon><Refresh /></el-icon>
          Refresh
        </el-button>
        <el-button type="success" @click="showAddSensorDialog = true">
          <el-icon><Plus /></el-icon>
          Add Sensor
        </el-button>
      </div>
    </div>

    <!-- Sensor Statistics -->
    <div class="stats-grid">
      <el-card class="stat-card">
        <div class="stat-content">
          <div class="stat-icon online">
            <el-icon><Monitor /></el-icon>
          </div>
          <div class="stat-info">
            <div class="stat-value">{{ onlineSensors.length }}</div>
            <div class="stat-label">Online Sensors</div>
          </div>
        </div>
      </el-card>

      <el-card class="stat-card">
        <div class="stat-content">
          <div class="stat-icon offline">
            <el-icon><Warning /></el-icon>
          </div>
          <div class="stat-info">
            <div class="stat-value">{{ offlineSensors.length }}</div>
            <div class="stat-label">Offline Sensors</div>
          </div>
        </div>
      </el-card>

      <el-card class="stat-card">
        <div class="stat-content">
          <div class="stat-icon calibrating">
            <el-icon><Setting /></el-icon>
          </div>
          <div class="stat-info">
            <div class="stat-value">{{ calibratingSensors.length }}</div>
            <div class="stat-label">Calibrating</div>
          </div>
        </div>
      </el-card>

      <el-card class="stat-card">
        <div class="stat-content">
          <div class="stat-icon total">
            <el-icon><DataBoard /></el-icon>
          </div>
          <div class="stat-info">
            <div class="stat-value">{{ sensors.length }}</div>
            <div class="stat-label">Total Sensors</div>
          </div>
        </div>
      </el-card>
    </div>

    <!-- Sensors Table -->
    <el-card class="sensors-table-card">
      <template #header>
        <div class="card-header">
          <span>Sensors</span>
          <div class="header-filters">
            <el-select v-model="selectedTankFilter" placeholder="Filter by Tank" clearable style="width: 200px">
              <el-option label="All Tanks" value="" />
              <el-option 
                v-for="tank in tankStore.tanks" 
                :key="tank.id" 
                :label="tank.name" 
                :value="tank.id" 
              />
            </el-select>
            <el-select v-model="selectedStatusFilter" placeholder="Filter by Status" clearable style="width: 150px">
              <el-option label="All Status" value="" />
              <el-option label="Online" value="Online" />
              <el-option label="Offline" value="Offline" />
              <el-option label="Calibrating" value="Calibrating" />
            </el-select>
          </div>
        </div>
      </template>

      <el-table :data="filteredSensors" v-loading="isLoading" style="width: 100%">
        <el-table-column prop="name" label="Sensor Name" min-width="150" />
        <el-table-column prop="type" label="Type" width="120" />
        <el-table-column prop="tankName" label="Tank" width="150" />
        <el-table-column label="Status" width="100">
          <template #default="{ row }">
            <el-tag :type="getStatusTagType(row.status)">
              {{ row.status }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="lastReading" label="Last Reading" width="120" />
        <el-table-column prop="unit" label="Unit" width="80" />
        <el-table-column label="Last Updated" width="150">
          <template #default="{ row }">
            {{ formatTimestamp(row.lastUpdated) }}
          </template>
        </el-table-column>
        <el-table-column label="Actions" width="150" fixed="right">
          <template #default="{ row }">
            <el-button size="small" @click="calibrateSensor(row.id)">
              Calibrate
            </el-button>
            <el-button size="small" type="danger" @click="deleteSensor(row.id)">
              Delete
            </el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-card>

    <!-- Add Sensor Dialog -->
    <el-dialog v-model="showAddSensorDialog" title="Add New Sensor" width="500px">
      <el-form :model="newSensorForm" label-width="120px">
        <el-form-item label="Sensor Name">
          <el-input v-model="newSensorForm.name" placeholder="Enter sensor name" />
        </el-form-item>
        <el-form-item label="Sensor Type">
          <el-select v-model="newSensorForm.type" placeholder="Select sensor type">
            <el-option label="Temperature" value="Temperature" />
            <el-option label="pH" value="pH" />
            <el-option label="Dissolved Oxygen" value="DissolvedOxygen" />
            <el-option label="Salinity" value="Salinity" />
          </el-select>
        </el-form-item>
        <el-form-item label="Tank">
          <el-select v-model="newSensorForm.tankId" placeholder="Select tank">
            <el-option 
              v-for="tank in tankStore.tanks" 
              :key="tank.id" 
              :label="tank.name" 
              :value="tank.id" 
            />
          </el-select>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="showAddSensorDialog = false">Cancel</el-button>
        <el-button type="primary" @click="addSensor">Add Sensor</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useTankStore } from '@/stores/tankStore'
import { useRealTimeStore } from '@/stores/realTimeStore'
import { useAlertStore } from '@/stores/alertStore'
import { 
  Refresh, 
  DataBoard, 
  Monitor, 
  Warning,
  Setting,
  Plus
} from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'

// Stores
const router = useRouter()
const tankStore = useTankStore()
const realTimeStore = useRealTimeStore()
const alertStore = useAlertStore()

// State
const isLoading = ref(false)
const showAddSensorDialog = ref(false)
const selectedTankFilter = ref('')
const selectedStatusFilter = ref('')

// Sample sensor data
const sensors = ref([
  {
    id: 'sensor-1',
    name: 'Temperature Sensor 01',
    type: 'Temperature',
    tankId: 'tank-1',
    tankName: 'Main Tank 01',
    status: 'Online',
    lastReading: '24.5',
    unit: '°C',
    lastUpdated: new Date(Date.now() - 1000 * 60 * 5) // 5 minutes ago
  },
  {
    id: 'sensor-2',
    name: 'pH Sensor 01',
    type: 'pH',
    tankId: 'tank-1',
    tankName: 'Main Tank 01',
    status: 'Online',
    lastReading: '7.2',
    unit: 'pH',
    lastUpdated: new Date(Date.now() - 1000 * 60 * 3) // 3 minutes ago
  },
  {
    id: 'sensor-3',
    name: 'DO Sensor 02',
    type: 'DissolvedOxygen',
    tankId: 'tank-2',
    tankName: 'Breeding Tank 01',
    status: 'Calibrating',
    lastReading: '6.8',
    unit: 'mg/L',
    lastUpdated: new Date(Date.now() - 1000 * 60 * 15) // 15 minutes ago
  },
  {
    id: 'sensor-4',
    name: 'Temperature Sensor 02',
    type: 'Temperature',
    tankId: 'tank-3',
    tankName: 'Nursery Tank 01',
    status: 'Offline',
    lastReading: 'N/A',
    unit: '°C',
    lastUpdated: new Date(Date.now() - 1000 * 60 * 60 * 2) // 2 hours ago
  }
])

const newSensorForm = ref({
  name: '',
  type: '',
  tankId: ''
})

// Computed
const onlineSensors = computed(() => sensors.value.filter(s => s.status === 'Online'))
const offlineSensors = computed(() => sensors.value.filter(s => s.status === 'Offline'))
const calibratingSensors = computed(() => sensors.value.filter(s => s.status === 'Calibrating'))

const filteredSensors = computed(() => {
  let filtered = sensors.value

  if (selectedTankFilter.value) {
    filtered = filtered.filter(s => s.tankId === selectedTankFilter.value)
  }

  if (selectedStatusFilter.value) {
    filtered = filtered.filter(s => s.status === selectedStatusFilter.value)
  }

  return filtered
})

// Methods
const refreshSensors = async () => {
  isLoading.value = true
  try {
    // TODO: Replace with actual API call
    await new Promise(resolve => setTimeout(resolve, 1000))
    ElMessage.success('Sensors refreshed')
  } catch (error) {
    ElMessage.error('Failed to refresh sensors')
  } finally {
    isLoading.value = false
  }
}

const addSensor = async () => {
  if (!newSensorForm.value.name || !newSensorForm.value.type || !newSensorForm.value.tankId) {
    ElMessage.warning('Please fill in all required fields')
    return
  }

  try {
    // TODO: Replace with actual API call
    const newSensor = {
      id: `sensor-${Date.now()}`,
      name: newSensorForm.value.name,
      type: newSensorForm.value.type,
      tankId: newSensorForm.value.tankId,
      tankName: tankStore.tanks.find(t => t.id === newSensorForm.value.tankId)?.name || 'Unknown',
      status: 'Online',
      lastReading: 'N/A',
      unit: getSensorUnit(newSensorForm.value.type),
      lastUpdated: new Date()
    }

    sensors.value.push(newSensor)
    showAddSensorDialog.value = false
    newSensorForm.value = { name: '', type: '', tankId: '' }
    ElMessage.success('Sensor added successfully')
  } catch (error) {
    ElMessage.error('Failed to add sensor')
  }
}

const calibrateSensor = async (sensorId: string) => {
  const sensor = sensors.value.find(s => s.id === sensorId)
  if (sensor) {
    sensor.status = 'Calibrating'
    ElMessage.info(`Calibrating ${sensor.name}...`)
    
    // Simulate calibration
    setTimeout(() => {
      sensor.status = 'Online'
      sensor.lastUpdated = new Date()
      ElMessage.success(`${sensor.name} calibration completed`)
    }, 3000)
  }
}

const deleteSensor = async (sensorId: string) => {
  const index = sensors.value.findIndex(s => s.id === sensorId)
  if (index !== -1) {
    sensors.value.splice(index, 1)
    ElMessage.success('Sensor deleted successfully')
  }
}

const getStatusTagType = (status: string) => {
  switch (status) {
    case 'Online': return 'success'
    case 'Offline': return 'danger'
    case 'Calibrating': return 'warning'
    default: return 'info'
  }
}

const getSensorUnit = (type: string) => {
  switch (type) {
    case 'Temperature': return '°C'
    case 'pH': return 'pH'
    case 'DissolvedOxygen': return 'mg/L'
    case 'Salinity': return 'ppt'
    default: return ''
  }
}

const formatTimestamp = (timestamp: Date) => {
  const now = new Date()
  const diff = now.getTime() - timestamp.getTime()
  const minutes = Math.floor(diff / (1000 * 60))
  
  if (minutes < 1) return 'Just now'
  if (minutes < 60) return `${minutes}m ago`
  if (minutes < 1440) return `${Math.floor(minutes / 60)}h ago`
  return timestamp.toLocaleDateString()
}

// Lifecycle
onMounted(async () => {
  await tankStore.fetchTanks()
  await refreshSensors()
})
</script>

<style lang="scss" scoped>
.sensors-view {
  padding: 24px;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 24px;

  .header-content {
    h1 {
      margin: 0 0 8px 0;
      font-size: 28px;
      font-weight: 600;
      color: var(--el-text-color-primary);
    }

    p {
      margin: 0;
      color: var(--el-text-color-regular);
      font-size: 14px;
    }
  }

  .header-actions {
    display: flex;
    gap: 12px;
  }
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 16px;
  margin-bottom: 24px;
}

.stat-card {
  .stat-content {
    display: flex;
    align-items: center;
    gap: 16px;
  }

  .stat-icon {
    width: 48px;
    height: 48px;
    border-radius: 8px;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 24px;

    &.online {
      background-color: #f0f9ff;
      color: #0ea5e9;
    }

    &.offline {
      background-color: #fef2f2;
      color: #ef4444;
    }

    &.calibrating {
      background-color: #fffbeb;
      color: #f59e0b;
    }

    &.total {
      background-color: #f8fafc;
      color: #64748b;
    }
  }

  .stat-info {
    .stat-value {
      font-size: 24px;
      font-weight: 600;
      color: var(--el-text-color-primary);
      line-height: 1;
    }

    .stat-label {
      font-size: 12px;
      color: var(--el-text-color-regular);
      margin-top: 4px;
    }
  }
}

.sensors-table-card {
  .card-header {
    display: flex;
    justify-content: space-between;
    align-items: center;

    .header-filters {
      display: flex;
      gap: 12px;
    }
  }
}

@media (max-width: 768px) {
  .page-header {
    flex-direction: column;
    gap: 16px;

    .header-actions {
      width: 100%;
      justify-content: stretch;

      .el-button {
        flex: 1;
      }
    }
  }

  .stats-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}
</style>

