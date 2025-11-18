/**
 * Export utilities for downloading data in various formats
 */

export interface ExportOptions {
  filename?: string
  format?: 'csv' | 'json' | 'excel'
}

/**
 * Convert data to CSV format
 */
export function convertToCSV(data: any[], headers?: string[]): string {
  if (!data || data.length === 0) {
    return ''
  }

  // Get headers from first object if not provided
  const csvHeaders = headers || Object.keys(data[0])
  
  // Create header row
  const headerRow = csvHeaders.join(',')
  
  // Create data rows
  const dataRows = data.map(row => {
    return csvHeaders.map(header => {
      const value = row[header]
      
      // Handle null/undefined
      if (value === null || value === undefined) {
        return ''
      }
      
      // Handle strings with commas or quotes
      if (typeof value === 'string' && (value.includes(',') || value.includes('"') || value.includes('\n'))) {
        return `"${value.replace(/"/g, '""')}"`
      }
      
      return value
    }).join(',')
  })
  
  return [headerRow, ...dataRows].join('\n')
}

/**
 * Download data as CSV file
 */
export function downloadCSV(data: any[], options: ExportOptions = {}): void {
  const { filename = 'export.csv' } = options
  
  const csv = convertToCSV(data)
  const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' })
  downloadBlob(blob, filename)
}

/**
 * Download data as JSON file
 */
export function downloadJSON(data: any, options: ExportOptions = {}): void {
  const { filename = 'export.json' } = options
  
  const json = JSON.stringify(data, null, 2)
  const blob = new Blob([json], { type: 'application/json;charset=utf-8;' })
  downloadBlob(blob, filename)
}

/**
 * Download blob as file
 */
export function downloadBlob(blob: Blob, filename: string): void {
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = filename
  link.style.display = 'none'
  
  document.body.appendChild(link)
  link.click()
  
  // Cleanup
  document.body.removeChild(link)
  URL.revokeObjectURL(url)
}

/**
 * Export analytics data
 */
export function exportAnalyticsData(data: {
  metrics: any
  tanks: any[]
  sensors: any[]
  alerts: any[]
}, format: 'csv' | 'json' = 'csv'): void {
  const timestamp = new Date().toISOString().split('T')[0]
  const filename = `analytics-export-${timestamp}.${format}`
  
  if (format === 'json') {
    downloadJSON(data, { filename })
  } else {
    // For CSV, export each section separately or combine
    const combinedData = [
      { section: 'Metrics', ...data.metrics },
      ...data.tanks.map(t => ({ section: 'Tanks', ...t })),
      ...data.sensors.map(s => ({ section: 'Sensors', ...s })),
      ...data.alerts.map(a => ({ section: 'Alerts', ...a }))
    ]
    downloadCSV(combinedData, { filename })
  }
}

/**
 * Export tank data
 */
export function exportTankData(tanks: any[], format: 'csv' | 'json' = 'csv'): void {
  const timestamp = new Date().toISOString().split('T')[0]
  const filename = `tanks-export-${timestamp}.${format}`
  
  if (format === 'json') {
    downloadJSON(tanks, { filename })
  } else {
    // Flatten tank data for CSV
    const flattenedTanks = tanks.map(tank => ({
      id: tank.id,
      name: tank.name,
      type: tank.tankType,
      capacity: tank.capacity?.value || tank.capacity,
      unit: tank.capacity?.unit || '',
      status: tank.status,
      building: tank.location?.building || '',
      room: tank.location?.room || '',
      zone: tank.location?.zone || '',
      isActive: tank.isActive,
      createdAt: tank.createdAt
    }))
    downloadCSV(flattenedTanks, { filename })
  }
}

/**
 * Export sensor data
 */
export function exportSensorData(sensors: any[], format: 'csv' | 'json' = 'csv'): void {
  const timestamp = new Date().toISOString().split('T')[0]
  const filename = `sensors-export-${timestamp}.${format}`
  
  if (format === 'json') {
    downloadJSON(sensors, { filename })
  } else {
    // Flatten sensor data for CSV
    const flattenedSensors = sensors.map(sensor => ({
      id: sensor.id,
      tankId: sensor.tankId,
      type: sensor.sensorType,
      model: sensor.model,
      manufacturer: sensor.manufacturer,
      serialNumber: sensor.serialNumber,
      status: sensor.status,
      isActive: sensor.isActive,
      minValue: sensor.minValue,
      maxValue: sensor.maxValue,
      accuracy: sensor.accuracy,
      installationDate: sensor.installationDate,
      calibrationDate: sensor.calibrationDate,
      lastReading: sensor.lastReading?.value || '',
      lastReadingTime: sensor.lastReading?.timestamp || ''
    }))
    downloadCSV(flattenedSensors, { filename })
  }
}

/**
 * Export alert data
 */
export function exportAlertData(alerts: any[], format: 'csv' | 'json' = 'csv'): void {
  const timestamp = new Date().toISOString().split('T')[0]
  const filename = `alerts-export-${timestamp}.${format}`
  
  if (format === 'json') {
    downloadJSON(alerts, { filename })
  } else {
    downloadCSV(alerts, { filename })
  }
}

export default {
  convertToCSV,
  downloadCSV,
  downloadJSON,
  downloadBlob,
  exportAnalyticsData,
  exportTankData,
  exportSensorData,
  exportAlertData
}

