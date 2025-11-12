<template>
  <div class="app-layout">
    <el-container>
      <!-- Sidebar -->
      <el-aside :width="isCollapsed ? '64px' : '240px'" class="app-sidebar">
        <div class="sidebar-header">
          <h1 v-if="!isCollapsed" class="app-title">AquaControl</h1>
          <el-icon v-else class="app-icon"><Water /></el-icon>
        </div>
        
        <el-menu
          :default-active="activeRoute"
          :collapse="isCollapsed"
          router
          class="sidebar-menu"
        >
          <el-menu-item index="/">
            <el-icon><Odometer /></el-icon>
            <template #title>Dashboard</template>
          </el-menu-item>
          
          <el-menu-item index="/tanks">
            <el-icon><Box /></el-icon>
            <template #title>Tanks</template>
          </el-menu-item>
          
          <el-menu-item index="/sensors">
            <el-icon><Cpu /></el-icon>
            <template #title>Sensors</template>
          </el-menu-item>
          
          <el-menu-item index="/analytics">
            <el-icon><DataAnalysis /></el-icon>
            <template #title>Analytics</template>
          </el-menu-item>
          
          <el-menu-item index="/settings">
            <el-icon><Setting /></el-icon>
            <template #title>Settings</template>
          </el-menu-item>
        </el-menu>
      </el-aside>

      <!-- Main Content -->
      <el-container>
        <!-- Header -->
        <el-header class="app-header">
          <div class="header-left">
            <el-button
              :icon="isCollapsed ? Expand : Fold"
              @click="toggleSidebar"
              text
            />
          </div>
          
          <div class="header-right">
            <el-dropdown @command="handleCommand">
              <span class="user-info">
                <el-avatar :size="32" :icon="UserFilled" />
                <span class="username">{{ authStore.user?.username || 'User' }}</span>
              </span>
              <template #dropdown>
                <el-dropdown-menu>
                  <el-dropdown-item command="profile">
                    <el-icon><User /></el-icon>
                    Profile
                  </el-dropdown-item>
                  <el-dropdown-item command="settings">
                    <el-icon><Setting /></el-icon>
                    Settings
                  </el-dropdown-item>
                  <el-dropdown-item divided command="logout">
                    <el-icon><SwitchButton /></el-icon>
                    Logout
                  </el-dropdown-item>
                </el-dropdown-menu>
              </template>
            </el-dropdown>
          </div>
        </el-header>

        <!-- Content Area -->
        <el-main class="app-main">
          <router-view />
        </el-main>
      </el-container>
    </el-container>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'
import { useRealTimeStore } from '@/stores/realTimeStore'
import {
  Histogram as Water, Odometer, Box, Cpu, DataAnalysis, Setting,
  UserFilled, User, SwitchButton, Fold, Expand
} from '@element-plus/icons-vue'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()
const realTimeStore = useRealTimeStore()

const isCollapsed = ref(false)

const activeRoute = computed(() => route.path)

const toggleSidebar = () => {
  isCollapsed.value = !isCollapsed.value
}

const handleCommand = async (command: string) => {
  switch (command) {
    case 'profile':
      router.push('/profile')
      break
    case 'settings':
      router.push('/settings')
      break
    case 'logout':
      await authStore.logout()
      await realTimeStore.disconnect()
      router.push('/login')
      break
  }
}
</script>

<style lang="scss" scoped>
.app-layout {
  height: 100vh;
  width: 100vw;
  
  .app-sidebar {
    background: var(--el-bg-color);
    border-right: 1px solid var(--el-border-color);
    transition: width 0.3s;
    
    .sidebar-header {
      height: 60px;
      display: flex;
      align-items: center;
      justify-content: center;
      border-bottom: 1px solid var(--el-border-color);
      
      .app-title {
        margin: 0;
        font-size: 20px;
        font-weight: 600;
        color: var(--el-text-color-primary);
      }
      
      .app-icon {
        font-size: 24px;
        color: var(--el-color-primary);
      }
    }
    
    .sidebar-menu {
      border-right: none;
      height: calc(100vh - 60px);
    }
  }
  
  .app-header {
    background: var(--el-bg-color);
    border-bottom: 1px solid var(--el-border-color);
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 0 24px;
    
    .header-left {
      display: flex;
      align-items: center;
    }
    
    .header-right {
      .user-info {
        display: flex;
        align-items: center;
        gap: 8px;
        cursor: pointer;
        
        .username {
          font-size: 14px;
          color: var(--el-text-color-primary);
        }
      }
    }
  }
  
  .app-main {
    background: var(--el-bg-color-page);
    padding: 24px;
    overflow-y: auto;
  }
}
</style>

