import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useTankStore } from '@/stores/tankStore'
import type { Tank } from '@/types/domain'

// Mock the tank service
vi.mock('@/services/api/tankService', () => ({
  tankService: {
    getTanks: vi.fn(),
    getTankById: vi.fn(),
    createTank: vi.fn(),
    updateTank: vi.fn(),
    deleteTank: vi.fn(),
  }
}))

// Mock notification store
vi.mock('@/stores/notificationStore', () => ({
  useNotificationStore: () => ({
    addNotification: vi.fn()
  })
}))

// Mock real-time store
vi.mock('@/stores/realTimeStore', () => ({
  useRealTimeStore: () => ({
    isConnected: false,
    subscribe: vi.fn(),
    unsubscribe: vi.fn()
  })
}))

describe('Tank Store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
  })

  it('should initialize with empty state', () => {
    const store = useTankStore()
    
    expect(store.tanks).toEqual([])
    expect(store.selectedTank).toBeNull()
    expect(store.isLoading).toBe(false)
    expect(store.hasError).toBe(false)
  })

  it('should compute active tanks correctly', () => {
    const store = useTankStore()
    
    // Add mock tanks
    store.tanks = [
      createMockTank('1', 'Active'),
      createMockTank('2', 'Inactive'),
      createMockTank('3', 'Active'),
    ]

    expect(store.activeTanks).toHaveLength(2)
    expect(store.activeTanks.every(tank => tank.status === 'Active')).toBe(true)
  })

  it('should compute inactive tanks correctly', () => {
    const store = useTankStore()
    
    store.tanks = [
      createMockTank('1', 'Active'),
      createMockTank('2', 'Inactive'),
      createMockTank('3', 'Inactive'),
    ]

    expect(store.inactiveTanks).toHaveLength(2)
    expect(store.inactiveTanks.every(tank => tank.status === 'Inactive')).toBe(true)
  })

  it('should filter tanks by search term', () => {
    const store = useTankStore()
    
    store.tanks = [
      createMockTank('1', 'Active', 'Salmon Tank'),
      createMockTank('2', 'Active', 'Trout Tank'),
      createMockTank('3', 'Active', 'Breeding Tank'),
    ]

    store.filters.searchTerm = 'salmon'

    expect(store.filteredTanks).toHaveLength(1)
    expect(store.filteredTanks[0].name).toBe('Salmon Tank')
  })

  it('should handle optimistic updates correctly', async () => {
    const store = useTankStore()
    const { tankService } = await import('@/services/api/tankService')
    
    // Mock successful creation
    vi.mocked(tankService.createTank).mockResolvedValue({
      data: createMockTank('new-id', 'Inactive', 'New Tank'),
      success: true
    })

    const command = {
      name: 'New Tank',
      capacity: 1000,
      capacityUnit: 'L',
      building: 'Building A',
      room: 'Room 1',
      zone: null,
      latitude: null,
      longitude: null,
      tankType: 'Freshwater' as any
    }

    const result = await store.createTank(command)

    expect(result).not.toBeNull()
    expect(store.tanks).toHaveLength(1)
    expect(store.tanks[0].name).toBe('New Tank')
  })

  it('should rollback optimistic updates on failure', async () => {
    const store = useTankStore()
    const { tankService } = await import('@/services/api/tankService')
    
    // Mock failed creation
    vi.mocked(tankService.createTank).mockRejectedValue(new Error('Creation failed'))

    const command = {
      name: 'Failed Tank',
      capacity: 1000,
      capacityUnit: 'L',
      building: 'Building A',
      room: 'Room 1',
      zone: null,
      latitude: null,
      longitude: null,
      tankType: 'Freshwater' as any
    }

    const result = await store.createTank(command)

    expect(result).toBeNull()
    expect(store.tanks).toHaveLength(0) // Should be rolled back
  })

  it('should update filters correctly', () => {
    const store = useTankStore()
    
    const newFilters = {
      searchTerm: 'test',
      tankType: 'Saltwater' as any,
      isActive: true
    }

    store.updateFilters(newFilters)

    expect(store.filters.searchTerm).toBe('test')
    expect(store.filters.tankType).toBe('Saltwater')
    expect(store.filters.isActive).toBe(true)
  })

  it('should reset filters to default values', () => {
    const store = useTankStore()
    
    // Set some filters
    store.filters = {
      searchTerm: 'test',
      tankType: 'Saltwater' as any,
      isActive: true,
      sortBy: 'createdAt',
      sortDescending: true
    }

    store.resetFilters()

    expect(store.filters.searchTerm).toBe('')
    expect(store.filters.tankType).toBeUndefined()
    expect(store.filters.isActive).toBeUndefined()
    expect(store.filters.sortBy).toBe('name')
    expect(store.filters.sortDescending).toBe(false)
  })
})

// Helper function to create mock tanks
function createMockTank(id: string, status: string, name?: string): Tank {
  return {
    id,
    name: name || `Tank ${id}`,
    capacity: { value: 1000, unit: 'L' },
    location: {
      building: 'Building A',
      room: 'Room 1',
      zone: null,
      latitude: null,
      longitude: null,
      fullAddress: 'Building A, Room 1'
    },
    tankType: 'Freshwater',
    status,
    isActive: status === 'Active',
    sensors: [],
    activeSensorCount: 0,
    sensorCount: 0,
    createdAt: new Date().toISOString(),
    updatedAt: null,
    lastMaintenanceDate: null,
    nextMaintenanceDate: null,
    isMaintenanceDue: false,
    version: 1
  } as Tank
}

