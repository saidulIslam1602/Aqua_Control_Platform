import { httpClient } from './httpClient'
import type { LoginCredentials, LoginResponse, TokenResponse, RefreshTokenRequest } from '@/types/auth'

export const authService = {
  async login(credentials: LoginCredentials): Promise<LoginResponse> {
    try {
      const response = await httpClient.post<LoginResponse>('/auth/login', credentials)
      return response.data
    } catch (error) {
      console.error('Login failed:', error)
      throw error
    }
  },

  async refreshToken(refreshToken: string): Promise<TokenResponse> {
    try {
      const response = await httpClient.post<TokenResponse>('/auth/refresh', { 
        refreshToken 
      } as RefreshTokenRequest)
      return response.data
    } catch (error) {
      console.error('Token refresh failed:', error)
      throw error
    }
  },

  async logout(): Promise<void> {
    try {
      await httpClient.post('/auth/logout')
    } catch (error) {
      console.error('Logout failed:', error)
      // Don't throw error for logout - clear local state anyway
    }
  },

  async validateToken(token: string): Promise<boolean> {
    try {
      await httpClient.get('/auth/validate', {
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
      const response = await httpClient.get('/auth/profile', {
        headers: {
          Authorization: `Bearer ${token}`
        }
      })
      return response.data
    } catch (error) {
      console.error('Get user profile failed:', error)
      throw error
    }
  }
}
