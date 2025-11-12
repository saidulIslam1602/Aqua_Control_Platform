<template>
  <div class="settings-view">
    <div class="page-header">
      <div class="header-content">
        <h1>System Settings</h1>
        <p>Configure your AquaControl platform settings</p>
      </div>
    </div>

    <div class="settings-container">
      <!-- User Profile Settings -->
      <el-card class="settings-card">
        <template #header>
          <span>User Profile</span>
        </template>
        <el-form :model="userForm" label-width="120px">
          <el-form-item label="First Name">
            <el-input v-model="userForm.firstName" />
          </el-form-item>
          <el-form-item label="Last Name">
            <el-input v-model="userForm.lastName" />
          </el-form-item>
          <el-form-item label="Email">
            <el-input v-model="userForm.email" type="email" />
          </el-form-item>
          <el-form-item label="Username">
            <el-input v-model="userForm.username" disabled />
          </el-form-item>
          <el-form-item>
            <el-button type="primary" @click="updateProfile">Update Profile</el-button>
          </el-form-item>
        </el-form>
      </el-card>

      <!-- Security Settings -->
      <el-card class="settings-card">
        <template #header>
          <span>Security</span>
        </template>
        <el-form :model="securityForm" label-width="120px">
          <el-form-item label="Current Password">
            <el-input v-model="securityForm.currentPassword" type="password" show-password />
          </el-form-item>
          <el-form-item label="New Password">
            <el-input v-model="securityForm.newPassword" type="password" show-password />
          </el-form-item>
          <el-form-item label="Confirm Password">
            <el-input v-model="securityForm.confirmPassword" type="password" show-password />
          </el-form-item>
          <el-form-item>
            <el-button type="primary" @click="changePassword">Change Password</el-button>
          </el-form-item>
        </el-form>
      </el-card>

      <!-- System Preferences -->
      <el-card class="settings-card">
        <template #header>
          <span>System Preferences</span>
        </template>
        <el-form :model="systemForm" label-width="120px">
          <el-form-item label="Theme">
            <el-radio-group v-model="systemForm.theme">
              <el-radio value="light">Light</el-radio>
              <el-radio value="dark">Dark</el-radio>
              <el-radio value="auto">Auto</el-radio>
            </el-radio-group>
          </el-form-item>
          <el-form-item label="Language">
            <el-select v-model="systemForm.language" style="width: 200px">
              <el-option label="English" value="en" />
              <el-option label="Spanish" value="es" />
              <el-option label="French" value="fr" />
            </el-select>
          </el-form-item>
          <el-form-item label="Timezone">
            <el-select v-model="systemForm.timezone" style="width: 200px">
              <el-option label="UTC" value="UTC" />
              <el-option label="America/New_York" value="America/New_York" />
              <el-option label="Europe/London" value="Europe/London" />
              <el-option label="Asia/Tokyo" value="Asia/Tokyo" />
            </el-select>
          </el-form-item>
          <el-form-item label="Date Format">
            <el-select v-model="systemForm.dateFormat" style="width: 200px">
              <el-option label="MM/DD/YYYY" value="MM/DD/YYYY" />
              <el-option label="DD/MM/YYYY" value="DD/MM/YYYY" />
              <el-option label="YYYY-MM-DD" value="YYYY-MM-DD" />
            </el-select>
          </el-form-item>
          <el-form-item>
            <el-button type="primary" @click="updateSystemSettings">Save Preferences</el-button>
          </el-form-item>
        </el-form>
      </el-card>

      <!-- Notification Settings -->
      <el-card class="settings-card">
        <template #header>
          <span>Notifications</span>
        </template>
        <el-form :model="notificationForm" label-width="120px">
          <el-form-item label="Email Alerts">
            <el-switch v-model="notificationForm.emailAlerts" />
          </el-form-item>
          <el-form-item label="SMS Alerts">
            <el-switch v-model="notificationForm.smsAlerts" />
          </el-form-item>
          <el-form-item label="Push Notifications">
            <el-switch v-model="notificationForm.pushNotifications" />
          </el-form-item>
          <el-form-item label="Critical Alerts Only">
            <el-switch v-model="notificationForm.criticalOnly" />
          </el-form-item>
          <el-form-item label="Quiet Hours">
            <el-time-picker
              v-model="notificationForm.quietHours"
              is-range
              range-separator="To"
              start-placeholder="Start time"
              end-placeholder="End time"
              format="HH:mm"
              value-format="HH:mm"
            />
          </el-form-item>
          <el-form-item>
            <el-button type="primary" @click="updateNotificationSettings">Save Notification Settings</el-button>
          </el-form-item>
        </el-form>
      </el-card>

      <!-- System Information -->
      <el-card class="settings-card">
        <template #header>
          <span>System Information</span>
        </template>
        <div class="system-info">
          <div class="info-item">
            <span class="info-label">Application Version:</span>
            <span class="info-value">2.0.0</span>
          </div>
          <div class="info-item">
            <span class="info-label">Database Version:</span>
            <span class="info-value">TimescaleDB 2.11</span>
          </div>
          <div class="info-item">
            <span class="info-label">Last Backup:</span>
            <span class="info-value">{{ formatDate(new Date()) }}</span>
          </div>
          <div class="info-item">
            <span class="info-label">System Uptime:</span>
            <span class="info-value">7 days, 14 hours</span>
          </div>
        </div>
        <div class="system-actions">
          <el-button @click="exportData">Export Data</el-button>
          <el-button @click="runBackup">Run Backup</el-button>
          <el-button type="warning" @click="clearCache">Clear Cache</el-button>
        </div>
      </el-card>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useAuthStore } from '@/stores/authStore'
import { ElMessage } from 'element-plus'

// Store
const authStore = useAuthStore()

// Forms
const userForm = ref({
  firstName: '',
  lastName: '',
  email: '',
  username: ''
})

const securityForm = ref({
  currentPassword: '',
  newPassword: '',
  confirmPassword: ''
})

const systemForm = ref({
  theme: 'light',
  language: 'en',
  timezone: 'UTC',
  dateFormat: 'MM/DD/YYYY'
})

const notificationForm = ref({
  emailAlerts: true,
  smsAlerts: false,
  pushNotifications: true,
  criticalOnly: false,
  quietHours: ['22:00', '06:00']
})

// Methods
const updateProfile = async () => {
  try {
    // TODO: Replace with actual API call
    await new Promise(resolve => setTimeout(resolve, 1000))
    ElMessage.success('Profile updated successfully')
  } catch (error) {
    ElMessage.error('Failed to update profile')
  }
}

const changePassword = async () => {
  if (securityForm.value.newPassword !== securityForm.value.confirmPassword) {
    ElMessage.error('Passwords do not match')
    return
  }

  if (securityForm.value.newPassword.length < 8) {
    ElMessage.error('Password must be at least 8 characters long')
    return
  }

  try {
    // TODO: Replace with actual API call
    await new Promise(resolve => setTimeout(resolve, 1000))
    ElMessage.success('Password changed successfully')
    securityForm.value = {
      currentPassword: '',
      newPassword: '',
      confirmPassword: ''
    }
  } catch (error) {
    ElMessage.error('Failed to change password')
  }
}

const updateSystemSettings = async () => {
  try {
    // TODO: Replace with actual API call
    await new Promise(resolve => setTimeout(resolve, 500))
    ElMessage.success('System preferences saved')
  } catch (error) {
    ElMessage.error('Failed to save preferences')
  }
}

const updateNotificationSettings = async () => {
  try {
    // TODO: Replace with actual API call
    await new Promise(resolve => setTimeout(resolve, 500))
    ElMessage.success('Notification settings saved')
  } catch (error) {
    ElMessage.error('Failed to save notification settings')
  }
}

const exportData = async () => {
  try {
    ElMessage.info('Preparing data export...')
    // TODO: Replace with actual export functionality
    await new Promise(resolve => setTimeout(resolve, 2000))
    ElMessage.success('Data export completed')
  } catch (error) {
    ElMessage.error('Failed to export data')
  }
}

const runBackup = async () => {
  try {
    ElMessage.info('Starting backup...')
    // TODO: Replace with actual backup functionality
    await new Promise(resolve => setTimeout(resolve, 3000))
    ElMessage.success('Backup completed successfully')
  } catch (error) {
    ElMessage.error('Backup failed')
  }
}

const clearCache = async () => {
  try {
    // Clear browser cache
    if ('caches' in window) {
      const cacheNames = await caches.keys()
      await Promise.all(cacheNames.map(name => caches.delete(name)))
    }
    
    // Clear localStorage
    localStorage.clear()
    
    ElMessage.success('Cache cleared successfully')
  } catch (error) {
    ElMessage.error('Failed to clear cache')
  }
}

const formatDate = (date: Date) => {
  return date.toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

// Lifecycle
onMounted(() => {
  // Initialize forms with current user data
  if (authStore.user) {
    userForm.value = {
      firstName: authStore.user.firstName,
      lastName: authStore.user.lastName,
      email: authStore.user.email,
      username: authStore.user.username
    }
  }
})
</script>

<style lang="scss" scoped>
.settings-view {
  padding: 24px;
}

.page-header {
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
}

.settings-container {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
  gap: 24px;
}

.settings-card {
  .el-form {
    .el-form-item:last-child {
      margin-bottom: 0;
    }
  }
}

.system-info {
  margin-bottom: 20px;
}

.info-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 8px 0;
  border-bottom: 1px solid var(--el-border-color-lighter);

  &:last-child {
    border-bottom: none;
  }

  .info-label {
    font-weight: 500;
    color: var(--el-text-color-regular);
  }

  .info-value {
    color: var(--el-text-color-primary);
  }
}

.system-actions {
  display: flex;
  gap: 12px;
  flex-wrap: wrap;
}

@media (max-width: 768px) {
  .settings-container {
    grid-template-columns: 1fr;
  }

  .system-actions {
    flex-direction: column;

    .el-button {
      width: 100%;
    }
  }
}
</style>

