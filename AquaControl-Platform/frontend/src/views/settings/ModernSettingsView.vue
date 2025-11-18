<template>
  <div class="modern-settings-view">
    <!-- Page Header -->
    <section class="page-header section--sm">
      <div class="container">
        <div class="header-content">
          <div class="header-text">
            <h1 class="modern-heading modern-heading--h1">Settings</h1>
            <p class="modern-text modern-text--lead">
              Manage your account, preferences, and system configuration
            </p>
          </div>
        </div>
      </div>
    </section>

    <!-- Settings Content -->
    <section class="section">
      <div class="container">
        <div class="settings-layout">
          <!-- Settings Navigation -->
          <div class="settings-sidebar">
            <nav class="settings-nav">
              <button
                v-for="tab in settingsTabs"
                :key="tab.id"
                class="nav-item"
                :class="{ active: activeTab === tab.id }"
                @click="activeTab = tab.id"
              >
                <el-icon><component :is="tab.icon" /></el-icon>
                <span>{{ tab.label }}</span>
              </button>
            </nav>
          </div>

          <!-- Settings Content -->
          <div class="settings-content">
            <!-- Profile Settings -->
            <ModernCard v-show="activeTab === 'profile'" class="settings-card scroll-animate">
              <template #header>
                <div class="card-header">
                  <h3>Profile Settings</h3>
                  <p class="card-subtitle">Manage your personal information and avatar</p>
                </div>
              </template>

              <div class="settings-section">
                <div class="profile-avatar">
                  <div class="avatar-preview">
                    <img :src="profileData.avatar" :alt="profileData.name" />
                  </div>
                  <div class="avatar-actions">
                    <button class="btn btn-outline btn-small">
                      <el-icon><Upload /></el-icon>
                      <span>Upload Photo</span>
                    </button>
                    <button class="btn btn-text btn-small">Remove</button>
                  </div>
                </div>

                <div class="form-grid">
                  <div class="form-group">
                    <label>Full Name</label>
                    <el-input v-model="profileData.name" size="large" />
                  </div>
                  <div class="form-group">
                    <label>Email Address</label>
                    <el-input v-model="profileData.email" type="email" size="large" />
                  </div>
                  <div class="form-group">
                    <label>Phone Number</label>
                    <el-input v-model="profileData.phone" size="large" />
                  </div>
                  <div class="form-group">
                    <label>Role</label>
                    <el-input v-model="profileData.role" size="large" disabled />
                  </div>
                  <div class="form-group full-width">
                    <label>Bio</label>
                    <el-input
                      v-model="profileData.bio"
                      type="textarea"
                      :rows="3"
                      placeholder="Tell us about yourself..."
                    />
                  </div>
                </div>

                <div class="form-actions">
                  <button class="btn btn-outline">Cancel</button>
                  <button class="btn btn-primary" @click="saveProfile">
                    <el-icon><Check /></el-icon>
                    <span>Save Changes</span>
                  </button>
                </div>
              </div>
            </ModernCard>

            <!-- Account Settings -->
            <ModernCard v-show="activeTab === 'account'" class="settings-card scroll-animate">
              <template #header>
                <div class="card-header">
                  <h3>Account Settings</h3>
                  <p class="card-subtitle">Manage your account security and login preferences</p>
                </div>
              </template>

              <div class="settings-section">
                <div class="setting-item">
                  <div class="setting-info">
                    <h4>Change Password</h4>
                    <p>Update your password regularly to keep your account secure</p>
                  </div>
                  <button class="btn btn-outline" @click="showChangePassword = true">
                    <el-icon><Lock /></el-icon>
                    <span>Change Password</span>
                  </button>
                </div>

                <div class="setting-item">
                  <div class="setting-info">
                    <h4>Two-Factor Authentication</h4>
                    <p>Add an extra layer of security to your account</p>
                  </div>
                  <el-switch v-model="accountSettings.twoFactorAuth" size="large" />
                </div>

                <div class="setting-item">
                  <div class="setting-info">
                    <h4>Session Timeout</h4>
                    <p>Automatically log out after period of inactivity</p>
                  </div>
                  <el-select v-model="accountSettings.sessionTimeout" size="large" style="width: 180px">
                    <el-option label="15 minutes" :value="15" />
                    <el-option label="30 minutes" :value="30" />
                    <el-option label="1 hour" :value="60" />
                    <el-option label="4 hours" :value="240" />
                  </el-select>
                </div>

                <div class="setting-item danger-zone">
                  <div class="setting-info">
                    <h4>Delete Account</h4>
                    <p>Permanently delete your account and all associated data</p>
                  </div>
                  <button class="btn btn-danger" @click="confirmDeleteAccount">
                    <el-icon><Delete /></el-icon>
                    <span>Delete Account</span>
                  </button>
                </div>
              </div>
            </ModernCard>

            <!-- Notifications Settings -->
            <ModernCard v-show="activeTab === 'notifications'" class="settings-card scroll-animate">
              <template #header>
                <div class="card-header">
                  <h3>Notification Preferences</h3>
                  <p class="card-subtitle">Choose how and when you want to be notified</p>
                </div>
              </template>

              <div class="settings-section">
                <div class="notification-group">
                  <h4>Email Notifications</h4>
                  <div class="notification-item">
                    <div class="notification-info">
                      <span>Alert Notifications</span>
                      <small>Receive emails when alerts are triggered</small>
                    </div>
                    <el-switch v-model="notificationSettings.email.alerts" />
                  </div>
                  <div class="notification-item">
                    <div class="notification-info">
                      <span>Maintenance Reminders</span>
                      <small>Get notified about upcoming maintenance</small>
                    </div>
                    <el-switch v-model="notificationSettings.email.maintenance" />
                  </div>
                  <div class="notification-item">
                    <div class="notification-info">
                      <span>Weekly Reports</span>
                      <small>Receive weekly summary reports</small>
                    </div>
                    <el-switch v-model="notificationSettings.email.reports" />
                  </div>
                </div>

                <div class="notification-group">
                  <h4>Push Notifications</h4>
                  <div class="notification-item">
                    <div class="notification-info">
                      <span>Critical Alerts</span>
                      <small>Instant notifications for critical issues</small>
                    </div>
                    <el-switch v-model="notificationSettings.push.critical" />
                  </div>
                  <div class="notification-item">
                    <div class="notification-info">
                      <span>Sensor Updates</span>
                      <small>Real-time sensor reading notifications</small>
                    </div>
                    <el-switch v-model="notificationSettings.push.sensors" />
                  </div>
                </div>

                <div class="form-actions">
                  <button class="btn btn-outline">Reset to Default</button>
                  <button class="btn btn-primary" @click="saveNotifications">
                    <el-icon><Check /></el-icon>
                    <span>Save Preferences</span>
                  </button>
                </div>
              </div>
            </ModernCard>

            <!-- Appearance Settings -->
            <ModernCard v-show="activeTab === 'appearance'" class="settings-card scroll-animate">
              <template #header>
                <div class="card-header">
                  <h3>Appearance</h3>
                  <p class="card-subtitle">Customize the look and feel of your dashboard</p>
                </div>
              </template>

              <div class="settings-section">
                <div class="setting-item">
                  <div class="setting-info">
                    <h4>Theme</h4>
                    <p>Choose your preferred color theme</p>
                  </div>
                  <el-radio-group v-model="appearanceSettings.theme" size="large">
                    <el-radio-button value="light">Light</el-radio-button>
                    <el-radio-button value="dark">Dark</el-radio-button>
                    <el-radio-button value="auto">Auto</el-radio-button>
                  </el-radio-group>
                </div>

                <div class="setting-item">
                  <div class="setting-info">
                    <h4>Accent Color</h4>
                    <p>Choose your primary accent color</p>
                  </div>
                  <div class="color-picker">
                    <button
                      v-for="color in accentColors"
                      :key="color.value"
                      class="color-option"
                      :class="{ active: appearanceSettings.accentColor === color.value }"
                      :style="{ background: color.hex }"
                      @click="appearanceSettings.accentColor = color.value"
                    >
                      <el-icon v-if="appearanceSettings.accentColor === color.value"><Check /></el-icon>
                    </button>
                  </div>
                </div>

                <div class="setting-item">
                  <div class="setting-info">
                    <h4>Compact Mode</h4>
                    <p>Reduce spacing for more content on screen</p>
                  </div>
                  <el-switch v-model="appearanceSettings.compactMode" size="large" />
                </div>

                <div class="setting-item">
                  <div class="setting-info">
                    <h4>Sidebar Position</h4>
                    <p>Choose sidebar layout preference</p>
                  </div>
                  <el-radio-group v-model="appearanceSettings.sidebarPosition" size="large">
                    <el-radio-button value="left">Left</el-radio-button>
                    <el-radio-button value="right">Right</el-radio-button>
                  </el-radio-group>
                </div>

                <div class="form-actions">
                  <button class="btn btn-outline">Reset to Default</button>
                  <button class="btn btn-primary" @click="saveAppearance">
                    <el-icon><Check /></el-icon>
                    <span>Apply Changes</span>
                  </button>
                </div>
              </div>
            </ModernCard>

            <!-- System Settings -->
            <ModernCard v-show="activeTab === 'system'" class="settings-card scroll-animate">
              <template #header>
                <div class="card-header">
                  <h3>System Configuration</h3>
                  <p class="card-subtitle">Configure system-wide settings and preferences</p>
                </div>
              </template>

              <div class="settings-section">
                <div class="setting-item">
                  <div class="setting-info">
                    <h4>Language</h4>
                    <p>Select your preferred language</p>
                  </div>
                  <el-select v-model="systemSettings.language" size="large" style="width: 200px">
                    <el-option label="English" value="en" />
                    <el-option label="Spanish" value="es" />
                    <el-option label="French" value="fr" />
                    <el-option label="German" value="de" />
                  </el-select>
                </div>

                <div class="setting-item">
                  <div class="setting-info">
                    <h4>Timezone</h4>
                    <p>Set your local timezone for accurate timestamps</p>
                  </div>
                  <el-select v-model="systemSettings.timezone" size="large" style="width: 250px">
                    <el-option label="UTC" value="UTC" />
                    <el-option label="America/New_York" value="America/New_York" />
                    <el-option label="Europe/London" value="Europe/London" />
                    <el-option label="Asia/Tokyo" value="Asia/Tokyo" />
                  </el-select>
                </div>

                <div class="setting-item">
                  <div class="setting-info">
                    <h4>Date Format</h4>
                    <p>Choose how dates are displayed</p>
                  </div>
                  <el-radio-group v-model="systemSettings.dateFormat" size="large">
                    <el-radio-button value="MM/DD/YYYY">MM/DD/YYYY</el-radio-button>
                    <el-radio-button value="DD/MM/YYYY">DD/MM/YYYY</el-radio-button>
                    <el-radio-button value="YYYY-MM-DD">YYYY-MM-DD</el-radio-button>
                  </el-radio-group>
                </div>

                <div class="setting-item">
                  <div class="setting-info">
                    <h4>Temperature Unit</h4>
                    <p>Select temperature measurement unit</p>
                  </div>
                  <el-radio-group v-model="systemSettings.temperatureUnit" size="large">
                    <el-radio-button value="celsius">Celsius (°C)</el-radio-button>
                    <el-radio-button value="fahrenheit">Fahrenheit (°F)</el-radio-button>
                  </el-radio-group>
                </div>

                <div class="setting-item">
                  <div class="setting-info">
                    <h4>Data Refresh Rate</h4>
                    <p>How often to refresh real-time data</p>
                  </div>
                  <el-select v-model="systemSettings.refreshRate" size="large" style="width: 180px">
                    <el-option label="5 seconds" :value="5" />
                    <el-option label="10 seconds" :value="10" />
                    <el-option label="30 seconds" :value="30" />
                    <el-option label="1 minute" :value="60" />
                  </el-select>
                </div>

                <div class="form-actions">
                  <button class="btn btn-outline">Reset to Default</button>
                  <button class="btn btn-primary" @click="saveSystem">
                    <el-icon><Check /></el-icon>
                    <span>Save Settings</span>
                  </button>
                </div>
              </div>
            </ModernCard>

            <!-- About Settings -->
            <ModernCard v-show="activeTab === 'about'" class="settings-card scroll-animate">
              <template #header>
                <div class="card-header">
                  <h3>About AquaControl</h3>
                  <p class="card-subtitle">System information and resources</p>
                </div>
              </template>

              <div class="settings-section">
                <div class="about-content">
                  <div class="app-logo">
                    <div class="logo-icon">
                      <el-icon :size="64"><DataAnalysis /></el-icon>
                    </div>
                    <h2>AquaControl Platform</h2>
                    <p class="version">Version 2.0.0</p>
                  </div>

                  <div class="info-grid">
                    <div class="info-item">
                      <span class="info-label">Build Number</span>
                      <span class="info-value">2024.11.18</span>
                    </div>
                    <div class="info-item">
                      <span class="info-label">License</span>
                      <span class="info-value">Enterprise</span>
                    </div>
                    <div class="info-item">
                      <span class="info-label">Last Updated</span>
                      <span class="info-value">November 18, 2025</span>
                    </div>
                    <div class="info-item">
                      <span class="info-label">System Status</span>
                      <el-tag type="success">Operational</el-tag>
                    </div>
                  </div>

                  <div class="about-actions">
                    <button class="btn btn-outline btn-block">
                      <el-icon><Document /></el-icon>
                      <span>View Documentation</span>
                    </button>
                    <button class="btn btn-outline btn-block">
                      <el-icon><Link /></el-icon>
                      <span>Visit Website</span>
                    </button>
                    <button class="btn btn-outline btn-block">
                      <el-icon><ChatDotRound /></el-icon>
                      <span>Contact Support</span>
                    </button>
                  </div>

                  <div class="copyright">
                    <p>© 2025 AquaControl Platform. All rights reserved.</p>
                    <p>Developed with ❤️ for sustainable aquaculture</p>
                  </div>
                </div>
              </div>
            </ModernCard>
          </div>
        </div>
      </div>
    </section>

    <!-- Change Password Modal -->
    <el-dialog
      v-model="showChangePassword"
      title="Change Password"
      width="500px"
    >
      <div class="password-form">
        <div class="form-group">
          <label>Current Password</label>
          <el-input
            v-model="passwordForm.current"
            type="password"
            size="large"
            show-password
          />
        </div>
        <div class="form-group">
          <label>New Password</label>
          <el-input
            v-model="passwordForm.new"
            type="password"
            size="large"
            show-password
          />
        </div>
        <div class="form-group">
          <label>Confirm New Password</label>
          <el-input
            v-model="passwordForm.confirm"
            type="password"
            size="large"
            show-password
          />
        </div>
      </div>
      <template #footer>
        <button class="btn btn-outline" @click="showChangePassword = false">Cancel</button>
        <button class="btn btn-primary" @click="changePassword">Change Password</button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import ModernCard from '@/components/common/ModernCard.vue'
import {
  User,
  Lock,
  Bell,
  Brush,
  Setting,
  InfoFilled,
  Upload,
  Check,
  Delete,
  DataAnalysis,
  Document,
  Link,
  ChatDotRound
} from '@element-plus/icons-vue'
import { ElMessage, ElMessageBox } from 'element-plus'

// State
const activeTab = ref('profile')
const showChangePassword = ref(false)

const settingsTabs = [
  { id: 'profile', label: 'Profile', icon: User },
  { id: 'account', label: 'Account', icon: Lock },
  { id: 'notifications', label: 'Notifications', icon: Bell },
  { id: 'appearance', label: 'Appearance', icon: Brush },
  { id: 'system', label: 'System', icon: Setting },
  { id: 'about', label: 'About', icon: InfoFilled }
]

const profileData = ref({
  name: 'John Doe',
  email: 'john.doe@aquacontrol.com',
  phone: '+1 (555) 123-4567',
  role: 'System Administrator',
  bio: 'Experienced aquaculture manager with 10+ years in the industry.',
  avatar: 'https://api.dicebear.com/7.x/avataaars/svg?seed=John'
})

const accountSettings = ref({
  twoFactorAuth: false,
  sessionTimeout: 30
})

const notificationSettings = ref({
  email: {
    alerts: true,
    maintenance: true,
    reports: false
  },
  push: {
    critical: true,
    sensors: false
  }
})

const appearanceSettings = ref({
  theme: 'light',
  accentColor: 'blue',
  compactMode: false,
  sidebarPosition: 'left'
})

const accentColors = [
  { value: 'blue', hex: '#3b82f6' },
  { value: 'green', hex: '#10b981' },
  { value: 'purple', hex: '#8b5cf6' },
  { value: 'red', hex: '#ef4444' },
  { value: 'orange', hex: '#f97316' },
  { value: 'teal', hex: '#14b8a6' }
]

const systemSettings = ref({
  language: 'en',
  timezone: 'UTC',
  dateFormat: 'MM/DD/YYYY',
  temperatureUnit: 'celsius',
  refreshRate: 10
})

const passwordForm = ref({
  current: '',
  new: '',
  confirm: ''
})

// Methods
const saveProfile = () => {
  ElMessage.success('Profile updated successfully')
}

const saveNotifications = () => {
  ElMessage.success('Notification preferences saved')
}

const saveAppearance = () => {
  ElMessage.success('Appearance settings applied')
}

const saveSystem = () => {
  ElMessage.success('System settings saved')
}

const changePassword = () => {
  if (passwordForm.value.new !== passwordForm.value.confirm) {
    ElMessage.error('Passwords do not match')
    return
  }
  ElMessage.success('Password changed successfully')
  showChangePassword.value = false
  passwordForm.value = { current: '', new: '', confirm: '' }
}

const confirmDeleteAccount = async () => {
  try {
    await ElMessageBox.confirm(
      'This action cannot be undone. All your data will be permanently deleted.',
      'Delete Account',
      {
        confirmButtonText: 'Delete My Account',
        cancelButtonText: 'Cancel',
        type: 'error',
      }
    )
    ElMessage.success('Account deletion initiated')
  } catch {
    // User cancelled
  }
}
</script>

<style lang="scss" scoped>
@import '@/styles/design-system/index.scss';

.modern-settings-view {
  min-height: 100vh;
  background: #f8f9fa;
}

.page-header {
  background: linear-gradient(135deg, #1e3a8a 0%, #3b82f6 100%);
  padding: var(--space-8) 0;

  .header-content {
    .header-text {
      max-width: 600px;

      h1 {
        color: white !important;
        margin-bottom: var(--space-3);
      }

      p {
        color: rgba(255, 255, 255, 0.9) !important;
      }
    }
  }
}

.settings-layout {
  display: grid;
  grid-template-columns: 280px 1fr;
  gap: var(--space-6);

  @media (max-width: 1024px) {
    grid-template-columns: 1fr;
  }
}

.settings-sidebar {
  @media (max-width: 1024px) {
    position: sticky;
    top: 80px;
    z-index: 10;
  }
}

.settings-nav {
  background: white;
  border-radius: var(--border-radius-lg);
  padding: var(--space-2);
  box-shadow: var(--shadow-sm);
  position: sticky;
  top: 100px;

  @media (max-width: 1024px) {
    display: flex;
    overflow-x: auto;
    position: static;
  }

  .nav-item {
    width: 100%;
    display: flex;
    align-items: center;
    gap: var(--space-3);
    padding: var(--space-4);
    background: transparent;
    border: none;
    border-radius: var(--border-radius-md);
    color: #6b7280;
    font-size: var(--font-size-base);
    cursor: pointer;
    transition: all var(--transition-fast);
    text-align: left;

    @media (max-width: 1024px) {
      white-space: nowrap;
      flex-shrink: 0;
    }

    &:hover {
      background: #f3f4f6;
      color: #111827;
    }

    &.active {
      background: linear-gradient(135deg, #eff6ff 0%, #dbeafe 100%);
      color: #1e40af;
      font-weight: var(--font-weight-semibold);
    }
  }
}

.settings-content {
  min-height: 500px;
}

.settings-card {
  .card-header {
    h3 {
      font-size: var(--font-size-xl);
      font-weight: var(--font-weight-semibold);
      color: #111827;
      margin: 0 0 var(--space-1);
    }

    .card-subtitle {
      font-size: var(--font-size-sm);
      color: #6b7280;
      margin: 0;
    }
  }
}

.settings-section {
  padding: var(--space-2) 0;
}

.profile-avatar {
  display: flex;
  align-items: center;
  gap: var(--space-6);
  padding: var(--space-6);
  background: var(--color-neutral-50);
  border-radius: var(--border-radius-lg);
  margin-bottom: var(--space-6);

  .avatar-preview {
    width: 120px;
    height: 120px;
    border-radius: 50%;
    overflow: hidden;
    border: 4px solid white;
    box-shadow: var(--shadow-md);

    img {
      width: 100%;
      height: 100%;
      object-fit: cover;
    }
  }

  .avatar-actions {
    display: flex;
    flex-direction: column;
    gap: var(--space-2);
  }
}

.form-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: var(--space-5);
  margin-bottom: var(--space-6);

  @media (max-width: 768px) {
    grid-template-columns: 1fr;
  }

  .full-width {
    grid-column: 1 / -1;
  }
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: var(--space-2);

  label {
    font-size: var(--font-size-sm);
    font-weight: var(--font-weight-medium);
    color: #374151;
  }
}

.form-actions {
  display: flex;
  justify-content: flex-end;
  gap: var(--space-3);
  padding-top: var(--space-6);
  border-top: 1px solid var(--color-neutral-200);
}

.setting-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-6);
  padding: var(--space-5);
  border-bottom: 1px solid var(--color-neutral-100);

  &:last-child {
    border-bottom: none;
  }

  &.danger-zone {
    background: var(--color-danger-50);
    border-radius: var(--border-radius-lg);
    border: 1px solid var(--color-danger-200);
    margin-top: var(--space-6);
  }

  .setting-info {
    flex: 1;

    h4 {
      font-size: var(--font-size-base);
      font-weight: var(--font-weight-semibold);
      color: #111827;
      margin: 0 0 var(--space-1);
    }

    p {
      font-size: var(--font-size-sm);
      color: #6b7280;
      margin: 0;
    }
  }
}

.notification-group {
  margin-bottom: var(--space-8);

  &:last-child {
    margin-bottom: var(--space-6);
  }

  h4 {
    font-size: var(--font-size-base);
    font-weight: var(--font-weight-semibold);
    color: #111827;
    margin: 0 0 var(--space-4);
  }
}

.notification-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: var(--space-4);
  border-bottom: 1px solid var(--color-neutral-100);

  &:last-child {
    border-bottom: none;
  }

  .notification-info {
    span {
      display: block;
      font-size: var(--font-size-base);
      font-weight: var(--font-weight-medium);
      color: #111827;
      margin-bottom: var(--space-1);
    }

    small {
      font-size: var(--font-size-sm);
      color: #6b7280;
    }
  }
}

.color-picker {
  display: flex;
  gap: var(--space-3);

  .color-option {
    width: 48px;
    height: 48px;
    border-radius: var(--border-radius-lg);
    border: 3px solid transparent;
    display: flex;
    align-items: center;
    justify-content: center;
    cursor: pointer;
    transition: all var(--transition-fast);
    color: white;

    &:hover {
      transform: scale(1.1);
    }

    &.active {
      border-color: var(--color-text-primary);
      box-shadow: var(--shadow-lg);
    }
  }
}

.about-content {
  text-align: center;

  .app-logo {
    padding: var(--space-10) 0;

    .logo-icon {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 120px;
      height: 120px;
      background: linear-gradient(135deg, var(--color-primary-500), var(--color-secondary-500));
      border-radius: var(--border-radius-xl);
      color: white;
      margin-bottom: var(--space-6);
    }

    h2 {
      font-size: var(--font-size-3xl);
      font-weight: var(--font-weight-bold);
      font-family: var(--font-family-display);
      color: #111827;
      margin: 0 0 var(--space-2);
    }

    .version {
      font-size: var(--font-size-base);
      color: #6b7280;
    }
  }

  .info-grid {
    display: grid;
    grid-template-columns: repeat(2, 1fr);
    gap: var(--space-4);
    max-width: 500px;
    margin: var(--space-8) auto;

    .info-item {
      text-align: left;
      padding: var(--space-4);
      background: var(--color-neutral-50);
      border-radius: var(--border-radius-md);

      .info-label {
        display: block;
        font-size: var(--font-size-xs);
        color: #9ca3af;
        margin-bottom: var(--space-1);
        text-transform: uppercase;
        letter-spacing: 0.05em;
      }

      .info-value {
        display: block;
        font-size: var(--font-size-base);
        font-weight: var(--font-weight-semibold);
        color: #111827;
      }
    }
  }

  .about-actions {
    display: flex;
    flex-direction: column;
    gap: var(--space-3);
    max-width: 400px;
    margin: var(--space-8) auto;
  }

  .copyright {
    margin-top: var(--space-10);
    padding-top: var(--space-6);
    border-top: 1px solid var(--color-neutral-200);

    p {
      font-size: var(--font-size-sm);
      color: #6b7280;
      margin: var(--space-2) 0;
    }
  }
}

.password-form {
  display: flex;
  flex-direction: column;
  gap: var(--space-5);
  padding: var(--space-4) 0;
}
</style>
