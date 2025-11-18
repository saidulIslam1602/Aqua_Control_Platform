import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { sensorService } from '@services/api/sensorService'
import type { 
  CreateSensorCommand, 
  UpdateSensorCommand, 
  CalibrateSensorCommand,
  GetSensorsQuery 
} from '@services/api/sensorService'
import { useNotificationStore } from './notificationStore'
import type { Sensor, SensorReading } from '@/types/domain'
import type { OptimisticUpdate, StoreState } from '@/types/api'

export const useSensorStore = defineStore('sensors', () => {
  // State
  const sensors = ref<Sensor[]>([])
  const selectedSensor = ref<Sensor | null>(null)
  const sensorReadings = ref<Map<string, SensorReading[]>>(new Map())
  const state = ref<StoreState>({
    loading: false,
    error: null,
    lastUpdated: null
  })

  // Pagination state
  const pagination = ref({
    page: 1,
    pageSize: 20,
    totalCount: 0,
    totalPages: 0,
    hasNextPage: false,
    hasPreviousPage: false
  })

  // Filters state
  const filters = ref<GetSensorsQuery>({
    searchTerm: '',
    sensorType: undefined,
    status: undefined,
    tankId: undefined,
    isActive: undefined,
    sortBy: 'sensorType',
    sortDescending: false
  })

  // Optimistic updates tracking
  const optimisticUpdates = ref<Map<string, OptimisticUpdate<Sensor>>>(new Map())

  // Dependencies
  const notificationStore = useNotificationStore()

  // Getters
  const activeSensors = computed(() => 
    sensors.value.filter(sensor => sensor.isActive && sensor.status === 'Online')
  )

  const offlineSensors = computed(() => 
    sensors.value.filter(sensor => sensor.status === 'Offline')
  )

  const calibrationDueSensors = computed(() => 
    sensors.value.filter(sensor => sensor.isCalibrationDue)
  )

  const sensorsByType = computed(() => {
    const grouped = new Map<string, Sensor[]>()
    sensors.value.forEach(sensor => {
      const type = sensor.sensorType
      if (!grouped.has(type)) {
        grouped.set(type, [])
      }
      grouped.get(type)!.push(sensor)
    })
    return grouped
  })

  const sensorsByTank = computed(() => {
    const grouped = new Map<string, Sensor[]>()
    sensors.value.forEach(sensor => {
      const tankId = sensor.tankId
      if (!grouped.has(tankId)) {
        grouped.set(tankId, [])
      }
      grouped.get(tankId)!.push(sensor)
    })
    return grouped
  })

  const filteredSensors = computed(() => {
    let filtered = sensors.value

    if (filters.value.searchTerm) {
      const searchLower = filters.value.searchTerm.toLowerCase()
      filtered = filtered.filter(sensor =>
        sensor.sensorType.toLowerCase().includes(searchLower) ||
        sensor.model.toLowerCase().includes(searchLower) ||
        sensor.serialNumber.toLowerCase().includes(searchLower)
      )
    }

    if (filters.value.sensorType) {
      filtered = filtered.filter(sensor => sensor.sensorType === filters.value.sensorType)
    }

    if (filters.value.status) {
      filtered = filtered.filter(sensor => sensor.status === filters.value.status)
    }

    if (filters.value.tankId) {
      filtered = filtered.filter(sensor => sensor.tankId === filters.value.tankId)
    }

    if (filters.value.isActive !== undefined) {
      filtered = filtered.filter(sensor => sensor.isActive === filters.value.isActive)
    }

    return filtered
  })

  const isLoading = computed(() => state.value.loading)
  const hasError = computed(() => !!state.value.error)
  const isEmpty = computed(() => sensors.value.length === 0 && !isLoading.value)

  // Actions
  const fetchSensors = async (query: GetSensorsQuery = {}) => {
    state.value.loading = true
    state.value.error = null

    try {
      const mergedQuery = { ...filters.value, ...query }
      const response = await sensorService.getSensors(mergedQuery)

      sensors.value = response.data
      pagination.value = response.pagination
      state.value.lastUpdated = new Date().toISOString()

      // Update filters if provided
      if (Object.keys(query).length > 0) {
        filters.value = { ...filters.value, ...query }
      }

    } catch (error: any) {
      state.value.error = error.message
      notificationStore.addNotification({
        type: 'error',
        title: 'Failed to load sensors',
        message: error.message
      })
    } finally {
      state.value.loading = false
    }
  }

  const fetchSensorById = async (id: string): Promise<Sensor | null> => {
    // Check if sensor is already in store
    const existingSensor = sensors.value.find(s => s.id === id)
    if (existingSensor) {
      selectedSensor.value = existingSensor
      return existingSensor
    }

    state.value.loading = true
    state.value.error = null

    try {
      const response = await sensorService.getSensorById(id)
      const sensor = response.data

      // Add to sensors array if not exists
      const index = sensors.value.findIndex(s => s.id === id)
      if (index === -1) {
        sensors.value.push(sensor)
      } else {
        sensors.value[index] = sensor
      }

      selectedSensor.value = sensor
      return sensor

    } catch (error: any) {
      state.value.error = error.message
      notificationStore.addNotification({
        type: 'error',
        title: 'Failed to load sensor',
        message: error.message
      })
      return null
    } finally {
      state.value.loading = false
    }
  }

  const fetchSensorsByTankId = async (tankId: string): Promise<Sensor[]> => {
    state.value.loading = true
    state.value.error = null

    try {
      const response = await sensorService.getSensorsByTankId(tankId)
      const tankSensors = response.data

      // Update sensors in store
      tankSensors.forEach(sensor => {
        const index = sensors.value.findIndex(s => s.id === sensor.id)
        if (index === -1) {
          sensors.value.push(sensor)
        } else {
          sensors.value[index] = sensor
        }
      })

      return tankSensors

    } catch (error: any) {
      state.value.error = error.message
      notificationStore.addNotification({
        type: 'error',
        title: 'Failed to load tank sensors',
        message: error.message
      })
      return []
    } finally {
      state.value.loading = false
    }
  }

  const createSensor = async (command: CreateSensorCommand): Promise<Sensor | null> => {
    // Optimistic update
    const tempId = `temp-${Date.now()}`
    const optimisticSensor: Sensor = {
      id: tempId,
      tankId: command.tankId,
      sensorType: command.sensorType as any,
      model: command.model,
      manufacturer: command.manufacturer || 'Unknown',
      serialNumber: command.serialNumber,
      status: 'Online' as any,
      isActive: true,
      minValue: command.minValue,
      maxValue: command.maxValue,
      accuracy: 0.95,
      installationDate: command.installationDate?.toISOString() || new Date().toISOString(),
      calibrationDate: command.calibrationDate?.toISOString(),
      isCalibrationDue: false
    }

    // Add optimistic update
    sensors.value.unshift(optimisticSensor)
    
    const rollback = () => {
      const index = sensors.value.findIndex(s => s.id === tempId)
      if (index !== -1) {
        sensors.value.splice(index, 1)
      }
    }

    optimisticUpdates.value.set(tempId, {
      id: tempId,
      type: 'create',
      data: optimisticSensor,
      timestamp: new Date().toISOString(),
      rollback
    })

    try {
      const response = await sensorService.createSensor(command)
      const createdSensor = response.data

      // Replace optimistic sensor with real sensor
      const index = sensors.value.findIndex(s => s.id === tempId)
      if (index !== -1) {
        sensors.value[index] = createdSensor
      }

      // Remove optimistic update
      optimisticUpdates.value.delete(tempId)

      notificationStore.addNotification({
        type: 'success',
        title: 'Sensor created',
        message: `Sensor "${createdSensor.model}" has been created successfully`
      })

      return createdSensor

    } catch (error: any) {
      // Rollback optimistic update
      rollback()
      optimisticUpdates.value.delete(tempId)

      state.value.error = error.message
      notificationStore.addNotification({
        type: 'error',
        title: 'Failed to create sensor',
        message: error.message
      })
      return null
    }
  }

  const updateSensor = async (id: string, command: UpdateSensorCommand): Promise<Sensor | null> => {
    const existingSensor = sensors.value.find(s => s.id === id)
    if (!existingSensor) return null

    // Optimistic update
    const optimisticSensor: Sensor = {
      ...existingSensor,
      model: command.model || existingSensor.model,
      minValue: command.minValue ?? existingSensor.minValue,
      maxValue: command.maxValue ?? existingSensor.maxValue
    }

    const index = sensors.value.findIndex(s => s.id === id)
    const originalSensor = sensors.value[index]
    sensors.value[index] = optimisticSensor

    const rollback = () => {
      sensors.value[index] = originalSensor
    }

    optimisticUpdates.value.set(id, {
      id,
      type: 'update',
      data: optimisticSensor,
      timestamp: new Date().toISOString(),
      rollback
    })

    try {
      const response = await sensorService.updateSensor(id, command)
      const updatedSensor = response.data

      // Replace optimistic sensor with real sensor
      sensors.value[index] = updatedSensor

      // Update selected sensor if it's the same
      if (selectedSensor.value?.id === id) {
        selectedSensor.value = updatedSensor
      }

      // Remove optimistic update
      optimisticUpdates.value.delete(id)

      notificationStore.addNotification({
        type: 'success',
        title: 'Sensor updated',
        message: `Sensor "${updatedSensor.model}" has been updated successfully`
      })

      return updatedSensor

    } catch (error: any) {
      // Rollback optimistic update
      rollback()
      optimisticUpdates.value.delete(id)

      state.value.error = error.message
      notificationStore.addNotification({
        type: 'error',
        title: 'Failed to update sensor',
        message: error.message
      })
      return null
    }
  }

  const deleteSensor = async (id: string): Promise<boolean> => {
    const index = sensors.value.findIndex(s => s.id === id)
    if (index === -1) return false

    const sensorToDelete = sensors.value[index]
    
    // Optimistic update - remove from array
    sensors.value.splice(index, 1)

    const rollback = () => {
      sensors.value.splice(index, 0, sensorToDelete)
    }

    optimisticUpdates.value.set(id, {
      id,
      type: 'delete',
      data: sensorToDelete,
      timestamp: new Date().toISOString(),
      rollback
    })

    try {
      await sensorService.deleteSensor(id)

      // Clear selected sensor if it's the deleted one
      if (selectedSensor.value?.id === id) {
        selectedSensor.value = null
      }

      // Remove optimistic update
      optimisticUpdates.value.delete(id)

      notificationStore.addNotification({
        type: 'success',
        title: 'Sensor deleted',
        message: `Sensor "${sensorToDelete.model}" has been deleted successfully`
      })

      return true

    } catch (error: any) {
      // Rollback optimistic update
      rollback()
      optimisticUpdates.value.delete(id)

      state.value.error = error.message
      notificationStore.addNotification({
        type: 'error',
        title: 'Failed to delete sensor',
        message: error.message
      })
      return false
    }
  }

  const calibrateSensor = async (id: string, command: CalibrateSensorCommand): Promise<Sensor | null> => {
    try {
      const response = await sensorService.calibrateSensor(id, command)
      const calibratedSensor = response.data

      // Update sensor in store
      const index = sensors.value.findIndex(s => s.id === id)
      if (index !== -1) {
        sensors.value[index] = calibratedSensor
      }

      // Update selected sensor if it's the same
      if (selectedSensor.value?.id === id) {
        selectedSensor.value = calibratedSensor
      }

      notificationStore.addNotification({
        type: 'success',
        title: 'Sensor calibrated',
        message: `Sensor "${calibratedSensor.model}" has been calibrated successfully`
      })

      return calibratedSensor

    } catch (error: any) {
      state.value.error = error.message
      notificationStore.addNotification({
        type: 'error',
        title: 'Failed to calibrate sensor',
        message: error.message
      })
      return null
    }
  }

  const activateSensor = async (id: string): Promise<boolean> => {
    try {
      const response = await sensorService.activateSensor(id)
      const activatedSensor = response.data

      // Update sensor in store
      const index = sensors.value.findIndex(s => s.id === id)
      if (index !== -1) {
        sensors.value[index] = activatedSensor
      }

      notificationStore.addNotification({
        type: 'success',
        title: 'Sensor activated',
        message: `Sensor "${activatedSensor.model}" is now active`
      })

      return true

    } catch (error: any) {
      state.value.error = error.message
      notificationStore.addNotification({
        type: 'error',
        title: 'Failed to activate sensor',
        message: error.message
      })
      return false
    }
  }

  const deactivateSensor = async (id: string): Promise<boolean> => {
    try {
      const response = await sensorService.deactivateSensor(id)
      const deactivatedSensor = response.data

      // Update sensor in store
      const index = sensors.value.findIndex(s => s.id === id)
      if (index !== -1) {
        sensors.value[index] = deactivatedSensor
      }

      notificationStore.addNotification({
        type: 'success',
        title: 'Sensor deactivated',
        message: `Sensor "${deactivatedSensor.model}" is now inactive`
      })

      return true

    } catch (error: any) {
      state.value.error = error.message
      notificationStore.addNotification({
        type: 'error',
        title: 'Failed to deactivate sensor',
        message: error.message
      })
      return false
    }
  }

  const fetchSensorReadings = async (sensorId: string, startDate?: Date, endDate?: Date): Promise<SensorReading[]> => {
    try {
      const response = await sensorService.getSensorReadings({
        sensorId,
        startDate,
        endDate,
        page: 1,
        pageSize: 100
      })

      const readings = response.data
      sensorReadings.value.set(sensorId, readings)

      return readings

    } catch (error: any) {
      state.value.error = error.message
      notificationStore.addNotification({
        type: 'error',
        title: 'Failed to load sensor readings',
        message: error.message
      })
      return []
    }
  }

  const getLatestReading = async (sensorId: string): Promise<SensorReading | null> => {
    try {
      const response = await sensorService.getLatestReading(sensorId)
      return response.data

    } catch (error: any) {
      state.value.error = error.message
      return null
    }
  }

  const testSensor = async (id: string): Promise<boolean> => {
    try {
      const response = await sensorService.testSensor(id)
      const result = response.data

      notificationStore.addNotification({
        type: result.isConnected ? 'success' : 'warning',
        title: result.isConnected ? 'Sensor connected' : 'Sensor not responding',
        message: result.message
      })

      return result.isConnected

    } catch (error: any) {
      state.value.error = error.message
      notificationStore.addNotification({
        type: 'error',
        title: 'Failed to test sensor',
        message: error.message
      })
      return false
    }
  }

  const exportSensorData = async (
    sensorId: string,
    startDate: Date,
    endDate: Date,
    format: 'csv' | 'json' | 'excel' = 'csv'
  ): Promise<boolean> => {
    try {
      const blob = await sensorService.exportSensorData(sensorId, startDate, endDate, format)
      
      // Create download link
      const url = window.URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = url
      link.download = `sensor-${sensorId}-data.${format}`
      document.body.appendChild(link)
      link.click()
      document.body.removeChild(link)
      window.URL.revokeObjectURL(url)

      notificationStore.addNotification({
        type: 'success',
        title: 'Data exported',
        message: `Sensor data exported successfully as ${format.toUpperCase()}`
      })

      return true

    } catch (error: any) {
      state.value.error = error.message
      notificationStore.addNotification({
        type: 'error',
        title: 'Failed to export data',
        message: error.message
      })
      return false
    }
  }

  // Utility actions
  const clearError = () => {
    state.value.error = null
  }

  const setSelectedSensor = (sensor: Sensor | null) => {
    selectedSensor.value = sensor
  }

  const updateFilters = (newFilters: Partial<GetSensorsQuery>) => {
    filters.value = { ...filters.value, ...newFilters }
  }

  const resetFilters = () => {
    filters.value = {
      searchTerm: '',
      sensorType: undefined,
      status: undefined,
      tankId: undefined,
      isActive: undefined,
      sortBy: 'sensorType',
      sortDescending: false
    }
  }

  const refresh = () => {
    return fetchSensors(filters.value)
  }

  return {
    // State
    sensors,
    selectedSensor,
    sensorReadings,
    state,
    pagination,
    filters,
    optimisticUpdates,

    // Getters
    activeSensors,
    offlineSensors,
    calibrationDueSensors,
    sensorsByType,
    sensorsByTank,
    filteredSensors,
    isLoading,
    hasError,
    isEmpty,

    // Actions
    fetchSensors,
    fetchSensorById,
    fetchSensorsByTankId,
    createSensor,
    updateSensor,
    deleteSensor,
    calibrateSensor,
    activateSensor,
    deactivateSensor,
    fetchSensorReadings,
    getLatestReading,
    testSensor,
    exportSensorData,
    clearError,
    setSelectedSensor,
    updateFilters,
    resetFilters,
    refresh
  }
}, {
  persist: {
    key: 'aquacontrol-sensors',
    storage: localStorage,
    paths: ['filters', 'pagination.pageSize']
  }
})

