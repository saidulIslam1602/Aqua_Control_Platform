<template>
  <header class="modern-header" :class="{ 'is-scrolled': isScrolled, 'menu-open': isMobileMenuOpen }">
    <div class="modern-header__container">
      <!-- Logo -->
      <router-link to="/" class="modern-header__logo">
        <div class="logo-icon">
          <svg viewBox="0 0 40 40" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M20 5L35 15V25L20 35L5 25V15L20 5Z" fill="url(#logo-gradient)"/>
            <path d="M20 12L28 17V23L20 28L12 23V17L20 12Z" fill="white" opacity="0.3"/>
            <defs>
              <linearGradient id="logo-gradient" x1="5" y1="5" x2="35" y2="35">
                <stop offset="0%" stop-color="var(--color-primary-500)"/>
                <stop offset="100%" stop-color="var(--color-secondary-500)"/>
              </linearGradient>
            </defs>
          </svg>
        </div>
        <span class="logo-text">
          <span class="logo-text-main">AquaControl</span>
          <span class="logo-text-sub">Platform</span>
        </span>
      </router-link>

      <!-- Desktop Navigation -->
      <nav class="modern-header__nav desktop-nav">
        <a 
          v-for="item in navigationItems" 
          :key="item.path"
          :href="item.path"
          class="nav-link"
          :class="{ 'is-active': isActiveRoute(item.path) }"
          @click.prevent="navigateTo(item.path)"
        >
          {{ item.label }}
        </a>
      </nav>

      <!-- Actions -->
      <div class="modern-header__actions">
        <!-- Search -->
        <button class="action-button" @click="openSearch" title="Search">
          <el-icon><Search /></el-icon>
        </button>

        <!-- Notifications -->
        <button class="action-button" @click="toggleNotifications" title="Notifications">
          <el-icon><Bell /></el-icon>
          <span v-if="notificationCount > 0" class="notification-badge">{{ notificationCount }}</span>
        </button>

        <!-- User Menu -->
        <el-dropdown trigger="click" @command="handleUserAction">
          <div class="user-avatar">
            <img v-if="userAvatar" :src="userAvatar" :alt="userName" />
            <span v-else class="avatar-initials">{{ userInitials }}</span>
          </div>
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

        <!-- Mobile Menu Toggle -->
        <button class="mobile-menu-toggle" @click="toggleMobileMenu">
          <span class="menu-icon">
            <span></span>
            <span></span>
            <span></span>
          </span>
        </button>
      </div>
    </div>

    <!-- Mobile Menu -->
    <transition name="mobile-menu">
      <div v-if="isMobileMenuOpen" class="mobile-menu">
        <nav class="mobile-nav">
          <a 
            v-for="item in navigationItems" 
            :key="item.path"
            :href="item.path"
            class="mobile-nav-link"
            :class="{ 'is-active': isActiveRoute(item.path) }"
            @click.prevent="navigateToMobile(item.path)"
          >
            <el-icon v-if="item.icon">
              <component :is="item.icon" />
            </el-icon>
            {{ item.label }}
          </a>
        </nav>
      </div>
    </transition>
  </header>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'
import { useNotificationStore } from '@/stores/notificationStore'
import { 
  Search, 
  Bell, 
  User, 
  Setting, 
  SwitchButton,
  Monitor,
  DataAnalysis,
  Setting as SettingIcon,
  Grid
} from '@element-plus/icons-vue'

// Stores
const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()
const notificationStore = useNotificationStore()

// State
const isScrolled = ref(false)
const isMobileMenuOpen = ref(false)

// Navigation Items
const navigationItems = [
  { path: '/dashboard', label: 'Dashboard', icon: Monitor },
  { path: '/tanks', label: 'Tanks', icon: Grid },
  { path: '/sensors', label: 'Sensors', icon: Monitor },
  { path: '/analytics', label: 'Analytics', icon: DataAnalysis },
  { path: '/settings', label: 'Settings', icon: SettingIcon }
]

// Computed
const userName = computed(() => authStore.user?.firstName || 'User')
const userAvatar = computed(() => authStore.user?.avatar || '')
const userInitials = computed(() => {
  const name = userName.value
  return name.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2)
})
const notificationCount = computed(() => notificationStore.unreadCount)

// Methods
const handleScroll = () => {
  isScrolled.value = window.scrollY > 20
}

const isActiveRoute = (path: string) => {
  return route.path === path || route.path.startsWith(path + '/')
}

const navigateTo = (path: string) => {
  router.push(path)
}

const navigateToMobile = (path: string) => {
  router.push(path)
  isMobileMenuOpen.value = false
}

const toggleMobileMenu = () => {
  isMobileMenuOpen.value = !isMobileMenuOpen.value
  if (isMobileMenuOpen.value) {
    document.body.style.overflow = 'hidden'
  } else {
    document.body.style.overflow = ''
  }
}

const openSearch = () => {
  // Implement search functionality
  console.log('Open search')
}

const toggleNotifications = () => {
  // Implement notifications panel
  console.log('Toggle notifications')
}

const handleUserAction = (command: string) => {
  switch (command) {
    case 'profile':
      router.push('/profile')
      break
    case 'settings':
      router.push('/settings')
      break
    case 'logout':
      authStore.logout()
      break
  }
}

// Lifecycle
onMounted(() => {
  window.addEventListener('scroll', handleScroll)
  handleScroll()
})

onUnmounted(() => {
  window.removeEventListener('scroll', handleScroll)
  document.body.style.overflow = ''
})
</script>

<style lang="scss" scoped>
@import '@/styles/design-system/tokens.scss';

.modern-header {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  height: var(--header-height);
  background-color: var(--header-bg);
  backdrop-filter: blur(10px);
  border-bottom: 1px solid transparent;
  transition: all var(--transition-base);
  z-index: var(--z-sticky);

  &.is-scrolled {
    background-color: rgba(255, 255, 255, 0.95);
    box-shadow: var(--header-shadow);
    border-bottom-color: var(--color-neutral-200);
  }

  &__container {
    max-width: var(--container-max-width);
    margin: 0 auto;
    padding: 0 var(--container-padding);
    height: 100%;
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: var(--space-8);
  }

  &__logo {
    display: flex;
    align-items: center;
    gap: var(--space-3);
    text-decoration: none;
    transition: opacity var(--transition-fast);

    &:hover {
      opacity: 0.8;
    }

    .logo-icon {
      width: 40px;
      height: 40px;
      
      svg {
        width: 100%;
        height: 100%;
      }
    }

    .logo-text {
      display: flex;
      flex-direction: column;
      line-height: 1.2;

      &-main {
        font-size: var(--font-size-lg);
        font-weight: var(--font-weight-bold);
        color: var(--color-text-primary);
        font-family: var(--font-family-display);
      }

      &-sub {
        font-size: var(--font-size-xs);
        font-weight: var(--font-weight-medium);
        color: var(--color-text-tertiary);
        text-transform: uppercase;
        letter-spacing: 0.05em;
      }
    }
  }

  &__nav {
    flex: 1;
    display: flex;
    align-items: center;
    gap: var(--space-2);

    @media (max-width: 1024px) {
      display: none;
    }
  }

  &__actions {
    display: flex;
    align-items: center;
    gap: var(--space-4);
  }
}

.nav-link {
  position: relative;
  padding: var(--space-3) var(--space-4);
  color: var(--color-text-secondary);
  text-decoration: none;
  font-weight: var(--font-weight-medium);
  font-size: var(--font-size-sm);
  border-radius: var(--border-radius-lg);
  transition: all var(--transition-fast);

  &::before {
    content: '';
    position: absolute;
    bottom: 0;
    left: 50%;
    transform: translateX(-50%);
    width: 0;
    height: 2px;
    background: linear-gradient(90deg, var(--color-primary-500), var(--color-secondary-500));
    transition: width var(--transition-base);
  }

  &:hover {
    color: var(--color-primary-600);
    background-color: var(--color-neutral-50);
  }

  &.is-active {
    color: var(--color-primary-600);
    font-weight: var(--font-weight-semibold);

    &::before {
      width: 60%;
    }
  }
}

.action-button {
  position: relative;
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  border: none;
  background-color: var(--color-neutral-50);
  color: var(--color-text-secondary);
  border-radius: var(--border-radius-full);
  cursor: pointer;
  transition: all var(--transition-fast);

  &:hover {
    background-color: var(--color-neutral-100);
    color: var(--color-primary-600);
    transform: translateY(-2px);
  }

  .notification-badge {
    position: absolute;
    top: -4px;
    right: -4px;
    min-width: 20px;
    height: 20px;
    padding: 0 6px;
    display: flex;
    align-items: center;
    justify-content: center;
    background: linear-gradient(135deg, var(--color-error), var(--color-error-dark));
    color: white;
    font-size: var(--font-size-xs);
    font-weight: var(--font-weight-bold);
    border-radius: var(--border-radius-full);
    box-shadow: 0 2px 8px rgba(239, 68, 68, 0.4);
  }
}

.user-avatar {
  width: 40px;
  height: 40px;
  border-radius: var(--border-radius-full);
  overflow: hidden;
  cursor: pointer;
  border: 2px solid var(--color-neutral-200);
  transition: all var(--transition-fast);

  &:hover {
    border-color: var(--color-primary-500);
    transform: scale(1.05);
  }

  img {
    width: 100%;
    height: 100%;
    object-fit: cover;
  }

  .avatar-initials {
    width: 100%;
    height: 100%;
    display: flex;
    align-items: center;
    justify-content: center;
    background: linear-gradient(135deg, var(--color-primary-500), var(--color-secondary-500));
    color: white;
    font-weight: var(--font-weight-bold);
    font-size: var(--font-size-sm);
  }
}

.mobile-menu-toggle {
  display: none;
  width: 40px;
  height: 40px;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  border: none;
  background: none;
  cursor: pointer;
  padding: 0;

  @media (max-width: 1024px) {
    display: flex;
  }

  .menu-icon {
    width: 24px;
    height: 18px;
    position: relative;
    transform: rotate(0deg);
    transition: var(--transition-base);

    span {
      display: block;
      position: absolute;
      height: 2px;
      width: 100%;
      background: var(--color-text-primary);
      border-radius: 2px;
      opacity: 1;
      left: 0;
      transform: rotate(0deg);
      transition: var(--transition-base);

      &:nth-child(1) {
        top: 0;
      }

      &:nth-child(2) {
        top: 8px;
      }

      &:nth-child(3) {
        top: 16px;
      }
    }
  }

  .modern-header.menu-open & .menu-icon {
    span:nth-child(1) {
      top: 8px;
      transform: rotate(135deg);
    }

    span:nth-child(2) {
      opacity: 0;
      left: -60px;
    }

    span:nth-child(3) {
      top: 8px;
      transform: rotate(-135deg);
    }
  }
}

// Mobile Menu
.mobile-menu {
  position: fixed;
  top: var(--header-height);
  left: 0;
  right: 0;
  bottom: 0;
  background-color: var(--color-bg-primary);
  padding: var(--space-6);
  overflow-y: auto;
  z-index: calc(var(--z-sticky) - 1);

  @media (min-width: 1025px) {
    display: none;
  }
}

.mobile-nav {
  display: flex;
  flex-direction: column;
  gap: var(--space-2);
}

.mobile-nav-link {
  display: flex;
  align-items: center;
  gap: var(--space-3);
  padding: var(--space-4);
  color: var(--color-text-secondary);
  text-decoration: none;
  font-weight: var(--font-weight-medium);
  font-size: var(--font-size-base);
  border-radius: var(--border-radius-lg);
  transition: all var(--transition-fast);

  &:hover {
    background-color: var(--color-neutral-50);
    color: var(--color-primary-600);
  }

  &.is-active {
    background-color: var(--color-primary-50);
    color: var(--color-primary-600);
    font-weight: var(--font-weight-semibold);
  }
}

// Transitions
.mobile-menu-enter-active,
.mobile-menu-leave-active {
  transition: all var(--transition-base);
}

.mobile-menu-enter-from {
  opacity: 0;
  transform: translateY(-20px);
}

.mobile-menu-leave-to {
  opacity: 0;
  transform: translateY(-20px);
}
</style>
