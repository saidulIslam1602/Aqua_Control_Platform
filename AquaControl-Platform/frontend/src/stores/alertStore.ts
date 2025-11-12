import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { Alert, AlertSeverity, AlertType } from '@/types/domain'

export const useAlertStore = defineStore('alerts', () => {
  // State
  const alerts = ref<Alert[]>([])
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  // Computed
  const activeAlerts = computed(() => 
    alerts.value.filter(alert => !alert.isResolved)
  )

  const criticalAlerts = computed(() =>
    activeAlerts.value.filter(alert => alert.severity === 'Critical')
  )

  const warningAlerts = computed(() =>
    activeAlerts.value.filter(alert => alert.severity === 'Warning')
  )

  const infoAlerts = computed(() =>
    activeAlerts.value.filter(alert => alert.severity === 'Info')
  )

  const alertCount = computed(() => activeAlerts.value.length)

  const alertsByTank = computed(() => {
    const grouped = new Map<string, Alert[]>()
    activeAlerts.value.forEach(alert => {
      if (alert.tankId) {
        if (!grouped.has(alert.tankId)) {
          grouped.set(alert.tankId, [])
        }
        grouped.get(alert.tankId)!.push(alert)
      }
    })
    return grouped
  })

  // Actions
  const fetchAlerts = async () => {
    try {
      isLoading.value = true
      error.value = null

      // TODO: Replace with actual API call
      // For now, generate sample alerts
      const sampleAlerts: Alert[] = [
        {
          id: '1',
          tankId: 'tank-1',
          type: 'Temperature',
          severity: 'Critical',
          title: 'High Temperature Alert',
          message: 'Temperature in Tank 1 has exceeded safe limits (28°C)',
          value: 28.5,
          threshold: 25.0,
          unit: '°C',
          createdAt: new Date(Date.now() - 1000 * 60 * 30), // 30 minutes ago
          isResolved: false,
          resolvedAt: null,
          resolvedBy: null
        },
        {
          id: '2',
          tankId: 'tank-2',
          type: 'pH',
          severity: 'Warning',
          title: 'pH Level Warning',
          message: 'pH level in Tank 2 is approaching critical range',
          value: 6.2,
          threshold: 6.5,
          unit: 'pH',
          createdAt: new Date(Date.now() - 1000 * 60 * 60), // 1 hour ago
          isResolved: false,
          resolvedAt: null,
          resolvedBy: null
        },
        {
          id: '3',
          tankId: 'tank-3',
          type: 'DissolvedOxygen',
          severity: 'Warning',
          title: 'Low Oxygen Level',
          message: 'Dissolved oxygen in Tank 3 is below optimal range',
          value: 4.2,
          threshold: 5.0,
          unit: 'mg/L',
          createdAt: new Date(Date.now() - 1000 * 60 * 90), // 1.5 hours ago
          isResolved: false,
          resolvedAt: null,
          resolvedBy: null
        }
      ]

      alerts.value = sampleAlerts
    } catch (err: any) {
      error.value = err.message || 'Failed to fetch alerts'
      console.error('Failed to fetch alerts:', err)
    } finally {
      isLoading.value = false
    }
  }

  const addAlert = (alert: Omit<Alert, 'id' | 'createdAt'>) => {
    const newAlert: Alert = {
      ...alert,
      id: crypto.randomUUID(),
      createdAt: new Date()
    }
    alerts.value.unshift(newAlert)
  }

  const resolveAlert = async (alertId: string, resolvedBy: string) => {
    const alert = alerts.value.find(a => a.id === alertId)
    if (alert) {
      alert.isResolved = true
      alert.resolvedAt = new Date()
      alert.resolvedBy = resolvedBy
    }
  }

  const dismissAlert = (alertId: string) => {
    const index = alerts.value.findIndex(a => a.id === alertId)
    if (index !== -1) {
      alerts.value.splice(index, 1)
    }
  }

  const clearResolvedAlerts = () => {
    alerts.value = alerts.value.filter(alert => !alert.isResolved)
  }

  const getAlertsByTank = (tankId: string) => {
    return alerts.value.filter(alert => alert.tankId === tankId)
  }

  const getAlertsBySeverity = (severity: AlertSeverity) => {
    return alerts.value.filter(alert => alert.severity === severity)
  }

  const getAlertsCount = () => {
    return {
      total: alertCount.value,
      critical: criticalAlerts.value.length,
      warning: warningAlerts.value.length,
      info: infoAlerts.value.length
    }
  }

  return {
    // State
    alerts: readonly(alerts),
    isLoading: readonly(isLoading),
    error: readonly(error),
    
    // Computed
    activeAlerts,
    criticalAlerts,
    warningAlerts,
    infoAlerts,
    alertCount,
    alertsByTank,
    
    // Actions
    fetchAlerts,
    addAlert,
    resolveAlert,
    dismissAlert,
    clearResolvedAlerts,
    getAlertsByTank,
    getAlertsBySeverity,
    getAlertsCount
  }
}, {
  persist: {
    key: 'aquacontrol-alerts',
    storage: localStorage,
    paths: ['alerts']
  }
})

function readonly<T>(ref: any): T {
  return ref as T
}
