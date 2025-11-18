<template>
  <div class="modern-analytics-view">
    <!-- Page Header -->
    <section class="page-header section--sm">
      <div class="container">
        <div class="header-content">
          <div class="header-text">
            <h1 class="modern-heading modern-heading--h1">Analytics Dashboard</h1>
            <p class="modern-text modern-text--lead">
              Comprehensive insights and performance metrics for your aquaculture operations
            </p>
          </div>
          <div class="header-actions">
            <el-select v-model="timeRange" size="large" style="width: 180px">
              <el-option label="Last 24 Hours" value="24h" />
              <el-option label="Last 7 Days" value="7d" />
              <el-option label="Last 30 Days" value="30d" />
              <el-option label="Last 90 Days" value="90d" />
              <el-option label="Custom Range" value="custom" />
            </el-select>
            <button class="btn btn-outline" @click="exportData">
              <el-icon><Download /></el-icon>
              <span>Export</span>
            </button>
            <button class="btn btn-primary" @click="refreshData">
              <el-icon><Refresh /></el-icon>
              <span>Refresh</span>
            </button>
          </div>
        </div>
      </div>
    </section>

    <!-- Key Metrics -->
    <section class="section--sm bg--secondary">
      <div class="container">
        <div class="metrics-grid grid grid--4">
          <ModernCard variant="glass" class="metric-card scroll-animate">
            <div class="metric-content">
              <div class="metric-header">
                <div class="metric-icon" style="--metric-color: var(--color-primary-500)">
                  <el-icon :size="28"><TrendCharts /></el-icon>
                </div>
                <el-tag type="success" size="small">+12.5%</el-tag>
              </div>
              <div class="metric-value">{{ systemEfficiency }}%</div>
              <div class="metric-label">System Efficiency</div>
              <div class="metric-trend">
                <div class="mini-chart">
                  <svg viewBox="0 0 100 30" preserveAspectRatio="none">
                    <polyline 
                      points="0,20 20,15 40,18 60,10 80,12 100,8" 
                      fill="none" 
                      stroke="var(--color-success)" 
                      stroke-width="2"
                    />
                  </svg>
                </div>
              </div>
            </div>
          </ModernCard>

          <ModernCard variant="glass" class="metric-card scroll-animate">
            <div class="metric-content">
              <div class="metric-header">
                <div class="metric-icon" style="--metric-color: var(--color-info)">
                  <el-icon :size="28"><DataAnalysis /></el-icon>
                </div>
                <el-tag type="info" size="small">+8.3%</el-tag>
              </div>
              <div class="metric-value">{{ avgWaterQuality }}%</div>
              <div class="metric-label">Avg Water Quality</div>
              <div class="metric-trend">
                <div class="mini-chart">
                  <svg viewBox="0 0 100 30" preserveAspectRatio="none">
                    <polyline 
                      points="0,15 20,12 40,14 60,10 80,9 100,7" 
                      fill="none" 
                      stroke="var(--color-info)" 
                      stroke-width="2"
                    />
                  </svg>
                </div>
              </div>
            </div>
          </ModernCard>

          <ModernCard variant="glass" class="metric-card scroll-animate">
            <div class="metric-content">
              <div class="metric-header">
                <div class="metric-icon" style="--metric-color: var(--color-success)">
                  <el-icon :size="28"><Histogram /></el-icon>
                </div>
                <el-tag type="warning" size="small">-3.2%</el-tag>
              </div>
              <div class="metric-value">{{ totalEvents }}</div>
              <div class="metric-label">Total Events</div>
              <div class="metric-trend">
                <div class="mini-chart">
                  <svg viewBox="0 0 100 30" preserveAspectRatio="none">
                    <polyline 
                      points="0,10 20,12 40,15 60,14 80,18 100,20" 
                      fill="none" 
                      stroke="var(--color-warning)" 
                      stroke-width="2"
                    />
                  </svg>
                </div>
              </div>
            </div>
          </ModernCard>

          <ModernCard variant="glass" class="metric-card scroll-animate">
            <div class="metric-content">
              <div class="metric-header">
                <div class="metric-icon" style="--metric-color: var(--color-warning)">
                  <el-icon :size="28"><Odometer /></el-icon>
                </div>
                <el-tag type="success" size="small">+5.1%</el-tag>
              </div>
              <div class="metric-value">{{ sensorUptime }}%</div>
              <div class="metric-label">Sensor Uptime</div>
              <div class="metric-trend">
                <div class="mini-chart">
                  <svg viewBox="0 0 100 30" preserveAspectRatio="none">
                    <polyline 
                      points="0,18 20,16 40,17 60,14 80,13 100,10" 
                      fill="none" 
                      stroke="var(--color-success)" 
                      stroke-width="2"
                    />
                  </svg>
                </div>
              </div>
            </div>
          </ModernCard>
        </div>
      </div>
    </section>

    <!-- Charts Section -->
    <section class="section">
      <div class="container">
        <div class="charts-layout">
          <!-- Main Charts -->
          <div class="charts-main">
            <!-- Temperature Trends -->
            <ModernCard class="chart-card scroll-animate">
              <template #header>
                <div class="chart-header">
                  <div>
                    <h3>Temperature Trends</h3>
                    <p class="chart-subtitle">Real-time temperature monitoring across all tanks</p>
                  </div>
                  <el-select v-model="tempChartTank" size="small" style="width: 150px">
                    <el-option label="All Tanks" value="all" />
                    <el-option 
                      v-for="tank in tanks" 
                      :key="tank.id"
                      :label="tank.name" 
                      :value="tank.id" 
                    />
                  </el-select>
                </div>
              </template>
              <TemperatureTrendChart :data="temperatureData" height="400px" />
            </ModernCard>

            <!-- pH Levels -->
            <ModernCard class="chart-card scroll-animate">
              <template #header>
                <div class="chart-header">
                  <div>
                    <h3>pH Level Analysis</h3>
                    <p class="chart-subtitle">pH balance monitoring and alerts</p>
                  </div>
                  <el-select v-model="phChartTank" size="small" style="width: 150px">
                    <el-option label="All Tanks" value="all" />
                    <el-option 
                      v-for="tank in tanks" 
                      :key="tank.id"
                      :label="tank.name" 
                      :value="tank.id" 
                    />
                  </el-select>
                </div>
              </template>
              <TemperatureTrendChart :data="temperatureData" height="400px" />
            </ModernCard>

            <!-- Multi-parameter Comparison -->
            <ModernCard class="chart-card scroll-animate">
              <template #header>
                <div class="chart-header">
                  <div>
                    <h3>Multi-Parameter Comparison</h3>
                    <p class="chart-subtitle">Comprehensive water quality metrics</p>
                  </div>
                  <div class="chart-controls">
                    <el-checkbox-group v-model="selectedParameters" size="small">
                      <el-checkbox label="Temperature" />
                      <el-checkbox label="pH" />
                      <el-checkbox label="Oxygen" />
                      <el-checkbox label="Salinity" />
                    </el-checkbox-group>
                  </div>
                </div>
              </template>
              <div class="chart-placeholder">
                <el-icon :size="64"><Histogram /></el-icon>
                <p>Multi-Parameter Chart</p>
                <small>ECharts integration - Coming soon</small>
              </div>
            </ModernCard>
          </div>

          <!-- Sidebar Stats -->
          <div class="charts-sidebar">
            <!-- Tank Performance -->
            <ModernCard class="performance-card scroll-animate">
              <template #header>
                <h3>Tank Performance</h3>
              </template>
              <div class="performance-list">
                <div 
                  v-for="tank in tankPerformance" 
                  :key="tank.id"
                  class="performance-item"
                >
                  <div class="performance-info">
                    <div class="tank-name">{{ tank.name }}</div>
                    <div class="tank-score">{{ tank.score }}%</div>
                  </div>
                  <div class="performance-bar">
                    <div 
                      class="performance-fill" 
                      :style="{ 
                        width: `${tank.score}%`,
                        background: getPerformanceColor(tank.score)
                      }"
                    ></div>
                  </div>
                  <div class="performance-status" :class="getStatusClass(tank.score)">
                    <el-icon><component :is="getStatusIcon(tank.score)" /></el-icon>
                    <span>{{ getStatusText(tank.score) }}</span>
                  </div>
                </div>
              </div>
            </ModernCard>

            <!-- Alert Summary -->
            <ModernCard class="alerts-card scroll-animate">
              <template #header>
                <div class="card-header-flex">
                  <h3>Recent Alerts</h3>
                  <el-badge :value="alertCount" :max="99" />
                </div>
              </template>
              <div class="alerts-list">
                <div 
                  v-for="alert in recentAlerts" 
                  :key="alert.id"
                  class="alert-item"
                  :class="`alert-${alert.severity}`"
                >
                  <div class="alert-icon">
                    <el-icon><component :is="getAlertIcon(alert.severity)" /></el-icon>
                  </div>
                  <div class="alert-content">
                    <div class="alert-title">{{ alert.title }}</div>
                    <div class="alert-time">{{ alert.time }}</div>
                  </div>
                </div>
              </div>
              <button class="btn btn-outline btn-block btn-small" @click="viewAllAlerts">
                View All Alerts
              </button>
            </ModernCard>

            <!-- Water Quality Distribution -->
            <ModernCard class="chart-card scroll-animate">
              <template #header>
                <h3>Water Quality Distribution</h3>
              </template>
              <WaterQualityChart :data="waterQualityData" height="350px" />
            </ModernCard>

            <!-- Sensor Status -->
            <ModernCard class="chart-card scroll-animate">
              <template #header>
                <h3>Sensor Status</h3>
              </template>
              <SensorStatusChart :data="sensorStatusData" height="300px" />
            </ModernCard>

            <!-- System Efficiency Gauge -->
            <ModernCard class="chart-card scroll-animate">
              <template #header>
                <h3>System Performance</h3>
              </template>
              <EfficiencyGaugeChart :value="efficiencyValue" title="System Efficiency" height="300px" />
            </ModernCard>

            <!-- System Health -->
            <ModernCard class="health-card scroll-animate">
              <template #header>
                <h3>System Health</h3>
              </template>
              <div class="health-items">
                <div class="health-item">
                  <div class="health-label">
                    <el-icon><Monitor /></el-icon>
                    <span>Sensors</span>
                  </div>
                  <div class="health-status status-good">
                    <span class="status-dot"></span>
                    <span>Operational</span>
                  </div>
                </div>
                <div class="health-item">
                  <div class="health-label">
                    <el-icon><Connection /></el-icon>
                    <span>Network</span>
                  </div>
                  <div class="health-status status-good">
                    <span class="status-dot"></span>
                    <span>Connected</span>
                  </div>
                </div>
                <div class="health-item">
                  <div class="health-label">
                    <el-icon><Cpu /></el-icon>
                    <span>Database</span>
                  </div>
                  <div class="health-status status-warning">
                    <span class="status-dot"></span>
                    <span>High Load</span>
                  </div>
                </div>
                <div class="health-item">
                  <div class="health-label">
                    <el-icon><CloudyIcon /></el-icon>
                    <span>Storage</span>
                  </div>
                  <div class="health-status status-good">
                    <span class="status-dot"></span>
                    <span>78% Used</span>
                  </div>
                </div>
              </div>
            </ModernCard>
          </div>
        </div>
      </div>
    </section>

    <!-- Data Tables -->
    <section class="section bg--secondary">
      <div class="container">
        <ModernCard class="data-table-card scroll-animate">
          <template #header>
            <div class="table-header">
              <h3>Sensor Data Log</h3>
              <div class="table-actions">
                <el-input 
                  v-model="tableSearch"
                  placeholder="Search..."
                  :prefix-icon="Search"
                  size="small"
                  style="width: 200px"
                  clearable
                />
                <button class="btn btn-outline btn-small" @click="exportTable">
                  <el-icon><Download /></el-icon>
                  <span>Export CSV</span>
                </button>
              </div>
            </div>
          </template>
          
          <div class="table-container">
            <table class="data-table">
              <thead>
                <tr>
                  <th>Timestamp</th>
                  <th>Tank</th>
                  <th>Sensor</th>
                  <th>Parameter</th>
                  <th>Value</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="row in sensorDataLog" :key="row.id">
                  <td>{{ row.timestamp }}</td>
                  <td>{{ row.tank }}</td>
                  <td>{{ row.sensor }}</td>
                  <td>{{ row.parameter }}</td>
                  <td class="value-cell">{{ row.value }}</td>
                  <td>
                    <el-tag :type="getTagType(row.status)" size="small">
                      {{ row.status }}
                    </el-tag>
                  </td>
                  <td>
                    <div class="table-actions-btns">
                      <button class="table-btn" @click="viewDetails(row.id)">
                        <el-icon><View /></el-icon>
                      </button>
                      <button class="table-btn" @click="downloadRow(row.id)">
                        <el-icon><Download /></el-icon>
                      </button>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <template #footer>
            <div class="table-pagination">
              <span class="pagination-info">Showing 1-10 of 156 records</span>
              <el-pagination
                small
                layout="prev, pager, next"
                :total="156"
                :page-size="10"
              />
            </div>
          </template>
        </ModernCard>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useTankStore } from '@/stores/tankStore'
import ModernCard from '@/components/common/ModernCard.vue'
import TemperatureTrendChart from '@/components/charts/TemperatureTrendChart.vue'
import WaterQualityChart from '@/components/charts/WaterQualityChart.vue'
import SensorStatusChart from '@/components/charts/SensorStatusChart.vue'
import EfficiencyGaugeChart from '@/components/charts/EfficiencyGaugeChart.vue'
import { exportAnalyticsData } from '@/utils/exportUtils'
import {
  Download,
  Refresh,
  TrendCharts,
  DataAnalysis,
  Histogram,
  Odometer,
  DataLine,
  Monitor,
  Connection,
  Cpu,
  Search,
  View,
  CircleCheck,
  Warning,
  CircleClose,
  WarningFilled,
  InfoFilled
} from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'

// Component for Cloudy icon (Element Plus doesn't have it)
const CloudyIcon = Cpu

// Store
const tankStore = useTankStore()

// State
const timeRange = ref('7d')
const tempChartTank = ref('all')
const phChartTank = ref('all')
const selectedParameters = ref(['Temperature', 'pH'])
const tableSearch = ref('')

// Computed
const tanks = computed(() => tankStore.tanks)

const systemEfficiency = computed(() => 94.2)
const avgWaterQuality = computed(() => 92.8)
const totalEvents = computed(() => 1247)
const sensorUptime = computed(() => 98.5)

const tankPerformance = computed(() => [
  { id: '1', name: 'Tank Alpha', score: 96 },
  { id: '2', name: 'Tank Beta', score: 88 },
  { id: '3', name: 'Tank Gamma', score: 92 },
  { id: '4', name: 'Tank Delta', score: 78 }
])

const alertCount = computed(() => 12)

const recentAlerts = computed(() => [
  { id: '1', severity: 'warning', title: 'pH level deviation in Tank Alpha', time: '5 min ago' },
  { id: '2', severity: 'info', title: 'Maintenance scheduled', time: '1 hour ago' },
  { id: '3', severity: 'error', title: 'Sensor offline in Tank Delta', time: '2 hours ago' },
  { id: '4', severity: 'warning', title: 'Temperature spike detected', time: '3 hours ago' }
])

const sensorDataLog = computed(() => [
  { id: '1', timestamp: '2025-11-18 14:30', tank: 'Tank Alpha', sensor: 'TMP-001', parameter: 'Temperature', value: '24.5°C', status: 'Normal' },
  { id: '2', timestamp: '2025-11-18 14:28', tank: 'Tank Beta', sensor: 'PH-002', parameter: 'pH', value: '7.2', status: 'Normal' },
  { id: '3', timestamp: '2025-11-18 14:25', tank: 'Tank Gamma', sensor: 'OXY-003', parameter: 'Oxygen', value: '6.8 mg/L', status: 'Warning' },
  { id: '4', timestamp: '2025-11-18 14:20', tank: 'Tank Delta', sensor: 'SAL-004', parameter: 'Salinity', value: '35 ppt', status: 'Normal' },
  { id: '5', timestamp: '2025-11-18 14:15', tank: 'Tank Alpha', sensor: 'TMP-001', parameter: 'Temperature', value: '24.3°C', status: 'Normal' }
])

// Chart Data
const temperatureData = computed(() => ({
  timestamps: ['00:00', '04:00', '08:00', '12:00', '16:00', '20:00', '24:00'],
  temperatures: [22.5, 23.1, 24.2, 25.8, 25.2, 24.0, 23.2],
  minThreshold: 20,
  maxThreshold: 28
}))

const waterQualityData = computed(() => ({
  excellent: 45,
  good: 32,
  fair: 18,
  poor: 5
}))

const sensorStatusData = computed(() => ({
  online: 24,
  offline: 3,
  error: 1,
  calibrating: 2
}))

const efficiencyValue = computed(() => systemEfficiency.value)

// Methods
const getPerformanceColor = (score: number) => {
  if (score >= 90) return 'linear-gradient(90deg, var(--color-success), var(--color-success-light))'
  if (score >= 75) return 'linear-gradient(90deg, var(--color-warning), var(--color-warning-light))'
  return 'linear-gradient(90deg, var(--color-danger), var(--color-danger-light))'
}

const getStatusClass = (score: number) => {
  if (score >= 90) return 'status-excellent'
  if (score >= 75) return 'status-good'
  return 'status-poor'
}

const getStatusIcon = (score: number) => {
  if (score >= 90) return CircleCheck
  if (score >= 75) return Warning
  return CircleClose
}

const getStatusText = (score: number) => {
  if (score >= 90) return 'Excellent'
  if (score >= 75) return 'Good'
  return 'Needs Attention'
}

const getAlertIcon = (severity: string) => {
  if (severity === 'error') return CircleClose
  if (severity === 'warning') return WarningFilled
  return InfoFilled
}

const getTagType = (status: string) => {
  if (status === 'Normal') return 'success'
  if (status === 'Warning') return 'warning'
  return 'danger'
}

const exportData = () => {
  try {
    const exportPayload = {
      metrics: {
        systemEfficiency: systemEfficiency.value,
        avgWaterQuality: avgWaterQuality.value,
        totalEvents: totalEvents.value,
        sensorUptime: sensorUptime.value
      },
      tanks: tankPerformance.value,
      sensors: sensorDataLog.value,
      alerts: recentAlerts.value
    }
    
    exportAnalyticsData(exportPayload, 'csv')
    ElMessage.success('Analytics data exported successfully')
  } catch (error: any) {
    ElMessage.error('Failed to export data: ' + error.message)
  }
}

const refreshData = () => {
  ElMessage.info('Refreshing analytics data...')
  // TODO: Implement refresh
}

const viewAllAlerts = () => {
  ElMessage.info('View all alerts')
  // TODO: Navigate to alerts page
}

const exportTable = () => {
  ElMessage.success('Exporting table data to CSV...')
  // TODO: Implement CSV export
}

const viewDetails = (id: string) => {
  ElMessage.info(`View details for: ${id}`)
}

const downloadRow = (id: string) => {
  ElMessage.success(`Downloading data for: ${id}`)
}

// Lifecycle
onMounted(async () => {
  await tankStore.fetchTanks()
})
</script>

<style lang="scss" scoped>
@import '@/styles/design-system/index.scss';

.modern-analytics-view {
  min-height: 100vh;
}

.page-header {
  background: linear-gradient(135deg, var(--color-primary-50), var(--color-secondary-50));
  
  .header-content {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: var(--space-6);
    flex-wrap: wrap;
  }

  .header-text {
    flex: 1;
    min-width: 300px;
  }

  .header-actions {
    display: flex;
    gap: var(--space-3);
    flex-wrap: wrap;
  }
}

.metrics-grid {
  .metric-card {
    .metric-content {
      .metric-header {
        display: flex;
        align-items: center;
        justify-content: space-between;
        margin-bottom: var(--space-4);
      }

      .metric-icon {
        width: 56px;
        height: 56px;
        display: flex;
        align-items: center;
        justify-content: center;
        background: linear-gradient(135deg, var(--metric-color), var(--metric-color));
        border-radius: var(--border-radius-lg);
        color: white;
        opacity: 0.9;
      }

      .metric-value {
        font-size: var(--font-size-3xl);
        font-weight: var(--font-weight-bold);
        font-family: var(--font-family-display);
        color: var(--color-text-primary);
        line-height: 1;
        margin-bottom: var(--space-2);
      }

      .metric-label {
        font-size: var(--font-size-sm);
        color: var(--color-text-tertiary);
        margin-bottom: var(--space-4);
      }

      .mini-chart {
        height: 40px;
        margin-top: var(--space-2);

        svg {
          width: 100%;
          height: 100%;
        }
      }
    }
  }
}

.charts-layout {
  display: grid;
  grid-template-columns: 1fr 400px;
  gap: var(--space-6);

  @media (max-width: 1200px) {
    grid-template-columns: 1fr;
  }
}

.charts-main {
  display: flex;
  flex-direction: column;
  gap: var(--space-6);
}

.charts-sidebar {
  display: flex;
  flex-direction: column;
  gap: var(--space-6);
}

.chart-card {
  .chart-header {
    display: flex;
    align-items: flex-start;
    justify-content: space-between;
    gap: var(--space-4);
    flex-wrap: wrap;

    h3 {
      font-size: var(--font-size-lg);
      font-weight: var(--font-weight-semibold);
      color: var(--color-text-primary);
      margin: 0 0 var(--space-1);
    }

    .chart-subtitle {
      font-size: var(--font-size-sm);
      color: var(--color-text-tertiary);
      margin: 0;
    }

    .chart-controls {
      display: flex;
      gap: var(--space-2);
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
    background: var(--color-neutral-50);
    border-radius: var(--border-radius-lg);

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
}

.performance-list {
  display: flex;
  flex-direction: column;
  gap: var(--space-5);

  .performance-item {
    .performance-info {
      display: flex;
      justify-content: space-between;
      margin-bottom: var(--space-2);

      .tank-name {
        font-weight: var(--font-weight-medium);
        color: var(--color-text-primary);
      }

      .tank-score {
        font-weight: var(--font-weight-bold);
        color: var(--color-text-secondary);
      }
    }

    .performance-bar {
      height: 8px;
      background: var(--color-neutral-100);
      border-radius: var(--border-radius-full);
      overflow: hidden;
      margin-bottom: var(--space-2);

      .performance-fill {
        height: 100%;
        transition: width var(--transition-base);
        border-radius: var(--border-radius-full);
      }
    }

    .performance-status {
      display: flex;
      align-items: center;
      gap: var(--space-2);
      font-size: var(--font-size-sm);

      &.status-excellent {
        color: var(--color-success);
      }

      &.status-good {
        color: var(--color-warning);
      }

      &.status-poor {
        color: var(--color-danger);
      }
    }
  }
}

.card-header-flex {
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

.alerts-list {
  display: flex;
  flex-direction: column;
  gap: var(--space-3);
  margin-bottom: var(--space-4);

  .alert-item {
    display: flex;
    gap: var(--space-3);
    padding: var(--space-3);
    border-radius: var(--border-radius-md);
    border-left: 3px solid;

    &.alert-error {
      background: var(--color-danger-50);
      border-color: var(--color-danger);
      
      .alert-icon {
        color: var(--color-danger);
      }
    }

    &.alert-warning {
      background: var(--color-warning-50);
      border-color: var(--color-warning);
      
      .alert-icon {
        color: var(--color-warning);
      }
    }

    &.alert-info {
      background: var(--color-info-50);
      border-color: var(--color-info);
      
      .alert-icon {
        color: var(--color-info);
      }
    }

    .alert-icon {
      font-size: var(--font-size-xl);
    }

    .alert-content {
      flex: 1;

      .alert-title {
        font-size: var(--font-size-sm);
        font-weight: var(--font-weight-medium);
        color: var(--color-text-primary);
        margin-bottom: var(--space-1);
      }

      .alert-time {
        font-size: var(--font-size-xs);
        color: var(--color-text-tertiary);
      }
    }
  }
}

.health-items {
  display: flex;
  flex-direction: column;
  gap: var(--space-4);

  .health-item {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: var(--space-3);
    background: var(--color-neutral-50);
    border-radius: var(--border-radius-md);

    .health-label {
      display: flex;
      align-items: center;
      gap: var(--space-2);
      font-size: var(--font-size-sm);
      color: var(--color-text-secondary);
    }

    .health-status {
      display: flex;
      align-items: center;
      gap: var(--space-2);
      font-size: var(--font-size-xs);
      font-weight: var(--font-weight-medium);

      .status-dot {
        width: 8px;
        height: 8px;
        border-radius: 50%;
      }

      &.status-good {
        color: var(--color-success);
        
        .status-dot {
          background: var(--color-success);
          animation: pulse 2s infinite;
        }
      }

      &.status-warning {
        color: var(--color-warning);
        
        .status-dot {
          background: var(--color-warning);
        }
      }
    }
  }
}

.data-table-card {
  .table-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    flex-wrap: wrap;
    gap: var(--space-4);

    h3 {
      font-size: var(--font-size-lg);
      font-weight: var(--font-weight-semibold);
      color: var(--color-text-primary);
      margin: 0;
    }

    .table-actions {
      display: flex;
      gap: var(--space-3);
    }
  }
}

.table-container {
  overflow-x: auto;
}

.data-table {
  width: 100%;
  border-collapse: collapse;

  thead {
    background: var(--color-neutral-50);

    th {
      padding: var(--space-4);
      text-align: left;
      font-size: var(--font-size-sm);
      font-weight: var(--font-weight-semibold);
      color: var(--color-text-secondary);
      text-transform: uppercase;
      letter-spacing: 0.05em;
      border-bottom: 2px solid var(--color-neutral-200);
    }
  }

  tbody {
    tr {
      border-bottom: 1px solid var(--color-neutral-100);
      transition: background var(--transition-fast);

      &:hover {
        background: var(--color-neutral-50);
      }

      td {
        padding: var(--space-4);
        font-size: var(--font-size-sm);
        color: var(--color-text-primary);

        &.value-cell {
          font-weight: var(--font-weight-medium);
          font-family: var(--font-family-mono);
        }
      }
    }
  }

  .table-actions-btns {
    display: flex;
    gap: var(--space-2);

    .table-btn {
      padding: var(--space-1) var(--space-2);
      background: transparent;
      border: none;
      border-radius: var(--border-radius-sm);
      color: var(--color-text-tertiary);
      cursor: pointer;
      transition: all var(--transition-fast);

      &:hover {
        background: var(--color-primary-50);
        color: var(--color-primary-600);
      }
    }
  }
}

.table-pagination {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding-top: var(--space-4);
  border-top: 1px solid var(--color-neutral-200);

  .pagination-info {
    font-size: var(--font-size-sm);
    color: var(--color-text-tertiary);
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
