import { defineStore } from 'pinia'
import { ref } from 'vue'

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(null)
  const user = ref<any>(null)
  const isAuthenticated = ref(false)

  const login = async (credentials: { username: string; password: string }) => {
    // TODO: Implement login logic
    token.value = 'mock-token'
    isAuthenticated.value = true
  }

  const logout = async () => {
    token.value = null
    user.value = null
    isAuthenticated.value = false
  }

  const refreshToken = async () => {
    // TODO: Implement token refresh logic
    throw new Error('Token refresh not implemented')
  }

  return {
    token,
    user,
    isAuthenticated,
    login,
    logout,
    refreshToken
  }
}, {
  persist: {
    key: 'aquacontrol-auth',
    storage: localStorage,
    paths: ['token', 'user', 'isAuthenticated']
  }
})

