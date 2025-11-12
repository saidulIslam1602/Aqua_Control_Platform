import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      redirect: '/tanks'
    },
    {
      path: '/tanks',
      name: 'tanks',
      component: () => import('@/views/TankList.vue')
    },
    {
      path: '/tanks/:id',
      name: 'tank-detail',
      component: () => import('@/views/TankDetail.vue')
    }
  ]
})

export default router
