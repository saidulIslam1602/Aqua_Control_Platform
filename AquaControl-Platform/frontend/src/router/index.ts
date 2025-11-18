import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'
import type { RouteRecordRaw } from 'vue-router'

// Lazy load components - All Modern Views
const DashboardView = () => import('@/views/dashboard/ModernDashboardView.vue')
const TanksView = () => import('@/views/tanks/ModernTankListView.vue')
const TankDetailView = () => import('@/views/tanks/ModernTankDetailView.vue')
const SensorsView = () => import('@/views/sensors/ModernSensorsView.vue')
const SensorDetailView = () => import('@/views/sensors/ModernSensorDetailView.vue')
const AnalyticsView = () => import('@/views/analytics/ModernAnalyticsView.vue')
const SettingsView = () => import('@/views/settings/ModernSettingsView.vue')
const NotFoundView = () => import('@/views/common/NotFoundView.vue')

const routes: RouteRecordRaw[] = [
  {
    path: '/',
    name: 'Home',
    redirect: '/dashboard'
  },
  {
    path: '/dashboard',
    name: 'Dashboard',
    component: DashboardView,
    meta: { requiresAuth: true, title: 'Dashboard' }
  },
  {
    path: '/tanks',
    name: 'Tanks',
    component: TanksView,
    meta: { requiresAuth: true, title: 'Tanks' }
  },
  {
    path: '/tanks/:id',
    name: 'TankDetail',
    component: TankDetailView,
    meta: { requiresAuth: true, title: 'Tank Details' },
    props: true
  },
  {
    path: '/sensors',
    name: 'Sensors',
    component: SensorsView,
    meta: { requiresAuth: true, title: 'Sensors' }
  },
  {
    path: '/sensors/:id',
    name: 'SensorDetail',
    component: SensorDetailView,
    meta: { requiresAuth: true, title: 'Sensor Details' },
    props: true
  },
  {
    path: '/analytics',
    name: 'Analytics',
    component: AnalyticsView,
    meta: { requiresAuth: true, title: 'Analytics' }
  },
  {
    path: '/settings',
    name: 'Settings',
    component: SettingsView,
    meta: { requiresAuth: true, title: 'Settings' }
  },
  {
    path: '/:pathMatch(.*)*',
    name: 'NotFound',
    component: NotFoundView,
    meta: { title: 'Page Not Found' }
  }
]

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
  scrollBehavior(_to, _from, savedPosition) {
    if (savedPosition) {
      return savedPosition
    } else {
      return { top: 0 }
    }
  }
})

// Navigation guards
router.beforeEach(async (_to, _from, next) => {
  const authStore = useAuthStore()
  
  // Set page title
  document.title = _to.meta.title ? `${_to.meta.title} - AquaControl` : 'AquaControl'
  
  // Check authentication
  if (_to.meta.requiresAuth && !authStore.isAuthenticated) {
    // User needs to authenticate - App.vue will show login form
    next(false) // Cancel navigation, let App.vue handle showing login
    return
  }
  
  // Check if user is authenticated but trying to access login
  if (_to.name === 'Login' && authStore.isAuthenticated) {
    next({ name: 'Dashboard' })
    return
  }
  
  next()
})

export default router
