<template>
  <div class="analytics-view">
    <div class="page-header">
      <div class="header-content">
        <h1>Analytics Dashboard</h1>
        <p>Comprehensive analytics and insights for your aquaculture operations</p>
      </div>
      <div class="header-actions">
        <el-date-picker
          v-model="dateRange"
          type="datetimerange"
          range-separator="To"
          start-placeholder="Start date"
          end-placeholder="End date"
          format="YYYY-MM-DD HH:mm:ss"
          value-format="YYYY-MM-DD HH:mm:ss"
        />
        <el-button type="primary" @click="refreshAnalytics" :loading="isLoading">
          <el-icon><Refresh /></el-icon>
          Refresh
        </el-button>
      </div>
    </div>

    <!-- Analytics Cards -->
    <div class="analytics-grid">
      <!-- Water Quality Trends -->
      <el-card class="analytics-card">
        <template #header>
          <div class="card-header">
            <span>Water Quality Trends</span>
            <el-select v-model="selectedMetric" style="width: 150px">
              <el-option label="Temperature" value="temperature" />
              <el-option label="pH Level" value="ph" />
              <el-option label="Dissolved Oxygen" value="oxygen" />
              <el-option label="Salinity" value="salinity" />
            </el-select>
          </div>
        </template>
        <div class="chart-container">
          <div class="chart-placeholder">
            <el-icon><TrendCharts /></el-icon>
            <p>Water Quality Chart</p>
            <small>{{ selectedMetric.toUpperCase() }} trends over time</small>
          </div>
        </div>
      </el-card>

      <!-- Tank Performance -->
      <el-card class="analytics-card">
        <template #header>
          <span>Tank Performance</span>
        </template>
        <div class="performance-metrics">
          <div class="metric-item">
            <div class="metric-label">Average Growth Rate</div>
            <div class="metric-value">+2.3%</div>
            <div class="metric-trend positive">↗ +0.5%</div>
          </div>
          <div class="metric-item">
            <div class="metric-label">Feed Conversion Ratio</div>
            <div class="metric-value">1.4:1</div>
            <div class="metric-trend positive">↗ Improved</div>
          </div>
          <div class="metric-item">
            <div class="metric-label">Mortality Rate</div>
            <div class="metric-value">0.8%</div>
            <div class="metric-trend positive">↘ -0.2%</div>
          </div>
          <div class="metric-item">
            <div class="metric-label">System Uptime</div>
            <div class="metric-value">99.2%</div>
            <div class="metric-trend positive">↗ +0.1%</div>
          </div>
        </div>
      </el-card>

      <!-- Alert Analysis -->
      <el-card class="analytics-card">
        <template #header>
          <span>Alert Analysis</span>
        </template>
        <div class="alert-stats">
          <div class="alert-stat critical">
            <div class="stat-number">{{ alertStore.criticalAlerts.length }}</div>
            <div class="stat-label">Critical Alerts</div>
          </div>
          <div class="alert-stat warning">
            <div class="stat-number">{{ alertStore.warningAlerts.length }}</div>
            <div class="stat-label">Warning Alerts</div>
          </div>
          <div class="alert-stat info">
            <div class="stat-number">{{ alertStore.infoAlerts.length }}</div>
            <div class="stat-label">Info Alerts</div>
          </div>
        </div>
        <div class="alert-chart">
          <div class="chart-placeholder">
            <el-icon><PieChart /></el-icon>
            <p>Alert Distribution</p>
          </div>
        </div>
      </el-card>

      <!-- Production Analytics -->
      <el-card class="analytics-card">
        <template #header>
          <span>Production Analytics</span>
        </template>
        <div class="production-stats">
          <div class="production-item">
            <div class="production-icon">
              <el-icon><DataBoard /></el-icon>
            </div>
            <div class="production-info">
              <div class="production-value">1,247 kg</div>
              <div class="production-label">Total Harvest (This Month)</div>
              <div class="production-change">+12% vs last month</div>
            </div>
          </div>
          <div class="production-item">
            <div class="production-icon">
              <el-icon><Money /></el-icon>
            </div>
            <div class="production-info">
              <div class="production-value">$24,890</div>
              <div class="production-label">Revenue (This Month)</div>
              <div class="production-change">+8% vs last month</div>
            </div>
          </div>
        </div>
      </el-card>

      <!-- Environmental Impact -->
      <el-card class="analytics-card">
        <template #header>
          <span>Environmental Impact</span>
        </template>
        <div class="environmental-metrics">
          <div class="env-metric">
            <div class="env-icon water">
              <el-icon><Drizzling /></el-icon>
            </div>
            <div class="env-info">
              <div class="env-value">2,340 L</div>
              <div class="env-label">Water Usage Today</div>
            </div>
          </div>
          <div class="env-metric">
            <div class="env-icon energy">
              <el-icon><Lightning /></el-icon>
            </div>
            <div class="env-info">
              <div class="env-value">145 kWh</div>
              <div class="env-label">Energy Consumption</div>
            </div>
          </div>
          <div class="env-metric">
            <div class="env-icon efficiency">
              <el-icon><CircleCheck /></el-icon>
            </div>
            <div class="env-info">
              <div class="env-value">94%</div>
              <div class="env-label">Efficiency Score</div>
            </div>
          </div>
        </div>
      </el-card>

      <!-- Recent Reports -->
      <el-card class="analytics-card">
        <template #header>
          <div class="card-header">
            <span>Recent Reports</span>
            <el-button size="small" type="primary">Generate Report</el-button>
          </div>
        </template>
        <div class="reports-list">
          <div class="report-item" v-for="report in recentReports" :key="report.id">
            <div class="report-icon">
              <el-icon><Document /></el-icon>
            </div>
            <div class="report-info">
              <div class="report-name">{{ report.name }}</div>
              <div class="report-date">{{ formatDate(report.createdAt) }}</div>
            </div>
            <div class="report-actions">
              <el-button size="small" text>Download</el-button>
            </div>
          </div>
        </div>
      </el-card>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useAlertStore } from '@/stores/alertStore'
import { useTankStore } from '@/stores/tankStore'
import { 
  Refresh,
  TrendCharts,
  PieChart,
  DataBoard,
  Money,
  Drizzling,
  Lightning,
  CircleCheck,
  Document
} from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'

// Stores
const alertStore = useAlertStore()
const tankStore = useTankStore()

// State
const isLoading = ref(false)
const dateRange = ref<[string, string]>([
  new Date(Date.now() - 7 * 24 * 60 * 60 * 1000).toISOString().slice(0, 19), // 7 days ago
  new Date().toISOString().slice(0, 19) // now
])
const selectedMetric = ref('temperature')

const recentReports = ref([
  {
    id: 1,
    name: 'Weekly Water Quality Report',
    createdAt: new Date(Date.now() - 1000 * 60 * 60 * 24) // 1 day ago
  },
  {
    id: 2,
    name: 'Monthly Production Summary',
    createdAt: new Date(Date.now() - 1000 * 60 * 60 * 24 * 3) // 3 days ago
  },
  {
    id: 3,
    name: 'Environmental Impact Assessment',
    createdAt: new Date(Date.now() - 1000 * 60 * 60 * 24 * 7) // 1 week ago
  }
])

// Methods
const refreshAnalytics = async () => {
  isLoading.value = true
  try {
    // TODO: Replace with actual API calls
    await new Promise(resolve => setTimeout(resolve, 1500))
    ElMessage.success('Analytics data refreshed')
  } catch (error) {
    ElMessage.error('Failed to refresh analytics')
  } finally {
    isLoading.value = false
  }
}

const formatDate = (date: Date) => {
  return date.toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric'
  })
}

// Lifecycle
onMounted(async () => {
  await alertStore.fetchAlerts()
  await refreshAnalytics()
})
</script>

<style lang="scss" scoped>
.analytics-view {
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
    align-items: center;
  }
}

.analytics-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
  gap: 24px;
}

.analytics-card {
  .card-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
  }
}

.chart-container {
  height: 300px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.chart-placeholder {
  text-align: center;
  color: var(--el-text-color-regular);

  .el-icon {
    font-size: 48px;
    margin-bottom: 12px;
    opacity: 0.5;
  }

  p {
    margin: 8px 0 4px 0;
    font-weight: 500;
  }

  small {
    opacity: 0.7;
  }
}

.performance-metrics {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 16px;
}

.metric-item {
  padding: 16px;
  border: 1px solid var(--el-border-color-light);
  border-radius: 8px;

  .metric-label {
    font-size: 12px;
    color: var(--el-text-color-regular);
    margin-bottom: 8px;
  }

  .metric-value {
    font-size: 20px;
    font-weight: 600;
    color: var(--el-text-color-primary);
    margin-bottom: 4px;
  }

  .metric-trend {
    font-size: 12px;
    font-weight: 500;

    &.positive {
      color: #67c23a;
    }

    &.negative {
      color: #f56c6c;
    }
  }
}

.alert-stats {
  display: flex;
  justify-content: space-around;
  margin-bottom: 20px;
}

.alert-stat {
  text-align: center;

  .stat-number {
    font-size: 24px;
    font-weight: 600;
    margin-bottom: 4px;
  }

  .stat-label {
    font-size: 12px;
    color: var(--el-text-color-regular);
  }

  &.critical .stat-number {
    color: #f56c6c;
  }

  &.warning .stat-number {
    color: #e6a23c;
  }

  &.info .stat-number {
    color: #409eff;
  }
}

.alert-chart {
  height: 200px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.production-stats {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.production-item {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 16px;
  border: 1px solid var(--el-border-color-light);
  border-radius: 8px;

  .production-icon {
    width: 48px;
    height: 48px;
    background-color: #f0f9ff;
    color: #0ea5e9;
    border-radius: 8px;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 24px;
  }

  .production-info {
    .production-value {
      font-size: 24px;
      font-weight: 600;
      color: var(--el-text-color-primary);
      margin-bottom: 4px;
    }

    .production-label {
      font-size: 14px;
      color: var(--el-text-color-regular);
      margin-bottom: 4px;
    }

    .production-change {
      font-size: 12px;
      color: #67c23a;
      font-weight: 500;
    }
  }
}

.environmental-metrics {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 16px;
}

.env-metric {
  text-align: center;
  padding: 16px;

  .env-icon {
    width: 40px;
    height: 40px;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    margin: 0 auto 12px;
    font-size: 20px;

    &.water {
      background-color: #e6f3ff;
      color: #1890ff;
    }

    &.energy {
      background-color: #fff7e6;
      color: #fa8c16;
    }

    &.efficiency {
      background-color: #f6ffed;
      color: #52c41a;
    }
  }

  .env-info {
    .env-value {
      font-size: 18px;
      font-weight: 600;
      color: var(--el-text-color-primary);
      margin-bottom: 4px;
    }

    .env-label {
      font-size: 12px;
      color: var(--el-text-color-regular);
    }
  }
}

.reports-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.report-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px;
  border: 1px solid var(--el-border-color-lighter);
  border-radius: 6px;
  transition: all 0.3s;

  &:hover {
    border-color: var(--el-color-primary);
    background-color: var(--el-fill-color-extra-light);
  }

  .report-icon {
    width: 32px;
    height: 32px;
    background-color: var(--el-fill-color-light);
    border-radius: 6px;
    display: flex;
    align-items: center;
    justify-content: center;
    color: var(--el-text-color-regular);
  }

  .report-info {
    flex: 1;

    .report-name {
      font-weight: 500;
      color: var(--el-text-color-primary);
      margin-bottom: 2px;
    }

    .report-date {
      font-size: 12px;
      color: var(--el-text-color-regular);
    }
  }
}

@media (max-width: 768px) {
  .analytics-grid {
    grid-template-columns: 1fr;
  }

  .page-header {
    flex-direction: column;
    gap: 16px;

    .header-actions {
      width: 100%;
      flex-direction: column;
      gap: 12px;
    }
  }

  .performance-metrics {
    grid-template-columns: 1fr;
  }

  .environmental-metrics {
    grid-template-columns: 1fr;
  }
}
</style>

