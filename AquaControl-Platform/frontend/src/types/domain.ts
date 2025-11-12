// Tank Domain Types
export interface Tank {
  id: string
  name: string
  capacity: TankCapacity
  location: Location
  tankType: TankType
  status: TankStatus
  optimalParameters?: WaterQualityParameters
  sensors: Sensor[]
  createdAt: string
  updatedAt?: string
  lastMaintenanceDate?: string
  nextMaintenanceDate?: string
  version: number
  
  // Computed properties for UI
  readonly isActive: boolean
  readonly isMaintenanceDue: boolean
  readonly sensorCount: number
  readonly activeSensorCount: number
}

export interface TankCapacity {
  value: number
  unit: 'L' | 'ML' | 'GAL'
}

export interface Location {
  building: string
  room: string
  zone?: string
  latitude?: number
  longitude?: number
  fullAddress: string
}

export interface WaterQualityParameters {
  optimalTemperature?: number
  minTemperature?: number
  maxTemperature?: number
  optimalPH?: number
  minPH?: number
  maxPH?: number
  optimalOxygen?: number
  minOxygen?: number
  optimalSalinity?: number
  minSalinity?: number
  maxSalinity?: number
}

export interface Sensor {
  id: string
  tankId: string
  sensorType: SensorType
  model: string
  manufacturer: string
  serialNumber: string
  status: SensorStatus
  isActive: boolean
  minValue?: number
  maxValue?: number
  accuracy: number
  installationDate: string
  calibrationDate?: string
  nextCalibrationDate?: string
  isCalibrationDue: boolean
}

export interface SensorReading {
  id: string
  sensorId: string
  timestamp: string
  value: number
  qualityScore: number
  metadata?: Record<string, any>
}

// Enums
export enum TankType {
  Freshwater = 'Freshwater',
  Saltwater = 'Saltwater',
  Breeding = 'Breeding',
  Quarantine = 'Quarantine',
  Nursery = 'Nursery',
  GrowOut = 'Grow_out',
  Broodstock = 'Broodstock'
}

export enum TankStatus {
  Inactive = 'Inactive',
  Active = 'Active',
  Maintenance = 'Maintenance',
  Emergency = 'Emergency',
  Cleaning = 'Cleaning'
}

export enum SensorType {
  Temperature = 'Temperature',
  pH = 'pH',
  DissolvedOxygen = 'DissolvedOxygen',
  Salinity = 'Salinity',
  Turbidity = 'Turbidity',
  Ammonia = 'Ammonia',
  Nitrite = 'Nitrite',
  Nitrate = 'Nitrate',
  Phosphate = 'Phosphate',
  Alkalinity = 'Alkalinity'
}

export type AlertSeverity = 'Critical' | 'Warning' | 'Info'
export type AlertType = 'Temperature' | 'pH' | 'DissolvedOxygen' | 'Salinity' | 'System' | 'Maintenance'

export interface Alert {
  id: string
  tankId?: string
  type: AlertType
  severity: AlertSeverity
  title: string
  message: string
  value?: number
  threshold?: number
  unit?: string
  createdAt: Date
  isResolved: boolean
  resolvedAt?: Date | null
  resolvedBy?: string | null
  metadata?: Record<string, any>
}

export enum SensorStatus {
  Offline = 'Offline',
  Online = 'Online',
  Calibrating = 'Calibrating',
  Error = 'Error',
  Maintenance = 'Maintenance'
}

// Command Types
export interface CreateTankCommand {
  name: string
  capacity: number
  capacityUnit: string
  building: string
  room: string
  zone?: string
  latitude?: number
  longitude?: number
  tankType: TankType
}

export interface UpdateTankCommand extends CreateTankCommand {
  id: string
}

// Query Types
export interface GetTanksQuery {
  page?: number
  pageSize?: number
  searchTerm?: string
  tankType?: TankType
  isActive?: boolean
  sortBy?: string
  sortDescending?: boolean
}

