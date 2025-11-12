import { httpClient } from './httpClient'
import type { LoginCredentials, LoginResponse, TokenResponse, RefreshTokenRequest } from '@/types/auth'

export const authService = {
  async login(credentials: LoginCredentials): Promise<LoginResponse> {
    try {
      console.log('🌐 Making API call to /api/auth/login')
      const response = await httpClient.postDirect<LoginResponse>('/api/auth/login', credentials)
      console.log('🌐 API response received:', {
        hasAccessToken: !!response.accessToken,
        hasRefreshToken: !!response.refreshToken,
        hasUser: !!response.user,
        userFirstName: response.user?.firstName
      })
      return response
    } catch (error) {
      console.error('🌐 API call failed:', error)
      throw error
    }
  },

  async refreshToken(refreshToken: string): Promise<TokenResponse> {
    try {
      const response = await httpClient.postDirect<TokenResponse>('/api/auth/refresh', { 
        refreshToken 
      } as RefreshTokenRequest)
      return response
    } catch (error) {
      console.error('Token refresh failed:', error)
      throw error
    }
  },

  async logout(): Promise<void> {
    try {
      await httpClient.post('/api/auth/logout')
    } catch (error) {
      console.error('Logout failed:', error)
      // Don't throw error for logout - clear local state anyway
    }
  },

  async validateToken(token: string): Promise<boolean> {
    try {
      await httpClient.getDirect('/api/auth/validate', {
        headers: {
          Authorization: `Bearer ${token}`
        }
      })
      return true
    } catch (error) {
      console.error('Token validation failed:', error)
      return false
    }
  },

  async getUserProfile(token: string): Promise<any> {
    try {
      const response = await httpClient.getDirect('/api/auth/profile', {
        headers: {
          Authorization: `Bearer ${token}`
        }
      })
      return response
    } catch (error) {
      console.error('Get user profile failed:', error)
      throw error
    }
  }
}
