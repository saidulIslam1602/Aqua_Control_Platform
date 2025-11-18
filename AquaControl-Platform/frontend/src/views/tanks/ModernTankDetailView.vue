<template>
  <div class="modern-tank-detail">
    <!-- Loading State -->
    <div v-if="tankStore.isLoading" class="loading-container">
      <el-icon class="is-loading" :size="64"><Loading /></el-icon>
      <p>Loading tank details...</p>
    </div>

    <!-- Error State -->
    <div v-else-if="tankStore.hasError" class="error-container">
      <el-icon :size="64"><WarningFilled /></el-icon>
      <h3>Error Loading Tank</h3>
      <p>{{ tankStore.state.error }}</p>
      <button class="btn btn-primary" @click="retryLoad">
        <el-icon><Refresh /></el-icon>
        <span>Retry</span>
      </button>
    </div>

    <!-- Tank Detail Content -->
    <div v-else-if="tank" class="tank-content">
      <!-- Page Header -->
      <section class="detail-header section--sm bg--secondary">
        <div class="container">
          <div class="header-nav">
            <button class="btn-back" @click="goBack">
              <el-icon><ArrowLeft /></el-icon>
              <span>Back to Tanks</span>
            </button>
            <div class="header-actions">
              <button class="btn btn-outline" @click="editTank">
                <el-icon><Edit /></el-icon>
                <span>Edit</span>
              </button>
              <button class="btn btn-outline" @click="scheduleMaintenance">
                <el-icon><Calendar /></el-icon>
                <span>Schedule Maintenance</span>
              </button>
              <button 
                v-if="tank.isActive"
                class="btn btn-warning"
                @click="deactivateTank"
              >
                <el-icon><VideoPause /></el-icon>
                <span>Deactivate</span>
              </button>
              <button 
                v-else
                class="btn btn-success"
                @click="activateTank"
              >
                <el-icon><VideoPlay /></el-icon>
                <span>Activate</span>
              </button>
            </div>
          </div>

          <div class="header-content">
            <div class="tank-title-section">
              <div class="status-indicator" :class="getStatusClass(tank)">
                <span class="status-dot"></span>
              </div>
              <div>
                <h1 class="modern-heading modern-heading--h1">{{ tank.name }}</h1>
                <div class="tank-meta">
                  <span class="meta-item">
                    <el-icon><Ticket /></el-icon>
                    {{ tank.tankType }}
                  </span>
                  <span class="meta-item">
                    <el-icon><Location /></el-icon>
                    {{ tank.location.building }}, {{ tank.location.room }}
                  </span>
                  <span class="meta-item">
                    <el-icon><Clock /></el-icon>
                    Created {{ formatRelativeDate(tank.createdAt) }}
                  </span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </section>

      <!-- Stats Overview -->
      <section class="section--sm">
        <div class="container">
          <div class="stats-grid grid grid--4">
            <ModernCard variant="glass" class="stat-card scroll-animate">
              <div class="stat-content">
                <div class="stat-icon" style="--stat-color: var(--color-primary-500)">
                  <el-icon :size="32"><DataLine /></el-icon>
                </div>
                <div class="stat-info">
                  <div class="stat-value">{{ tank.capacity.value }}</div>
                  <div class="stat-unit">{{ tank.capacity.unit }}</div>
                  <div class="stat-label">Capacity</div>
                </div>
              </div>
            </ModernCard>

            <ModernCard variant="glass" class="stat-card scroll-animate">
              <div class="stat-content">
                <div class="stat-icon" style="--stat-color: var(--color-success)">
                  <el-icon :size="32"><Monitor /></el-icon>
                </div>
                <div class="stat-info">
                  <div class="stat-value">{{ tank.activeSensorCount }}/{{ tank.sensorCount }}</div>
                  <div class="stat-label">Active Sensors</div>
                </div>
              </div>
            </ModernCard>

            <ModernCard variant="glass" class="stat-card scroll-animate">
              <div class="stat-content">
                <div class="stat-icon" style="--stat-color: var(--color-info)">
                  <el-icon :size="32"><TrendCharts /></el-icon>
                </div>
                <div class="stat-info">
                  <div class="stat-value">{{ waterQualityScore }}%</div>
                  <div class="stat-label">Water Quality</div>
                </div>
              </div>
            </ModernCard>

            <ModernCard variant="glass" class="stat-card scroll-animate">
              <div class="stat-content">
                <div class="stat-icon" style="--stat-color: var(--color-warning)">
                  <el-icon :size="32"><Tools /></el-icon>
                </div>
                <div class="stat-info">
                  <div class="stat-value">{{ maintenanceDays }}</div>
                  <div class="stat-label">Days Since Maintenance</div>
                </div>
              </div>
            </ModernCard>
          </div>
        </div>
      </section>

      <!-- Main Content Grid -->
      <section class="section">
        <div class="container">
          <div class="content-grid">
            <!-- Left Column -->
            <div class="content-main">
              <!-- Tank Information -->
              <ModernCard class="info-card scroll-animate">
                <template #header>
                  <div class="card-header-content">
                    <h3>Tank Information</h3>
                    <el-icon><InfoFilled /></el-icon>
                  </div>
                </template>

                <div class="info-grid">
                  <div class="info-item">
                    <span class="info-label">Tank ID</span>
                    <span class="info-value">{{ tank.id }}</span>
                  </div>
                  <div class="info-item">
                    <span class="info-label">Status</span>
                    <el-tag :type="getTagType(tank.status)" size="large">
                      {{ tank.status }}
                    </el-tag>
                  </div>
                  <div class="info-item">
                    <span class="info-label">Tank Type</span>
                    <span class="info-value">{{ tank.tankType }}</span>
                  </div>
                  <div class="info-item">
                    <span class="info-label">Capacity</span>
                    <span class="info-value">{{ tank.capacity.value }} {{ tank.capacity.unit }}</span>
                  </div>
                  <div class="info-item">
                    <span class="info-label">Building</span>
                    <span class="info-value">{{ tank.location.building }}</span>
                  </div>
                  <div class="info-item">
                    <span class="info-label">Room</span>
                    <span class="info-value">{{ tank.location.room }}</span>
                  </div>
                  <div class="info-item" v-if="tank.location.zone">
                    <span class="info-label">Zone</span>
                    <span class="info-value">{{ tank.location.zone }}</span>
                  </div>
                  <div class="info-item">
                    <span class="info-label">Created</span>
                    <span class="info-value">{{ formatDate(tank.createdAt) }}</span>
                  </div>
                  <div class="info-item" v-if="tank.updatedAt">
                    <span class="info-label">Last Updated</span>
                    <span class="info-value">{{ formatDate(tank.updatedAt) }}</span>
                  </div>
                </div>
              </ModernCard>

              <!-- Sensors Section -->
              <ModernCard class="sensors-card scroll-animate">
                <template #header>
                  <div class="card-header-content">
                    <h3>Sensors & Monitoring</h3>
                    <el-icon><Monitor /></el-icon>
                  </div>
                </template>

                <div v-if="tank.sensors && tank.sensors.length > 0" class="sensors-grid">
                  <div 
                    v-for="sensor in tank.sensors" 
                    :key="sensor.id"
                    class="sensor-item"
                    :class="getSensorStatusClass(sensor.status)"
                  >
                    <div class="sensor-icon">
                      <el-icon :size="24">
                        <component :is="getSensorIcon(sensor.sensorType)" />
                      </el-icon>
                    </div>
                    <div class="sensor-info">
                      <div class="sensor-name">{{ sensor.sensorType }}</div>
                      <div class="sensor-model">{{ sensor.model }}</div>
                    </div>
                    <div class="sensor-status">
                      <el-tag :type="getSensorTagType(sensor.status)" size="small">
                        {{ sensor.status }}
                      </el-tag>
                    </div>
                  </div>
                </div>

                <div v-else class="empty-sensors">
                  <el-icon :size="48"><Warning /></el-icon>
                  <p>No sensors installed</p>
                  <button class="btn btn-primary btn-small" @click="addSensor">
                    <el-icon><Plus /></el-icon>
                    <span>Add Sensor</span>
                  </button>
                </div>
              </ModernCard>

              <!-- Real-time Data Chart (Placeholder) -->
              <ModernCard class="chart-card scroll-animate">
                <template #header>
                  <div class="card-header-content">
                    <h3>Real-time Monitoring</h3>
                    <el-icon><TrendCharts /></el-icon>
                  </div>
                </template>

                <div class="chart-placeholder">
                  <el-icon :size="64"><DataAnalysis /></el-icon>
                  <p>Real-time sensor data visualization</p>
                  <small>Chart integration coming soon</small>
                </div>
              </ModernCard>
            </div>

            <!-- Right Column - Sidebar -->
            <div class="content-sidebar">
              <!-- Quick Actions -->
              <ModernCard class="actions-card scroll-animate">
                <template #header>
                  <h3>Quick Actions</h3>
                </template>

                <div class="action-buttons">
                  <button class="action-button" @click="viewSensorData">
                    <el-icon><DataAnalysis /></el-icon>
                    <span>View Sensor Data</span>
                  </button>
                  <button class="action-button" @click="viewAnalytics">
                    <el-icon><TrendCharts /></el-icon>
                    <span>View Analytics</span>
                  </button>
                  <button class="action-button" @click="downloadReport">
                    <el-icon><Download /></el-icon>
                    <span>Download Report</span>
                  </button>
                  <button class="action-button" @click="exportData">
                    <el-icon><Upload /></el-icon>
                    <span>Export Data</span>
                  </button>
                </div>
              </ModernCard>

              <!-- Maintenance History -->
              <ModernCard class="maintenance-card scroll-animate">
                <template #header>
                  <div class="card-header-content">
                    <h3>Maintenance</h3>
                    <el-icon><Tools /></el-icon>
                  </div>
                </template>

                <div class="maintenance-content">
                  <div class="maintenance-status" :class="{ 'due': tank.isMaintenanceDue }">
                    <el-icon :size="32">
                      <component :is="tank.isMaintenanceDue ? WarningFilled : CircleCheck" />
                    </el-icon>
                    <div class="maintenance-text">
                      <div class="maintenance-label">
                        {{ tank.isMaintenanceDue ? 'Maintenance Due' : 'Up to Date' }}
                      </div>
                      <div class="maintenance-days">
                        {{ maintenanceDays }} days since last service
                      </div>
                    </div>
                  </div>

                  <div class="maintenance-actions">
                    <button class="btn btn-primary btn-block" @click="scheduleMaintenance">
                      <el-icon><Calendar /></el-icon>
                      <span>Schedule Service</span>
                    </button>
                    <button class="btn btn-outline btn-block" @click="viewHistory">
                      <el-icon><DocumentCopy /></el-icon>
                      <span>View History</span>
                    </button>
                  </div>
                </div>
              </ModernCard>

              <!-- Activity Timeline -->
              <ModernCard class="activity-card scroll-animate">
                <template #header>
                  <h3>Recent Activity</h3>
                </template>

                <div class="activity-timeline">
                  <div class="timeline-item">
                    <div class="timeline-dot"></div>
                    <div class="timeline-content">
                      <div class="timeline-title">Tank Activated</div>
                      <div class="timeline-time">2 hours ago</div>
                    </div>
                  </div>
                  <div class="timeline-item">
                    <div class="timeline-dot"></div>
                    <div class="timeline-content">
                      <div class="timeline-title">Sensor Reading Updated</div>
                      <div class="timeline-time">5 hours ago</div>
                    </div>
                  </div>
                  <div class="timeline-item">
                    <div class="timeline-dot"></div>
                    <div class="timeline-content">
                      <div class="timeline-title">Maintenance Completed</div>
                      <div class="timeline-time">3 days ago</div>
                    </div>
                  </div>
                </div>
              </ModernCard>
            </div>
          </div>
        </div>
      </section>
    </div>

    <!-- Not Found State -->
    <div v-else class="not-found-container">
      <el-icon :size="64"><QuestionFilled /></el-icon>
      <h3>Tank Not Found</h3>
      <p>The tank you're looking for doesn't exist or has been removed.</p>
      <button class="btn btn-primary" @click="goBack">
        <el-icon><ArrowLeft /></el-icon>
        <span>Back to Tanks</span>
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useTankStore } from '@/stores/tankStore'
import ModernCard from '@/components/common/ModernCard.vue'
import {
  Loading,
  WarningFilled,
  Refresh,
  ArrowLeft,
  Edit,
  Calendar,
  VideoPause,
  VideoPlay,
  Ticket,
  Location,
  Clock,
  DataLine,
  Monitor,
  TrendCharts,
  Tools,
  InfoFilled,
  Plus,
  Warning,
  DataAnalysis,
  Download,
  Upload,
  CircleCheck,
  DocumentCopy,
  QuestionFilled,
  Thermometer,
  Odometer,
  Aim
} from '@element-plus/icons-vue'

// Router and Store
const route = useRoute()
const router = useRouter()
const tankStore = useTankStore()

// Computed Properties
const tankId = computed(() => route.params.id as string)
const tank = computed(() => tankStore.selectedTank)

const waterQualityScore = computed(() => {
  // Mock calculation - replace with real data
  return 92
})

const maintenanceDays = computed(() => {
  // Mock calculation - replace with real data
  return 15
})

// Methods
const getStatusClass = (tank: any) => {
  if (tank.isActive) return 'status-active'
  if (tank.isMaintenanceDue) return 'status-maintenance'
  return 'status-inactive'
}

const getTagType = (status: string) => {
  if (status === 'Active') return 'success'
  if (status === 'Maintenance') return 'warning'
  return 'info'
}

const getSensorStatusClass = (status: string) => {
  return `sensor-status-${status.toLowerCase()}`
}

const getSensorTagType = (status: string) => {
  if (status === 'Active') return 'success'
  if (status === 'Warning') return 'warning'
  if (status === 'Error') return 'danger'
  return 'info'
}

const getSensorIcon = (type: string) => {
  const iconMap: Record<string, any> = {
    Temperature: Thermometer,
    pH: Odometer,
    Oxygen: Aim,
    default: Monitor
  }
  return iconMap[type] || iconMap.default
}

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

const formatRelativeDate = (dateString: string) => {
  const date = new Date(dateString)
  const now = new Date()
  const diffTime = Math.abs(now.getTime() - date.getTime())
  const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24))
  
  if (diffDays === 0) return 'today'
  if (diffDays === 1) return 'yesterday'
  if (diffDays < 30) return `${diffDays} days ago`
  if (diffDays < 365) return `${Math.floor(diffDays / 30)} months ago`
  return `${Math.floor(diffDays / 365)} years ago`
}

const goBack = () => {
  router.push('/tanks')
}

const retryLoad = async () => {
  if (tankId.value) {
    await tankStore.fetchTankById(tankId.value)
  }
}

const editTank = () => {
  console.log('Edit tank')
  // TODO: Implement edit modal
}

const scheduleMaintenance = () => {
  console.log('Schedule maintenance')
  // TODO: Implement schedule maintenance modal
}

const activateTank = async () => {
  console.log('Activate tank')
  // TODO: Implement activate tank
}

const deactivateTank = async () => {
  console.log('Deactivate tank')
  // TODO: Implement deactivate tank
}

const addSensor = () => {
  console.log('Add sensor')
  // TODO: Implement add sensor modal
}

const viewSensorData = () => {
  router.push('/sensors')
}

const viewAnalytics = () => {
  router.push('/analytics')
}

const downloadReport = () => {
  console.log('Download report')
  // TODO: Implement download report
}

const exportData = () => {
  console.log('Export data')
  // TODO: Implement export data
}

const viewHistory = () => {
  console.log('View maintenance history')
  // TODO: Implement history view
}

// Lifecycle
onMounted(async () => {
  if (tankId.value) {
    await tankStore.fetchTankById(tankId.value)
  }
})
</script>

<style lang="scss" scoped>
@import '@/styles/design-system/index.scss';

.modern-tank-detail {
  min-height: 100vh;
  background: var(--color-neutral-50);
}

.loading-container,
.error-container,
.not-found-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: 60vh;
  text-align: center;
  padding: var(--space-8);

  .el-icon {
    margin-bottom: var(--space-6);
    color: var(--color-text-tertiary);
  }

  h3 {
    font-size: var(--font-size-2xl);
    color: var(--color-text-primary);
    margin: var(--space-4) 0;
  }

  p {
    color: var(--color-text-secondary);
    margin-bottom: var(--space-6);
  }
}

.detail-header {
  background: linear-gradient(135deg, var(--color-primary-50), var(--color-secondary-50));

  .header-nav {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-bottom: var(--space-6);
    flex-wrap: wrap;
    gap: var(--space-4);
  }

  .btn-back {
    display: flex;
    align-items: center;
    gap: var(--space-2);
    padding: var(--space-2) var(--space-4);
    background: white;
    border: 1px solid var(--color-neutral-200);
    border-radius: var(--border-radius-lg);
    color: var(--color-text-secondary);
    cursor: pointer;
    transition: all var(--transition-fast);
    font-size: var(--font-size-base);

    &:hover {
      background: var(--color-neutral-50);
      color: var(--color-primary-600);
      transform: translateX(-2px);
    }
  }

  .header-actions {
    display: flex;
    gap: var(--space-3);
    flex-wrap: wrap;
  }

  .tank-title-section {
    display: flex;
    align-items: flex-start;
    gap: var(--space-4);

    .status-indicator {
      width: 16px;
      height: 16px;
      border-radius: 50%;
      margin-top: var(--space-3);

      .status-dot {
        display: block;
        width: 100%;
        height: 100%;
        border-radius: 50%;
      }

      &.status-active {
        background: var(--color-success);
        animation: pulse 2s infinite;
      }

      &.status-maintenance {
        background: var(--color-warning);
      }

      &.status-inactive {
        background: var(--color-neutral-400);
      }
    }

    h1 {
      margin-bottom: var(--space-2);
    }

    .tank-meta {
      display: flex;
      gap: var(--space-6);
      flex-wrap: wrap;

      .meta-item {
        display: flex;
        align-items: center;
        gap: var(--space-2);
        color: var(--color-text-secondary);
        font-size: var(--font-size-sm);
      }
    }
  }
}

.stats-grid {
  .stat-card {
    .stat-content {
      display: flex;
      align-items: center;
      gap: var(--space-4);
    }

    .stat-icon {
      width: 64px;
      height: 64px;
      display: flex;
      align-items: center;
      justify-content: center;
      background: linear-gradient(135deg, var(--stat-color), var(--stat-color));
      border-radius: var(--border-radius-xl);
      color: white;
      opacity: 0.9;
    }

    .stat-info {
      flex: 1;
    }

    .stat-value {
      font-size: var(--font-size-3xl);
      font-weight: var(--font-weight-bold);
      font-family: var(--font-family-display);
      color: var(--color-text-primary);
      line-height: 1;
    }

    .stat-unit {
      font-size: var(--font-size-sm);
      color: var(--color-text-tertiary);
      margin-top: var(--space-1);
    }

    .stat-label {
      font-size: var(--font-size-sm);
      color: var(--color-text-tertiary);
      margin-top: var(--space-2);
    }
  }
}

.content-grid {
  display: grid;
  grid-template-columns: 1fr 400px;
  gap: var(--space-6);

  @media (max-width: 1200px) {
    grid-template-columns: 1fr;
  }
}

.content-main {
  display: flex;
  flex-direction: column;
  gap: var(--space-6);
}

.content-sidebar {
  display: flex;
  flex-direction: column;
  gap: var(--space-6);
}

.card-header-content {
  display: flex;
  align-items: center;
  justify-content: space-between;

  h3 {
    font-size: var(--font-size-lg);
    font-weight: var(--font-weight-semibold);
    color: var(--color-text-primary);
    margin: 0;
  }
}

.info-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: var(--space-5);

  .info-item {
    display: flex;
    flex-direction: column;
    gap: var(--space-2);

    .info-label {
      font-size: var(--font-size-sm);
      color: var(--color-text-tertiary);
      text-transform: uppercase;
      letter-spacing: 0.05em;
      font-weight: var(--font-weight-medium);
    }

    .info-value {
      font-size: var(--font-size-base);
      color: var(--color-text-primary);
      font-weight: var(--font-weight-medium);
    }
  }
}

.sensors-grid {
  display: flex;
  flex-direction: column;
  gap: var(--space-3);

  .sensor-item {
    display: flex;
    align-items: center;
    gap: var(--space-4);
    padding: var(--space-4);
    background: var(--color-neutral-50);
    border: 1px solid var(--color-neutral-200);
    border-radius: var(--border-radius-lg);
    transition: all var(--transition-fast);

    &:hover {
      background: white;
      border-color: var(--color-primary-200);
      transform: translateX(4px);
    }

    .sensor-icon {
      width: 48px;
      height: 48px;
      display: flex;
      align-items: center;
      justify-content: center;
      background: var(--color-primary-100);
      color: var(--color-primary-600);
      border-radius: var(--border-radius-lg);
    }

    .sensor-info {
      flex: 1;

      .sensor-name {
        font-weight: var(--font-weight-semibold);
        color: var(--color-text-primary);
        margin-bottom: var(--space-1);
      }

      .sensor-model {
        font-size: var(--font-size-sm);
        color: var(--color-text-tertiary);
      }
    }
  }
}

.empty-sensors {
  text-align: center;
  padding: var(--space-10);
  color: var(--color-text-tertiary);

  .el-icon {
    margin-bottom: var(--space-4);
  }

  p {
    margin-bottom: var(--space-6);
  }
}

.chart-placeholder {
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

.action-buttons {
  display: flex;
  flex-direction: column;
  gap: var(--space-2);

  .action-button {
    display: flex;
    align-items: center;
    gap: var(--space-3);
    padding: var(--space-4);
    background: var(--color-neutral-50);
    border: 1px solid var(--color-neutral-200);
    border-radius: var(--border-radius-lg);
    color: var(--color-text-secondary);
    cursor: pointer;
    transition: all var(--transition-fast);
    font-size: var(--font-size-base);
    text-align: left;

    &:hover {
      background: white;
      border-color: var(--color-primary-300);
      color: var(--color-primary-600);
      transform: translateX(4px);
    }
  }
}

.maintenance-content {
  .maintenance-status {
    display: flex;
    align-items: center;
    gap: var(--space-4);
    padding: var(--space-5);
    background: var(--color-success-50);
    border-radius: var(--border-radius-lg);
    margin-bottom: var(--space-5);
    color: var(--color-success);

    &.due {
      background: var(--color-warning-50);
      color: var(--color-warning);
    }

    .maintenance-text {
      flex: 1;
    }

    .maintenance-label {
      font-weight: var(--font-weight-semibold);
      font-size: var(--font-size-base);
      margin-bottom: var(--space-1);
    }

    .maintenance-days {
      font-size: var(--font-size-sm);
      opacity: 0.8;
    }
  }

  .maintenance-actions {
    display: flex;
    flex-direction: column;
    gap: var(--space-3);
  }
}

.activity-timeline {
  display: flex;
  flex-direction: column;
  gap: var(--space-4);

  .timeline-item {
    display: flex;
    gap: var(--space-4);
    position: relative;

    &:not(:last-child)::after {
      content: '';
      position: absolute;
      left: 5px;
      top: 20px;
      width: 2px;
      height: calc(100% + var(--space-4));
      background: var(--color-neutral-200);
    }

    .timeline-dot {
      width: 12px;
      height: 12px;
      border-radius: 50%;
      background: var(--color-primary-500);
      margin-top: 4px;
      flex-shrink: 0;
      z-index: 1;
    }

    .timeline-content {
      flex: 1;

      .timeline-title {
        font-weight: var(--font-weight-medium);
        color: var(--color-text-primary);
        margin-bottom: var(--space-1);
      }

      .timeline-time {
        font-size: var(--font-size-sm);
        color: var(--color-text-tertiary);
      }
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
