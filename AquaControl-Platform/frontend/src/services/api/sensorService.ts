import { httpClient } from './httpClient'
import type { ApiResponse, PaginatedResponse } from '@/types/api'
import type { Sensor, SensorReading } from '@/types/domain'

export interface CreateSensorCommand {
  sensorType: string
  model: string
  serialNumber: string
  tankId: string
  minValue: number
  maxValue: number
  unit: string
  calibrationDate?: Date
  installationDate?: Date
  manufacturer?: string
  description?: string
}

export interface UpdateSensorCommand {
  model?: string
  minValue?: number
  maxValue?: number
  unit?: string
  description?: string
}

export interface CalibrateSensorCommand {
  calibrationDate: Date
  calibratedBy: string
  calibrationValue: number
  notes?: string
}

export interface GetSensorsQuery {
  tankId?: string
  sensorType?: string
  status?: string
  isActive?: boolean
  searchTerm?: string
  page?: number
  pageSize?: number
  sortBy?: string
  sortDescending?: boolean
}

export interface GetSensorReadingsQuery {
  sensorId: string
  startDate?: Date
  endDate?: Date
  page?: number
  pageSize?: number
}

class SensorService {
  private readonly baseUrl = '/api/sensors'

  /**
   * Get all sensors with optional filtering and pagination
   */
  async getSensors(query?: GetSensorsQuery): Promise<PaginatedResponse<Sensor>> {
    const response = await httpClient.get<PaginatedResponse<Sensor>>(this.baseUrl, {
      params: query
    })
    return response.data
  }

  /**
   * Get a single sensor by ID
   */
  async getSensorById(id: string): Promise<ApiResponse<Sensor>> {
    const response = await httpClient.get<ApiResponse<Sensor>>(`${this.baseUrl}/${id}`)
    return response.data
  }

  /**
   * Get sensors for a specific tank
   */
  async getSensorsByTankId(tankId: string): Promise<ApiResponse<Sensor[]>> {
    const response = await httpClient.get<ApiResponse<Sensor[]>>(`${this.baseUrl}/tank/${tankId}`)
    return response.data
  }

  /**
   * Create a new sensor
   */
  async createSensor(command: CreateSensorCommand): Promise<ApiResponse<Sensor>> {
    const response = await httpClient.post<ApiResponse<Sensor>>(this.baseUrl, command)
    return response.data
  }

  /**
   * Update an existing sensor
   */
  async updateSensor(id: string, command: UpdateSensorCommand): Promise<ApiResponse<Sensor>> {
    const response = await httpClient.put<ApiResponse<Sensor>>(`${this.baseUrl}/${id}`, command)
    return response.data
  }

  /**
   * Delete a sensor
   */
  async deleteSensor(id: string): Promise<ApiResponse<void>> {
    const response = await httpClient.delete<ApiResponse<void>>(`${this.baseUrl}/${id}`)
    return response.data
  }

  /**
   * Calibrate a sensor
   */
  async calibrateSensor(id: string, command: CalibrateSensorCommand): Promise<ApiResponse<Sensor>> {
    const response = await httpClient.post<ApiResponse<Sensor>>(
      `${this.baseUrl}/${id}/calibrate`,
      command
    )
    return response.data
  }

  /**
   * Activate a sensor
   */
  async activateSensor(id: string): Promise<ApiResponse<Sensor>> {
    const response = await httpClient.post<ApiResponse<Sensor>>(`${this.baseUrl}/${id}/activate`)
    return response.data
  }

  /**
   * Deactivate a sensor
   */
  async deactivateSensor(id: string): Promise<ApiResponse<Sensor>> {
    const response = await httpClient.post<ApiResponse<Sensor>>(`${this.baseUrl}/${id}/deactivate`)
    return response.data
  }

  /**
   * Get sensor readings with optional date range
   */
  async getSensorReadings(query: GetSensorReadingsQuery): Promise<PaginatedResponse<SensorReading>> {
    const response = await httpClient.get<PaginatedResponse<SensorReading>>(
      `${this.baseUrl}/${query.sensorId}/readings`,
      {
        params: {
          startDate: query.startDate?.toISOString(),
          endDate: query.endDate?.toISOString(),
          page: query.page,
          pageSize: query.pageSize
        }
      }
    )
    return response.data
  }

  /**
   * Get latest reading for a sensor
   */
  async getLatestReading(sensorId: string): Promise<ApiResponse<SensorReading>> {
    const response = await httpClient.get<ApiResponse<SensorReading>>(
      `${this.baseUrl}/${sensorId}/readings/latest`
    )
    return response.data
  }

  /**
   * Get sensor statistics
   */
  async getSensorStatistics(sensorId: string, days: number = 7): Promise<ApiResponse<any>> {
    const response = await httpClient.get<ApiResponse<any>>(
      `${this.baseUrl}/${sensorId}/statistics`,
      {
        params: { days }
      }
    )
    return response.data
  }

  /**
   * Test sensor connection
   */
  async testSensor(id: string): Promise<ApiResponse<{ isConnected: boolean; message: string }>> {
    const response = await httpClient.post<ApiResponse<{ isConnected: boolean; message: string }>>(
      `${this.baseUrl}/${id}/test`
    )
    return response.data
  }

  /**
   * Export sensor data
   */
  async exportSensorData(
    sensorId: string,
    startDate: Date,
    endDate: Date,
    format: 'csv' | 'json' | 'excel' = 'csv'
  ): Promise<Blob> {
    const response = await httpClient.get(
      `${this.baseUrl}/${sensorId}/export`,
      {
        params: {
          startDate: startDate.toISOString(),
          endDate: endDate.toISOString(),
          format
        },
        responseType: 'blob'
      }
    )
    return response.data
  }
}

export const sensorService = new SensorService()

