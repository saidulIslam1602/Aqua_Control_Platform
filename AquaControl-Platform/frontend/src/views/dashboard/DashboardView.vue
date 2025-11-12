<template>
  <div class="dashboard-view">
    <!-- Header -->
    <div class="dashboard-header">
      <h1>AquaControl Dashboard</h1>
      <div class="header-stats">
        <el-statistic
          title="Total Tanks"
          :value="tankStore.tanks.length"
          class="stat-card"
        />
        <el-statistic
          title="Active Tanks"
          :value="tankStore.activeTanks.length"
          class="stat-card"
        />
        <el-statistic
          title="Alerts"
          :value="alertCount"
          class="stat-card alert-stat"
        />
        <el-statistic
          title="System Health"
          :value="systemHealth"
          suffix="%"
          class="stat-card health-stat"
        />
      </div>
    </div>

    <!-- Main Content -->
    <div class="dashboard-content">
      <!-- Tank Overview -->
      <div class="dashboard-section">
        <div class="section-header">
          <h2>Tank Overview</h2>
          <el-button type="primary" @click="refreshData">
            <el-icon><Refresh /></el-icon>
            Refresh
          </el-button>
        </div>
        
        <div class="tank-grid" v-if="!tankStore.isLoading">
          <div 
            v-for="tank in tankStore.tanks.slice(0, 6)" 
            :key="tank.id"
            class="tank-card"
            @click="navigateToTank(tank.id)"
          >
            <div class="tank-header">
              <h3>{{ tank.name }}</h3>
              <el-tag 
                :type="tank.isActive ? 'success' : 'info'"
                size="small"
              >
                {{ tank.status }}
              </el-tag>
            </div>
            
            <div class="tank-info">
              <div class="info-item">
                <span class="label">Type:</span>
                <span class="value">{{ tank.tankType }}</span>
              </div>
              <div class="info-item">
                <span class="label">Capacity:</span>
                <span class="value">{{ tank.capacity.value }} {{ tank.capacity.unit }}</span>
              </div>
              <div class="info-item">
                <span class="label">Location:</span>
                <span class="value">{{ tank.location.building }}, {{ tank.location.room }}</span>
              </div>
              <div class="info-item">
                <span class="label">Sensors:</span>
                <span class="value">{{ tank.activeSensorCount }}/{{ tank.sensorCount }}</span>
              </div>
            </div>

            <div class="tank-status">
              <div class="status-indicator" :class="getStatusClass(tank)"></div>
              <span class="status-text">{{ getStatusText(tank) }}</span>
            </div>
          </div>
        </div>

        <div v-else class="loading-state">
          <el-skeleton :rows="3" animated />
        </div>

        <div class="section-footer" v-if="tankStore.tanks.length > 6">
          <el-button type="text" @click="navigateToTanks">
            View All Tanks ({{ tankStore.tanks.length }})
          </el-button>
        </div>
      </div>

      <!-- Recent Activity -->
      <div class="dashboard-section">
        <div class="section-header">
          <h2>Recent Activity</h2>
        </div>
        
        <div class="activity-list">
          <div v-for="activity in recentActivities" :key="activity.id" class="activity-item">
            <div class="activity-icon">
              <el-icon :class="getActivityIconClass(activity.type)">
                <component :is="getActivityIcon(activity.type)" />
              </el-icon>
            </div>
            <div class="activity-content">
              <div class="activity-title">{{ activity.title }}</div>
              <div class="activity-description">{{ activity.description }}</div>
              <div class="activity-time">{{ formatTime(activity.timestamp) }}</div>
            </div>
          </div>
        </div>
      </div>

      <!-- System Status -->
      <div class="dashboard-section">
        <div class="section-header">
          <h2>System Status</h2>
        </div>
        
        <div class="status-grid">
          <div class="status-item">
            <div class="status-header">
              <el-icon class="status-icon database"><Database /></el-icon>
              <span>Database</span>
            </div>
            <div class="status-value">
              <el-tag type="success" size="small">Online</el-tag>
            </div>
          </div>
          
          <div class="status-item">
            <div class="status-header">
              <el-icon class="status-icon realtime"><Connection /></el-icon>
              <span>Real-time</span>
            </div>
            <div class="status-value">
              <el-tag :type="realTimeStore.isConnected ? 'success' : 'danger'" size="small">
                {{ realTimeStore.isConnected ? 'Connected' : 'Disconnected' }}
              </el-tag>
            </div>
          </div>
          
          <div class="status-item">
            <div class="status-header">
              <el-icon class="status-icon monitoring"><Monitor /></el-icon>
              <span>Monitoring</span>
            </div>
            <div class="status-value">
              <el-tag type="success" size="small">Active</el-tag>
            </div>
          </div>
          
          <div class="status-item">
            <div class="status-header">
              <el-icon class="status-icon alerts"><Bell /></el-icon>
              <span>Alerts</span>
            </div>
            <div class="status-value">
              <el-tag :type="alertCount > 0 ? 'warning' : 'success'" size="small">
                {{ alertCount }} Active
              </el-tag>
            </div>
          </div>
        </div>
      </div>
    </div>
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
  DataBoard as Database, 
  Link as Connection, 
  Monitor, 
  Bell,
  Warning,
  InfoFilled,
  SuccessFilled
} from '@element-plus/icons-vue'
import type { Tank } from '@/types/domain'

const router = useRouter()
const tankStore = useTankStore()
const realTimeStore = useRealTimeStore()
const alertStore = useAlertStore()

// Computed properties
const alertCount = computed(() => {
  return alertStore.alertCount
})

const systemHealth = computed(() => {
  const activeTanks = tankStore.activeTanks.length
  const totalTanks = tankStore.tanks.length
  if (totalTanks === 0) return 100
  return Math.round((activeTanks / totalTanks) * 100)
})

// Sample recent activities
const recentActivities = ref([
  {
    id: 1,
    type: 'tank_created',
    title: 'New Tank Added',
    description: 'Tank "Salmon Breeding 01" was created',
    timestamp: new Date(Date.now() - 1000 * 60 * 15) // 15 minutes ago
  },
  {
    id: 2,
    type: 'tank_activated',
    title: 'Tank Activated',
    description: 'Tank "Trout Growing 03" was activated',
    timestamp: new Date(Date.now() - 1000 * 60 * 45) // 45 minutes ago
  },
  {
    id: 3,
    type: 'sensor_alert',
    title: 'Sensor Alert',
    description: 'Temperature sensor in "Main Tank 01" reported high reading',
    timestamp: new Date(Date.now() - 1000 * 60 * 120) // 2 hours ago
  },
  {
    id: 4,
    type: 'maintenance',
    title: 'Maintenance Scheduled',
    description: 'Maintenance scheduled for "Breeding Tank 02"',
    timestamp: new Date(Date.now() - 1000 * 60 * 180) // 3 hours ago
  }
])

// Methods
const refreshData = async () => {
  await tankStore.fetchTanks()
}

const navigateToTank = (tankId: string) => {
  router.push(`/tanks/${tankId}`)
}

const navigateToTanks = () => {
  router.push('/tanks')
}

const getStatusClass = (tank: Tank) => {
  if (tank.isActive) return 'status-active'
  if (tank.isMaintenanceDue) return 'status-maintenance'
  return 'status-inactive'
}

const getStatusText = (tank: Tank) => {
  if (tank.isActive) return 'Operational'
  if (tank.isMaintenanceDue) return 'Maintenance Due'
  return 'Inactive'
}

const getActivityIcon = (type: string) => {
  switch (type) {
    case 'tank_created':
    case 'tank_activated':
      return SuccessFilled
    case 'sensor_alert':
      return Warning
    case 'maintenance':
      return InfoFilled
    default:
      return InfoFilled
  }
}

const getActivityIconClass = (type: string) => {
  switch (type) {
    case 'tank_created':
    case 'tank_activated':
      return 'activity-success'
    case 'sensor_alert':
      return 'activity-warning'
    case 'maintenance':
      return 'activity-info'
    default:
      return 'activity-info'
  }
}

const formatTime = (timestamp: Date) => {
  const now = new Date()
  const diff = now.getTime() - timestamp.getTime()
  const minutes = Math.floor(diff / (1000 * 60))
  const hours = Math.floor(diff / (1000 * 60 * 60))
  
  if (minutes < 60) {
    return `${minutes} minutes ago`
  } else if (hours < 24) {
    return `${hours} hours ago`
  } else {
    return timestamp.toLocaleDateString()
  }
}

// Lifecycle
onMounted(async () => {
  await refreshData()
  await alertStore.fetchAlerts()
})
</script>

<style lang="scss" scoped>
.dashboard-view {
  padding: 24px;
  max-width: 1400px;
  margin: 0 auto;
}

.dashboard-header {
  margin-bottom: 32px;
  
  h1 {
    margin: 0 0 24px 0;
    color: var(--el-text-color-primary);
    font-size: 32px;
    font-weight: 600;
  }
  
  .header-stats {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
    gap: 24px;
    
    .stat-card {
      background: var(--el-bg-color);
      border: 1px solid var(--el-border-color);
      border-radius: 8px;
      padding: 20px;
      
      &.alert-stat {
        border-color: var(--el-color-warning);
        background: var(--el-color-warning-light-9);
      }
      
      &.health-stat {
        border-color: var(--el-color-success);
        background: var(--el-color-success-light-9);
      }
    }
  }
}

.dashboard-content {
  display: flex;
  flex-direction: column;
  gap: 32px;
}

.dashboard-section {
  background: var(--el-bg-color);
  border: 1px solid var(--el-border-color);
  border-radius: 12px;
  padding: 24px;
  
  .section-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 20px;
    
    h2 {
      margin: 0;
      color: var(--el-text-color-primary);
      font-size: 20px;
      font-weight: 600;
    }
  }
  
  .section-footer {
    margin-top: 16px;
    text-align: center;
  }
}

.tank-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 20px;
}

.tank-card {
  background: var(--el-bg-color-page);
  border: 1px solid var(--el-border-color-light);
  border-radius: 8px;
  padding: 20px;
  cursor: pointer;
  transition: all 0.3s ease;
  
  &:hover {
    border-color: var(--el-color-primary);
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
    transform: translateY(-2px);
  }
  
  .tank-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 16px;
    
    h3 {
      margin: 0;
      color: var(--el-text-color-primary);
      font-size: 16px;
      font-weight: 600;
    }
  }
  
  .tank-info {
    margin-bottom: 16px;
    
    .info-item {
      display: flex;
      justify-content: space-between;
      margin-bottom: 8px;
      
      .label {
        color: var(--el-text-color-regular);
        font-size: 14px;
      }
      
      .value {
        color: var(--el-text-color-primary);
        font-size: 14px;
        font-weight: 500;
      }
    }
  }
  
  .tank-status {
    display: flex;
    align-items: center;
    gap: 8px;
    
    .status-indicator {
      width: 8px;
      height: 8px;
      border-radius: 50%;
      
      &.status-active {
        background-color: var(--el-color-success);
      }
      
      &.status-maintenance {
        background-color: var(--el-color-warning);
      }
      
      &.status-inactive {
        background-color: var(--el-color-info);
      }
    }
    
    .status-text {
      color: var(--el-text-color-regular);
      font-size: 14px;
    }
  }
}

.loading-state {
  padding: 20px;
}

.activity-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.activity-item {
  display: flex;
  align-items: flex-start;
  gap: 12px;
  padding: 16px;
  background: var(--el-bg-color-page);
  border-radius: 8px;
  border: 1px solid var(--el-border-color-lighter);
  
  .activity-icon {
    flex-shrink: 0;
    width: 32px;
    height: 32px;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    
    &.activity-success {
      background: var(--el-color-success-light-8);
      color: var(--el-color-success);
    }
    
    &.activity-warning {
      background: var(--el-color-warning-light-8);
      color: var(--el-color-warning);
    }
    
    &.activity-info {
      background: var(--el-color-info-light-8);
      color: var(--el-color-info);
    }
  }
  
  .activity-content {
    flex: 1;
    
    .activity-title {
      font-weight: 600;
      color: var(--el-text-color-primary);
      margin-bottom: 4px;
    }
    
    .activity-description {
      color: var(--el-text-color-regular);
      font-size: 14px;
      margin-bottom: 4px;
    }
    
    .activity-time {
      color: var(--el-text-color-placeholder);
      font-size: 12px;
    }
  }
}

.status-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 16px;
}

.status-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px;
  background: var(--el-bg-color-page);
  border-radius: 8px;
  border: 1px solid var(--el-border-color-lighter);
  
  .status-header {
    display: flex;
    align-items: center;
    gap: 8px;
    
    .status-icon {
      font-size: 18px;
      
      &.database {
        color: var(--el-color-primary);
      }
      
      &.realtime {
        color: var(--el-color-success);
      }
      
      &.monitoring {
        color: var(--el-color-warning);
      }
      
      &.alerts {
        color: var(--el-color-danger);
      }
    }
    
    span {
      font-weight: 500;
      color: var(--el-text-color-primary);
    }
  }
}

@media (max-width: 768px) {
  .dashboard-view {
    padding: 16px;
  }
  
  .dashboard-header {
    .header-stats {
      grid-template-columns: repeat(2, 1fr);
    }
  }
  
  .tank-grid {
    grid-template-columns: 1fr;
  }
  
  .status-grid {
    grid-template-columns: 1fr;
  }
}
</style>

