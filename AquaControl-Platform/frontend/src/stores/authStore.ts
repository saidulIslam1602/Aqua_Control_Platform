import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { authService } from '@/services/api/authService'
import { useNotificationStore } from './notificationStore'
import type { LoginCredentials, User } from '@/types/auth'

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(null)
  const refreshTokenValue = ref<string | null>(null)
  const user = ref<User | null>(null)
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  const notificationStore = useNotificationStore()

  // Computed properties
  const isAuthenticated = computed(() => !!token.value && !!user.value)
  const isTokenExpired = computed(() => {
    if (!token.value) return true
    
    try {
      const payload = JSON.parse(atob(token.value.split('.')[1]))
      const currentTime = Date.now() / 1000
      return payload.exp < currentTime
    } catch {
      return true
    }
  })

  // Actions
  const login = async (credentials: LoginCredentials): Promise<boolean> => {
    try {
      isLoading.value = true
      error.value = null

      console.log('🔐 Attempting login with credentials:', { username: credentials.username })
      
      const response = await authService.login(credentials)
      
      console.log('✅ Login API response received:', {
        hasAccessToken: !!response.accessToken,
        hasRefreshToken: !!response.refreshToken,
        hasUser: !!response.user,
        userFirstName: response.user?.firstName
      })
      
      token.value = response.accessToken
      refreshTokenValue.value = response.refreshToken
      user.value = response.user

      console.log('✅ Auth state updated:', {
        tokenSet: !!token.value,
        userSet: !!user.value,
        isAuthenticated: isAuthenticated.value
      })

      notificationStore.addNotification({
        type: 'success',
        title: 'Login Successful',
        message: `Welcome back, ${response.user.firstName}!`
      })

      return true
    } catch (err: any) {
      console.error('❌ Login failed:', err)
      console.error('❌ Error details:', {
        status: err.response?.status,
        statusText: err.response?.statusText,
        data: err.response?.data,
        message: err.message
      })
      
      error.value = err.response?.data?.message || err.message || 'Login failed'
      notificationStore.addNotification({
        type: 'error',
        title: 'Login Failed',
        message: error.value || 'Login failed'
      })
      return false
    } finally {
      isLoading.value = false
    }
  }

  const logout = async (): Promise<void> => {
    try {
      if (token.value) {
        await authService.logout()
      }
    } catch (err) {
      console.error('Logout error:', err)
    } finally {
      // Clear state regardless of API call success
      token.value = null
      refreshTokenValue.value = null
      user.value = null
      error.value = null

      notificationStore.addNotification({
        type: 'info',
        title: 'Logged Out',
        message: 'You have been logged out successfully'
      })
    }
  }

  const refreshToken = async (): Promise<boolean> => {
    if (!refreshTokenValue.value) {
      await logout()
      return false
    }

    try {
      const response = await authService.refreshToken(refreshTokenValue.value)
      
      token.value = response.accessToken
      refreshTokenValue.value = response.refreshToken
      
      return true
    } catch (err) {
      console.error('Token refresh failed:', err)
      await logout()
      return false
    }
  }

  const validateAndRefreshToken = async (): Promise<boolean> => {
    if (!token.value) return false

    // Check if token is expired
    if (isTokenExpired.value) {
      return await refreshToken()
    }

    // Validate token with server
    try {
      const isValid = await authService.validateToken(token.value)
      if (!isValid) {
        return await refreshToken()
      }
      return true
    } catch {
      return await refreshToken()
    }
  }

  const initialize = async (): Promise<void> => {
    if (token.value && user.value) {
      // Validate existing token
      const isValid = await validateAndRefreshToken()
      if (!isValid) {
        // Token refresh failed, user needs to login again
        await logout()
      }
    }
  }

  const updateUserProfile = async (): Promise<void> => {
    if (!token.value) return

    try {
      const userProfile = await authService.getUserProfile(token.value)
      user.value = userProfile
    } catch (err) {
      console.error('Failed to update user profile:', err)
    }
  }

  // Auto-refresh token before expiry
  const setupTokenRefresh = () => {
    if (!token.value) return

    try {
      const payload = JSON.parse(atob(token.value.split('.')[1]))
      const expiryTime = payload.exp * 1000
      const currentTime = Date.now()
      const timeUntilExpiry = expiryTime - currentTime
      
      // Refresh token 5 minutes before expiry
      const refreshTime = Math.max(timeUntilExpiry - 5 * 60 * 1000, 0)
      
      setTimeout(async () => {
        await refreshToken()
        setupTokenRefresh() // Setup next refresh
      }, refreshTime)
    } catch {
      // Invalid token format
      logout()
    }
  }

  return {
    // State
    token,
    refreshTokenValue,
    user,
    isLoading,
    error,
    
    // Computed
    isAuthenticated,
    isTokenExpired,
    
    // Actions
    login,
    logout,
    refreshTokenAction: refreshToken,
    validateAndRefreshToken,
    initialize,
    updateUserProfile,
    setupTokenRefresh
  }
}, {
  persist: {
    key: 'aquacontrol-auth',
    storage: localStorage,
    paths: ['token', 'refreshTokenValue', 'user']
  }
})

