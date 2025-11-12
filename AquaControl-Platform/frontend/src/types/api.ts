// Base API Types
export interface ApiResponse<T = any> {
  data: T
  success: boolean
  message?: string
  errors?: Record<string, string[]>
  metadata?: {
    timestamp: string
    requestId: string
    version: string
  }
}

export interface PagedResponse<T> extends ApiResponse<T[]> {
  pagination: {
    page: number
    pageSize: number
    totalCount: number
    totalPages: number
    hasNextPage: boolean
    hasPreviousPage: boolean
  }
}

export interface ApiError {
  code: string
  message: string
  details?: Record<string, any>
  timestamp: string
  path: string
}

// Command/Query Types
export interface Command {
  type: string
  payload: Record<string, any>
  metadata?: {
    correlationId: string
    causationId?: string
    userId?: string
    timestamp: string
  }
}

export interface Query {
  type: string
  parameters: Record<string, any>
  pagination?: {
    page: number
    pageSize: number
    sortBy?: string
    sortDirection?: 'asc' | 'desc'
  }
  filters?: Record<string, any>
}

// Real-time Types
export interface SignalRConnection {
  connectionId: string
  state: 'Disconnected' | 'Connecting' | 'Connected' | 'Disconnecting' | 'Reconnecting'
  groups: string[]
}

export interface RealTimeEvent<T = any> {
  eventType: string
  data: T
  timestamp: string
  source: string
  correlationId?: string
}

// State Management Types
export interface StoreState {
  loading: boolean
  error: string | null
  lastUpdated: string | null
}

export interface OptimisticUpdate<T> {
  id: string
  type: 'create' | 'update' | 'delete'
  data: T
  timestamp: string
  rollback: () => void
}

