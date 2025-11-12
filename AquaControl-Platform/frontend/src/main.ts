import { createApp } from 'vue'
import { createPinia } from 'pinia'
import piniaPluginPersistedstate from 'pinia-plugin-persistedstate'
import ElementPlus from 'element-plus'
import 'element-plus/dist/index.css'
import * as ElementPlusIconsVue from '@element-plus/icons-vue'

import App from './App.vue'
import router from './router'

// Create app
const app = createApp(App)

// Create pinia store
const pinia = createPinia()
pinia.use(piniaPluginPersistedstate)

// Use plugins
app.use(pinia)
app.use(router)
app.use(ElementPlus)

// Register Element Plus icons
for (const [key, component] of Object.entries(ElementPlusIconsVue)) {
  app.component(key, component)
}

// Global error handler
app.config.errorHandler = (err, _instance, info) => {
  console.error('Global error:', err, info)
  // Send to error reporting service
}

// Mount app
app.mount('#app')
