import { defineStore } from 'pinia'
import { ref, computed, watch } from 'vue'
import { tankService } from '@services/api/tankService'
import { useNotificationStore } from './notificationStore'
import { useRealTimeStore } from './realTimeStore'
import type { 
  Tank, 
  CreateTankCommand, 
  UpdateTankCommand, 
  GetTanksQuery 
} from '@types/domain'
import type { OptimisticUpdate, StoreState } from '@types/api'

export const useTankStore = defineStore('tanks', () => {
  // State
  const tanks = ref<Tank[]>([])
  const selectedTank = ref<Tank | null>(null)
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
  const filters = ref<GetTanksQuery>({
    searchTerm: '',
    tankType: undefined,
    isActive: undefined,
    sortBy: 'name',
    sortDescending: false
  })

  // Optimistic updates tracking
  const optimisticUpdates = ref<Map<string, OptimisticUpdate<Tank>>>(new Map())

  // Dependencies
  const notificationStore = useNotificationStore()
  const realTimeStore = useRealTimeStore()

  // Getters
  const activeTanks = computed(() => 
    tanks.value.filter(tank => tank.status === 'Active')
  )

  const inactiveTanks = computed(() => 
    tanks.value.filter(tank => tank.status === 'Inactive')
  )

  const maintenanceDueTanks = computed(() => 
    tanks.value.filter(tank => 
      tank.nextMaintenanceDate && 
      new Date(tank.nextMaintenanceDate) <= new Date()
    )
  )

  const tanksByType = computed(() => {
    const grouped = new Map<string, Tank[]>()
    tanks.value.forEach(tank => {
      const type = tank.tankType
      if (!grouped.has(type)) {
        grouped.set(type, [])
      }
      grouped.get(type)!.push(tank)
    })
    return grouped
  })

  const filteredTanks = computed(() => {
    let filtered = tanks.value

    if (filters.value.searchTerm) {
      const searchLower = filters.value.searchTerm.toLowerCase()
      filtered = filtered.filter(tank =>
        tank.name.toLowerCase().includes(searchLower) ||
        tank.location.building.toLowerCase().includes(searchLower) ||
        tank.location.room.toLowerCase().includes(searchLower)
      )
    }

    if (filters.value.tankType) {
      filtered = filtered.filter(tank => tank.tankType === filters.value.tankType)
    }

    if (filters.value.isActive !== undefined) {
      const targetStatus = filters.value.isActive ? 'Active' : 'Inactive'
      filtered = filtered.filter(tank => tank.status === targetStatus)
    }

    return filtered
  })

  const isLoading = computed(() => state.value.loading)
  const hasError = computed(() => !!state.value.error)
  const isEmpty = computed(() => tanks.value.length === 0 && !isLoading.value)

  // Actions
  const fetchTanks = async (query: GetTanksQuery = {}) => {
    state.value.loading = true
    state.value.error = null

    try {
      const mergedQuery = { ...filters.value, ...query }
      const response = await tankService.getTanks(mergedQuery)

      tanks.value = response.data
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
        title: 'Failed to load tanks',
        message: error.message
      })
    } finally {
      state.value.loading = false
    }
  }

  const fetchTankById = async (id: string): Promise<Tank | null> => {
    // Check if tank is already in store
    const existingTank = tanks.value.find(t => t.id === id)
    if (existingTank) {
      selectedTank.value = existingTank
      return existingTank
    }

    state.value.loading = true
    state.value.error = null

    try {
      const response = await tankService.getTankById(id)
      const tank = response.data

      // Add to tanks array if not exists
      const index = tanks.value.findIndex(t => t.id === id)
      if (index === -1) {
        tanks.value.push(tank)
      } else {
        tanks.value[index] = tank
      }

      selectedTank.value = tank
      return tank

    } catch (error: any) {
      state.value.error = error.message
      notificationStore.addNotification({
        type: 'error',
        title: 'Failed to load tank',
        message: error.message
      })
      return null
    } finally {
      state.value.loading = false
    }
  }

  const createTank = async (command: CreateTankCommand): Promise<Tank | null> => {
    // Optimistic update
    const tempId = `temp-${Date.now()}`
    const optimisticTank: Tank = {
      id: tempId,
      name: command.name,
      capacity: {
        value: command.capacity,
        unit: command.capacityUnit as any
      },
      location: {
        building: command.building,
        room: command.room,
        zone: command.zone,
        latitude: command.latitude,
        longitude: command.longitude,
        fullAddress: `${command.building}, ${command.room}${command.zone ? `, ${command.zone}` : ''}`
      },
      tankType: command.tankType,
      status: 'Inactive',
      sensors: [],
      createdAt: new Date().toISOString(),
      version: 1
    }

    // Add optimistic update
    tanks.value.unshift(optimisticTank)
    
    const rollback = () => {
      const index = tanks.value.findIndex(t => t.id === tempId)
      if (index !== -1) {
        tanks.value.splice(index, 1)
      }
    }

    optimisticUpdates.value.set(tempId, {
      id: tempId,
      type: 'create',
      data: optimisticTank,
      timestamp: new Date().toISOString(),
      rollback
    })

    try {
      const response = await tankService.createTank(command)
      const createdTank = response.data

      // Replace optimistic tank with real tank
      const index = tanks.value.findIndex(t => t.id === tempId)
      if (index !== -1) {
        tanks.value[index] = createdTank
      }

      // Remove optimistic update
      optimisticUpdates.value.delete(tempId)

      notificationStore.addNotification({
        type: 'success',
        title: 'Tank created',
        message: `Tank "${createdTank.name}" has been created successfully`
      })

      return createdTank

    } catch (error: any) {
      // Rollback optimistic update
      rollback()
      optimisticUpdates.value.delete(tempId)

      state.value.error = error.message
      notificationStore.addNotification({
        type: 'error',
        title: 'Failed to create tank',
        message: error.message
      })
      return null
    }
  }

  const updateTank = async (id: string, command: UpdateTankCommand): Promise<Tank | null> => {
    const existingTank = tanks.value.find(t => t.id === id)
    if (!existingTank) return null

    // Optimistic update
    const optimisticTank: Tank = {
      ...existingTank,
      name: command.name,
      capacity: {
        value: command.capacity,
        unit: command.capacityUnit as any
      },
      location: {
        building: command.building,
        room: command.room,
        zone: command.zone,
        latitude: command.latitude,
        longitude: command.longitude,
        fullAddress: `${command.building}, ${command.room}${command.zone ? `, ${command.zone}` : ''}`
      },
      tankType: command.tankType,
      updatedAt: new Date().toISOString(),
      version: existingTank.version + 1
    }

    const index = tanks.value.findIndex(t => t.id === id)
    const originalTank = tanks.value[index]
    tanks.value[index] = optimisticTank

    const rollback = () => {
      tanks.value[index] = originalTank
    }

    optimisticUpdates.value.set(id, {
      id,
      type: 'update',
      data: optimisticTank,
      timestamp: new Date().toISOString(),
      rollback
    })

    try {
      const response = await tankService.updateTank(id, command)
      const updatedTank = response.data

      // Replace optimistic tank with real tank
      tanks.value[index] = updatedTank

      // Update selected tank if it's the same
      if (selectedTank.value?.id === id) {
        selectedTank.value = updatedTank
      }

      // Remove optimistic update
      optimisticUpdates.value.delete(id)

      notificationStore.addNotification({
        type: 'success',
        title: 'Tank updated',
        message: `Tank "${updatedTank.name}" has been updated successfully`
      })

      return updatedTank

    } catch (error: any) {
      // Rollback optimistic update
      rollback()
      optimisticUpdates.value.delete(id)

      state.value.error = error.message
      notificationStore.addNotification({
        type: 'error',
        title: 'Failed to update tank',
        message: error.message
      })
      return null
    }
  }

  const deleteTank = async (id: string): Promise<boolean> => {
    const index = tanks.value.findIndex(t => t.id === id)
    if (index === -1) return false

    const tankToDelete = tanks.value[index]
    
    // Optimistic update - remove from array
    tanks.value.splice(index, 1)

    const rollback = () => {
      tanks.value.splice(index, 0, tankToDelete)
    }

    optimisticUpdates.value.set(id, {
      id,
      type: 'delete',
      data: tankToDelete,
      timestamp: new Date().toISOString(),
      rollback
    })

    try {
      await tankService.deleteTank(id)

      // Clear selected tank if it's the deleted one
      if (selectedTank.value?.id === id) {
        selectedTank.value = null
      }

      // Remove optimistic update
      optimisticUpdates.value.delete(id)

      notificationStore.addNotification({
        type: 'success',
        title: 'Tank deleted',
        message: `Tank "${tankToDelete.name}" has been deleted successfully`
      })

      return true

    } catch (error: any) {
      // Rollback optimistic update
      rollback()
      optimisticUpdates.value.delete(id)

      state.value.error = error.message
      notificationStore.addNotification({
        type: 'error',
        title: 'Failed to delete tank',
        message: error.message
      })
      return false
    }
  }

  // Real-time updates
  const handleRealTimeUpdate = (event: any) => {
    switch (event.eventType) {
      case 'TankCreated':
        // Add new tank if not already present
        if (!tanks.value.find(t => t.id === event.data.id)) {
          tanks.value.push(event.data)
        }
        break

      case 'TankUpdated':
        // Update existing tank
        const updateIndex = tanks.value.findIndex(t => t.id === event.data.id)
        if (updateIndex !== -1) {
          tanks.value[updateIndex] = event.data
          
          // Update selected tank if it's the same
          if (selectedTank.value?.id === event.data.id) {
            selectedTank.value = event.data
          }
        }
        break

      case 'TankDeleted':
        // Remove tank
        const deleteIndex = tanks.value.findIndex(t => t.id === event.data.id)
        if (deleteIndex !== -1) {
          tanks.value.splice(deleteIndex, 1)
          
          // Clear selected tank if it's the deleted one
          if (selectedTank.value?.id === event.data.id) {
            selectedTank.value = null
          }
        }
        break
    }
  }

  // Utility actions
  const clearError = () => {
    state.value.error = null
  }

  const setSelectedTank = (tank: Tank | null) => {
    selectedTank.value = tank
  }

  const updateFilters = (newFilters: Partial<GetTanksQuery>) => {
    filters.value = { ...filters.value, ...newFilters }
  }

  const resetFilters = () => {
    filters.value = {
      searchTerm: '',
      tankType: undefined,
      isActive: undefined,
      sortBy: 'name',
      sortDescending: false
    }
  }

  const refresh = () => {
    return fetchTanks(filters.value)
  }

  // Setup real-time subscriptions
  watch(() => realTimeStore.isConnected, (isConnected) => {
    if (isConnected) {
      realTimeStore.subscribe('TankUpdates', handleRealTimeUpdate)
    }
  }, { immediate: true })

  return {
    // State
    tanks,
    selectedTank,
    state,
    pagination,
    filters,
    optimisticUpdates,

    // Getters
    activeTanks,
    inactiveTanks,
    maintenanceDueTanks,
    tanksByType,
    filteredTanks,
    isLoading,
    hasError,
    isEmpty,

    // Actions
    fetchTanks,
    fetchTankById,
    createTank,
    updateTank,
    deleteTank,
    clearError,
    setSelectedTank,
    updateFilters,
    resetFilters,
    refresh
  }
}, {
  persist: {
    key: 'aquacontrol-tanks',
    storage: localStorage,
    paths: ['filters', 'pagination.pageSize']
  }
})

