import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'
import type { RouteRecordRaw } from 'vue-router'

// Lazy load components
const DashboardView = () => import('@/views/dashboard/DashboardView.vue')
const TanksView = () => import('@/views/TankList.vue')
const TankDetailView = () => import('@/views/TankDetail.vue')
const SensorsView = () => import('@/views/sensor/SensorsView.vue')
const AnalyticsView = () => import('@/views/analytics/AnalyticsView.vue')
const SettingsView = () => import('@/views/settings/SettingsView.vue')
const NotFoundView = () => import('@/views/common/NotFoundView.vue')

const routes: RouteRecordRaw[] = [
  {
    path: '/',
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
  scrollBehavior(to, from, savedPosition) {
    if (savedPosition) {
      return savedPosition
    } else {
      return { top: 0 }
    }
  }
})

// Navigation guards
router.beforeEach(async (to, from, next) => {
  const authStore = useAuthStore()
  
  // Set page title
  document.title = to.meta.title ? `${to.meta.title} - AquaControl` : 'AquaControl'
  
  // Check authentication
  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    // Redirect to login (will be handled by App.vue)
    next({ name: 'Dashboard' })
    return
  }
  
  // Check if user is authenticated but trying to access login
  if (to.name === 'Login' && authStore.isAuthenticated) {
    next({ name: 'Dashboard' })
    return
  }
  
  next()
})

export default router
