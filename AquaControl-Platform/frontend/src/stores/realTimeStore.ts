import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import * as signalR from '@microsoft/signalr'
import type { RealTimeEvent } from '@types/api'

type EventHandler = (event: RealTimeEvent) => void

export const useRealTimeStore = defineStore('realtime', () => {
  const connection = ref<signalR.HubConnection | null>(null)
  const isConnected = ref(false)
  const connectionState = ref<'Disconnected' | 'Connecting' | 'Connected' | 'Disconnecting' | 'Reconnecting'>('Disconnected')
  const eventHandlers = ref<Map<string, EventHandler[]>>(new Map())

  const isConnecting = computed(() => 
    connectionState.value === 'Connecting' || connectionState.value === 'Reconnecting'
  )

  const connect = async () => {
    if (connection.value && isConnected.value) {
      return
    }

    const hubUrl = import.meta.env.VITE_SIGNALR_HUB_URL || 'http://localhost:5000/hubs/tanks'
    
    connection.value = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl)
      .withAutomaticReconnect()
      .build()

    // Connection event handlers
    connection.value.onclose(() => {
      isConnected.value = false
      connectionState.value = 'Disconnected'
    })

    connection.value.onreconnecting(() => {
      connectionState.value = 'Reconnecting'
    })

    connection.value.onreconnected(() => {
      isConnected.value = true
      connectionState.value = 'Connected'
    })

    // Start connection
    try {
      connectionState.value = 'Connecting'
      await connection.value.start()
      isConnected.value = true
      connectionState.value = 'Connected'

      // Subscribe to all registered event types
      eventHandlers.value.forEach((handlers, eventType) => {
        connection.value?.on(eventType, (data: any) => {
          const event: RealTimeEvent = {
            eventType,
            data,
            timestamp: new Date().toISOString(),
            source: 'SignalR'
          }
          handlers.forEach(handler => handler(event))
        })
      })
    } catch (error) {
      console.error('Failed to connect to SignalR hub:', error)
      connectionState.value = 'Disconnected'
      throw error
    }
  }

  const disconnect = async () => {
    if (connection.value) {
      connectionState.value = 'Disconnecting'
      await connection.value.stop()
      connection.value = null
      isConnected.value = false
      connectionState.value = 'Disconnected'
    }
  }

  const subscribe = (eventType: string, handler: EventHandler) => {
    if (!eventHandlers.value.has(eventType)) {
      eventHandlers.value.set(eventType, [])
    }
    eventHandlers.value.get(eventType)!.push(handler)

    // If already connected, set up the handler immediately
    if (connection.value && isConnected.value) {
      connection.value.on(eventType, (data: any) => {
        const event: RealTimeEvent = {
          eventType,
          data,
          timestamp: new Date().toISOString(),
          source: 'SignalR'
        }
        handler(event)
      })
    }
  }

  const unsubscribe = (eventType: string, handler: EventHandler) => {
    const handlers = eventHandlers.value.get(eventType)
    if (handlers) {
      const index = handlers.indexOf(handler)
      if (index !== -1) {
        handlers.splice(index, 1)
      }
    }
  }

  const joinGroup = async (groupName: string) => {
    if (connection.value && isConnected.value) {
      await connection.value.invoke('JoinGroup', groupName)
    }
  }

  const leaveGroup = async (groupName: string) => {
    if (connection.value && isConnected.value) {
      await connection.value.invoke('LeaveGroup', groupName)
    }
  }

  return {
    connection,
    isConnected,
    isConnecting,
    connectionState,
    connect,
    disconnect,
    subscribe,
    unsubscribe,
    joinGroup,
    leaveGroup
  }
})

