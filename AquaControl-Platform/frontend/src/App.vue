<template>
  <div id="app">
    <RouterView />
  </div>
</template>

<script setup lang="ts">
import { RouterView } from 'vue-router'
import { onMounted } from 'vue'
import { useRealTimeStore } from '@stores/realTimeStore'

const realTimeStore = useRealTimeStore()

onMounted(async () => {
  // Initialize real-time connection
  try {
    await realTimeStore.connect()
  } catch (error) {
    console.error('Failed to connect to real-time hub:', error)
  }
})
</script>

<style>
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
}
</style>
