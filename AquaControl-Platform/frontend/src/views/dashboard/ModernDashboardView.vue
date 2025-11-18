<template>
  <ModernLayout>
    <!-- Hero Section -->
    <ModernHero
      badge-text="Advanced Aquaculture Management"
      title="Monitor & Control Your"
      subtitle="Aquaculture Operations"
      description="Real-time monitoring, intelligent alerts, and comprehensive analytics for modern fish farming. Manage your entire operation from a single, powerful platform."
      primary-button-text="View Dashboard"
      secondary-button-text="Watch Demo"
      :show-stats="true"
      :stats="heroStats"
      @primary-action="navigateToDashboard"
      @secondary-action="showDemo"
    />

    <!-- Features Section -->
    <section class="section section--lg bg--secondary">
      <div class="container">
        <div class="section-header scroll-animate">
          <h2 class="modern-heading modern-heading--h2">
            Comprehensive Aquaculture Solutions
          </h2>
          <p class="modern-text modern-text--lead">
            Everything you need to manage your aquaculture operations efficiently and sustainably.
          </p>
        </div>

        <div class="grid grid--3">
          <ModernCard
            v-for="(feature, index) in features"
            :key="feature.title"
            variant="feature"
            :icon="feature.icon"
            :icon-color="feature.iconColor"
            :title="feature.title"
            :description="feature.description"
            :action-text="feature.actionText"
            :hover="true"
            :clickable="true"
            :class="`scroll-animate scroll-animate-delay-${index + 1}`"
            @click="navigateToFeature(feature.path)"
            @action="navigateToFeature(feature.path)"
          />
        </div>
      </div>
    </section>

    <!-- Stats Section -->
    <section class="section section--lg bg--gradient-subtle">
      <div class="container">
        <div class="stats-grid scroll-animate">
          <div 
            v-for="stat in detailedStats" 
            :key="stat.label"
            class="stat-card"
          >
            <div class="stat-icon" :style="{ '--stat-color': stat.color }">
              <el-icon :size="32">
                <component :is="stat.icon" />
              </el-icon>
            </div>
            <div class="stat-content">
              <div class="stat-value">{{ stat.value }}</div>
              <div class="stat-label">{{ stat.label }}</div>
              <div class="stat-change" :class="stat.changeClass">
                {{ stat.change }}
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- Recent Activity Section -->
    <section class="section section--lg">
      <div class="container">
        <div class="section-header scroll-animate">
          <h2 class="modern-heading modern-heading--h2">
            Recent Activity
          </h2>
          <button class="btn btn-secondary" @click="viewAllActivity">
            View All
            <el-icon><ArrowRight /></el-icon>
          </button>
        </div>

        <div class="grid grid--2">
          <ModernCard
            v-for="(activity, index) in recentActivities"
            :key="activity.id"
            variant="news"
            :image-src="activity.image"
            :badge="activity.badge"
            :title="activity.title"
            :description="activity.description"
            :action-text="activity.actionText"
            :hover="true"
            :class="`scroll-animate scroll-animate-delay-${index + 1}`"
            @action="viewActivity(activity.id)"
          >
            <template #footer>
              <div class="activity-meta">
                <span class="activity-time">
                  <el-icon><Clock /></el-icon>
                  {{ activity.time }}
                </span>
                <span class="activity-author">
                  <el-icon><User /></el-icon>
                  {{ activity.author }}
                </span>
              </div>
            </template>
          </ModernCard>
        </div>
      </div>
    </section>

    <!-- CTA Section -->
    <section class="section section--lg bg--gradient">
      <div class="container">
        <div class="cta-content scroll-animate">
          <h2 class="modern-heading modern-heading--h2 text--inverse">
            Ready to Transform Your Aquaculture Operations?
          </h2>
          <p class="modern-text modern-text--lead text--inverse" style="opacity: 0.9;">
            Join hundreds of fish farmers worldwide who trust AquaControl Platform for their operations.
          </p>
          <div class="cta-actions">
            <button class="btn btn-large" style="background: white; color: var(--color-primary-600);" @click="getStarted">
              <span>Get Started Free</span>
              <el-icon><ArrowRight /></el-icon>
            </button>
            <button class="btn btn-large" style="background: transparent; border: 2px solid white; color: white;" @click="contactSales">
              <el-icon><Phone /></el-icon>
              <span>Contact Sales</span>
            </button>
          </div>
        </div>
      </div>
    </section>
  </ModernLayout>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useTankStore } from '@/stores/tankStore'
import { useAlertStore } from '@/stores/alertStore'
import ModernLayout from '@/components/layout/ModernLayout.vue'
import ModernHero from '@/components/common/ModernHero.vue'
import ModernCard from '@/components/common/ModernCard.vue'
import {
  Monitor,
  DataAnalysis,
  Warning,
  Setting,
  TrendCharts,
  Bell,
  ArrowRight,
  Clock,
  User,
  Phone
} from '@element-plus/icons-vue'

// Router
const router = useRouter()

// Stores
const tankStore = useTankStore()
const alertStore = useAlertStore()

// Hero Stats
const heroStats = computed(() => [
  { value: tankStore.activeTanks.length.toString(), label: 'Active Tanks' },
  { value: '99.9%', label: 'System Uptime' },
  { value: alertStore.alertCount.toString(), label: 'Active Alerts' }
])

// Features
const features = [
  {
    title: 'Tank Management',
    description: 'Complete control over your aquaculture tanks with real-time monitoring and automated management systems.',
    icon: Monitor,
    iconColor: 'var(--color-primary-500)',
    actionText: 'Explore Tanks',
    path: '/tanks'
  },
  {
    title: 'Real-time Analytics',
    description: 'Advanced analytics and insights powered by machine learning to optimize your operations and increase yields.',
    icon: DataAnalysis,
    iconColor: 'var(--color-secondary-500)',
    actionText: 'View Analytics',
    path: '/analytics'
  },
  {
    title: 'Smart Alerts',
    description: 'Intelligent alert system that notifies you of critical conditions before they become problems.',
    icon: Warning,
    iconColor: 'var(--color-warning)',
    actionText: 'Configure Alerts',
    path: '/alerts'
  },
  {
    title: 'Sensor Network',
    description: 'Connect unlimited sensors to monitor water quality, temperature, oxygen levels, and more in real-time.',
    icon: TrendCharts,
    iconColor: 'var(--color-info)',
    actionText: 'Manage Sensors',
    path: '/sensors'
  },
  {
    title: 'Automated Control',
    description: 'Set up automated responses to sensor readings to maintain optimal conditions without manual intervention.',
    icon: Setting,
    iconColor: 'var(--color-success)',
    actionText: 'Setup Automation',
    path: '/automation'
  },
  {
    title: 'Predictive Maintenance',
    description: 'AI-powered predictions help you schedule maintenance before equipment failures occur.',
    icon: Bell,
    iconColor: 'var(--color-error)',
    actionText: 'View Schedule',
    path: '/maintenance'
  }
]

// Detailed Stats
const detailedStats = [
  {
    label: 'Total Tanks',
    value: tankStore.tanks.length,
    change: '+12% from last month',
    changeClass: 'positive',
    icon: Monitor,
    color: 'var(--color-primary-500)'
  },
  {
    label: 'Average Health Score',
    value: '94.2%',
    change: '+2.1% from last week',
    changeClass: 'positive',
    icon: TrendCharts,
    color: 'var(--color-success)'
  },
  {
    label: 'Active Sensors',
    value: '1,247',
    change: '+45 new this month',
    changeClass: 'positive',
    icon: DataAnalysis,
    color: 'var(--color-secondary-500)'
  },
  {
    label: 'Response Time',
    value: '< 1 sec',
    change: 'Improved by 15%',
    changeClass: 'positive',
    icon: Warning,
    color: 'var(--color-info)'
  }
]

// Recent Activities
const recentActivities = [
  {
    id: 1,
    title: 'Tank Alpha-01 Activated',
    description: 'Successfully activated new breeding tank with optimal water parameters.',
    badge: 'Success',
    image: 'https://images.unsplash.com/photo-1559827260-dc66d52bef19?w=800',
    time: '2 hours ago',
    author: 'John Doe',
    actionText: 'View Details'
  },
  {
    id: 2,
    title: 'Water Quality Alert Resolved',
    description: 'Automated system adjusted pH levels in Tank Beta-03 back to optimal range.',
    badge: 'Resolved',
    image: 'https://images.unsplash.com/photo-1535591273668-578e31182c4f?w=800',
    time: '5 hours ago',
    author: 'System',
    actionText: 'View Report'
  },
  {
    id: 3,
    title: 'Maintenance Completed',
    description: 'Scheduled maintenance on filtration system completed successfully.',
    badge: 'Maintenance',
    image: 'https://images.unsplash.com/photo-1593642634367-d91a135587b5?w=800',
    time: '1 day ago',
    author: 'Jane Smith',
    actionText: 'View Log'
  },
  {
    id: 4,
    title: 'New Sensors Installed',
    description: 'Added 15 new temperature sensors across grow-out tanks.',
    badge: 'Update',
    image: 'https://images.unsplash.com/photo-1581092160562-40aa08e78837?w=800',
    time: '2 days ago',
    author: 'Tech Team',
    actionText: 'Configure'
  }
]

// Methods
const navigateToDashboard = () => {
  router.push('/dashboard')
}

const showDemo = () => {
  // Implement demo modal
  console.log('Show demo')
}

const navigateToFeature = (path: string) => {
  router.push(path)
}

const viewAllActivity = () => {
  router.push('/activity')
}

const viewActivity = (id: number) => {
  router.push(`/activity/${id}`)
}

const getStarted = () => {
  router.push('/signup')
}

const contactSales = () => {
  router.push('/contact')
}
</script>

<style lang="scss" scoped>
@import '@/styles/design-system/index.scss';

.section-header {
  text-align: center;
  max-width: 800px;
  margin: 0 auto var(--space-16);
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--space-4);

  .modern-heading {
    margin-bottom: var(--space-4);
  }
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: var(--space-6);
}

.stat-card {
  display: flex;
  align-items: center;
  gap: var(--space-4);
  padding: var(--space-6);
  background: white;
  border-radius: var(--border-radius-xl);
  box-shadow: var(--shadow-sm);
  transition: all var(--transition-base);

  &:hover {
    transform: translateY(-4px);
    box-shadow: var(--shadow-lg);
  }

  .stat-icon {
    width: 64px;
    height: 64px;
    display: flex;
    align-items: center;
    justify-content: center;
    background: linear-gradient(135deg, var(--stat-color, var(--color-primary-500)), var(--stat-color, var(--color-primary-400)));
    border-radius: var(--border-radius-lg);
    color: white;
    flex-shrink: 0;
  }

  .stat-content {
    flex: 1;
  }

  .stat-value {
    font-size: var(--font-size-2xl);
    font-weight: var(--font-weight-bold);
    font-family: var(--font-family-display);
    color: var(--color-text-primary);
    line-height: 1.2;
    margin-bottom: var(--space-1);
  }

  .stat-label {
    font-size: var(--font-size-sm);
    color: var(--color-text-tertiary);
    margin-bottom: var(--space-2);
  }

  .stat-change {
    font-size: var(--font-size-xs);
    font-weight: var(--font-weight-medium);

    &.positive {
      color: var(--color-success);
    }

    &.negative {
      color: var(--color-error);
    }
  }
}

.activity-meta {
  display: flex;
  align-items: center;
  gap: var(--space-4);
  font-size: var(--font-size-sm);
  color: var(--color-text-tertiary);

  span {
    display: flex;
    align-items: center;
    gap: var(--space-2);
  }
}

.cta-content {
  text-align: center;
  max-width: 800px;
  margin: 0 auto;
  padding: var(--space-20) 0;

  .modern-heading {
    margin-bottom: var(--space-6);
  }

  .modern-text {
    margin-bottom: var(--space-10);
  }
}

.cta-actions {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: var(--space-4);
  flex-wrap: wrap;

  @media (max-width: 640px) {
    flex-direction: column;
    width: 100%;

    .btn {
      width: 100%;
    }
  }
}
</style>
