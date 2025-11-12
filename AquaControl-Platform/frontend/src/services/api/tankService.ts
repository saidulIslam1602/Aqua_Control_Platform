import { httpClient } from './httpClient'
import type { 
  Tank, 
  CreateTankCommand, 
  UpdateTankCommand, 
  GetTanksQuery 
} from '@types/domain'
import type { ApiResponse, PagedResponse } from '@types/api'

export class TankService {
  private readonly baseUrl = '/api/tanks'

  async getTanks(query: GetTanksQuery = {}): Promise<PagedResponse<Tank>> {
    const params = new URLSearchParams()
    
    if (query.page) params.append('page', query.page.toString())
    if (query.pageSize) params.append('pageSize', query.pageSize.toString())
    if (query.searchTerm) params.append('searchTerm', query.searchTerm)
    if (query.tankType) params.append('tankType', query.tankType)
    if (query.isActive !== undefined) params.append('isActive', query.isActive.toString())
    if (query.sortBy) params.append('sortBy', query.sortBy)
    if (query.sortDescending) params.append('sortDescending', query.sortDescending.toString())

    const url = `${this.baseUrl}?${params.toString()}`
    return httpClient.get<Tank[]>(url) as Promise<PagedResponse<Tank>>
  }

  async getTankById(id: string): Promise<ApiResponse<Tank>> {
    return httpClient.get<Tank>(`${this.baseUrl}/${id}`)
  }

  async createTank(command: CreateTankCommand): Promise<ApiResponse<Tank>> {
    return httpClient.post<Tank>(this.baseUrl, command)
  }

  async updateTank(id: string, command: UpdateTankCommand): Promise<ApiResponse<Tank>> {
    return httpClient.put<Tank>(`${this.baseUrl}/${id}`, command)
  }

  async deleteTank(id: string): Promise<ApiResponse<void>> {
    return httpClient.delete<void>(`${this.baseUrl}/${id}`)
  }

  async activateTank(id: string): Promise<ApiResponse<void>> {
    return httpClient.post<void>(`${this.baseUrl}/${id}/activate`)
  }

  async deactivateTank(id: string, reason: string): Promise<ApiResponse<void>> {
    return httpClient.post<void>(`${this.baseUrl}/${id}/deactivate`, { reason })
  }

  async scheduleMaintenance(id: string, maintenanceDate: string): Promise<ApiResponse<void>> {
    return httpClient.post<void>(`${this.baseUrl}/${id}/schedule-maintenance`, { maintenanceDate })
  }

  async completeMaintenance(id: string, completionDate: string, notes: string): Promise<ApiResponse<void>> {
    return httpClient.post<void>(`${this.baseUrl}/${id}/complete-maintenance`, { 
      completionDate, 
      notes 
    })
  }

  // Bulk operations
  async bulkActivate(tankIds: string[]): Promise<ApiResponse<void>> {
    return httpClient.post<void>(`${this.baseUrl}/bulk/activate`, { tankIds })
  }

  async bulkDeactivate(tankIds: string[], reason: string): Promise<ApiResponse<void>> {
    return httpClient.post<void>(`${this.baseUrl}/bulk/deactivate`, { tankIds, reason })
  }

  async bulkDelete(tankIds: string[]): Promise<ApiResponse<void>> {
    return httpClient.post<void>(`${this.baseUrl}/bulk/delete`, { tankIds })
  }

  // Analytics
  async getTankAnalytics(id: string, timeRange: string): Promise<ApiResponse<any>> {
    return httpClient.get<any>(`${this.baseUrl}/${id}/analytics?timeRange=${timeRange}`)
  }

  async getTankHealth(id: string): Promise<ApiResponse<any>> {
    return httpClient.get<any>(`${this.baseUrl}/${id}/health`)
  }

  // Export/Import
  async exportTanks(format: 'csv' | 'excel' | 'pdf' = 'csv'): Promise<void> {
    await httpClient.downloadFile(`${this.baseUrl}/export?format=${format}`, `tanks.${format}`)
  }

  async importTanks(file: File): Promise<ApiResponse<any>> {
    return httpClient.uploadFile(`${this.baseUrl}/import`, file)
  }
}

// Export singleton instance
export const tankService = new TankService()
export default tankService

