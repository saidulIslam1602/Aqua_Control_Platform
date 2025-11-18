import { httpClient } from './httpClient'
import type { Alert } from '@/types/domain'
import type { ApiResponse, PagedResponse } from '@/types/api'

export interface GetAlertsQuery {
  page?: number
  pageSize?: number
  severity?: string
  tankId?: string
  isResolved?: boolean
  sortBy?: string
  sortDescending?: boolean
}

export interface ResolveAlertCommand {
  resolvedBy: string
  notes?: string
}

export class AlertService {
  private readonly baseUrl = '/api/alerts'

  async getAlerts(query: GetAlertsQuery = {}): Promise<PagedResponse<Alert>> {
    const params = new URLSearchParams()
    if (query.page) params.append('page', query.page.toString())
    if (query.pageSize) params.append('pageSize', query.pageSize.toString())
    if (query.severity) params.append('severity', query.severity)
    if (query.tankId) params.append('tankId', query.tankId)
    if (query.isResolved !== undefined) params.append('isResolved', query.isResolved.toString())
    if (query.sortBy) params.append('sortBy', query.sortBy)
    if (query.sortDescending) params.append('sortDescending', query.sortDescending.toString())

    const url = `${this.baseUrl}?${params.toString()}`
    return httpClient.get<Alert[]>(url) as Promise<PagedResponse<Alert>>
  }

  async getAlertById(id: string): Promise<ApiResponse<Alert>> {
    return httpClient.get<Alert>(`${this.baseUrl}/${id}`)
  }

  async getActiveAlerts(): Promise<ApiResponse<Alert[]>> {
    return httpClient.get<Alert[]>(`${this.baseUrl}/active`)
  }

  async getAlertsByTank(tankId: string): Promise<ApiResponse<Alert[]>> {
    return httpClient.get<Alert[]>(`${this.baseUrl}/tank/${tankId}`)
  }

  async getAlertsBySeverity(severity: string): Promise<ApiResponse<Alert[]>> {
    return httpClient.get<Alert[]>(`${this.baseUrl}/severity/${severity}`)
  }

  async resolveAlert(id: string, command: ResolveAlertCommand): Promise<ApiResponse<Alert>> {
    return httpClient.post<Alert>(`${this.baseUrl}/${id}/resolve`, command)
  }

  async acknowledgeAlert(id: string): Promise<ApiResponse<Alert>> {
    return httpClient.post<Alert>(`${this.baseUrl}/${id}/acknowledge`)
  }

  async deleteAlert(id: string): Promise<ApiResponse<void>> {
    return httpClient.delete<void>(`${this.baseUrl}/${id}`)
  }

  async getAlertStatistics(timeRange?: string): Promise<ApiResponse<any>> {
    const params = timeRange ? `?timeRange=${timeRange}` : ''
    return httpClient.get<any>(`${this.baseUrl}/statistics${params}`)
  }

  async exportAlerts(format: 'csv' | 'excel' | 'pdf' = 'csv'): Promise<void> {
    await httpClient.downloadFile(`${this.baseUrl}/export?format=${format}`, `alerts.${format}`)
  }
}

export const alertService = new AlertService()
export default alertService

