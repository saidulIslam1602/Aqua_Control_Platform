<template>
  <div class="tank-detail">
    <div v-if="tankStore.isLoading">
      Loading tank details...
    </div>
    
    <div v-else-if="tankStore.hasError">
      Error: {{ tankStore.state.error }}
    </div>
    
    <div v-else-if="tank">
      <h1>{{ tank.name }}</h1>
      
      <div class="tank-info">
        <div class="info-section">
          <h3>Basic Information</h3>
          <p><strong>Type:</strong> {{ tank.tankType }}</p>
          <p><strong>Status:</strong> {{ tank.status }}</p>
          <p><strong>Capacity:</strong> {{ tank.capacity.value }} {{ tank.capacity.unit }}</p>
          <p><strong>Created:</strong> {{ formatDate(tank.createdAt) }}</p>
          <p v-if="tank.updatedAt"><strong>Updated:</strong> {{ formatDate(tank.updatedAt) }}</p>
        </div>
        
        <div class="info-section">
          <h3>Location</h3>
          <p><strong>Building:</strong> {{ tank.location.building }}</p>
          <p><strong>Room:</strong> {{ tank.location.room }}</p>
          <p v-if="tank.location.zone"><strong>Zone:</strong> {{ tank.location.zone }}</p>
          <p><strong>Full Address:</strong> {{ tank.location.fullAddress }}</p>
        </div>
        
        <div class="info-section" v-if="tank.sensors.length > 0">
          <h3>Sensors</h3>
          <div v-for="sensor in tank.sensors" :key="sensor.id" class="sensor-item">
            <p><strong>{{ sensor.sensorType }}:</strong> {{ sensor.model }} ({{ sensor.status }})</p>
          </div>
        </div>
      </div>
      
      <div class="actions">
        <button @click="goBack">Back to Tanks</button>
      </div>
    </div>
    
    <div v-else>
      Tank not found
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useTankStore } from '@stores/tankStore'

const route = useRoute()
const router = useRouter()
const tankStore = useTankStore()

const tankId = computed(() => route.params.id as string)
const tank = computed(() => tankStore.selectedTank)

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString()
}

const goBack = () => {
  router.push('/tanks')
}

onMounted(async () => {
  if (tankId.value) {
    await tankStore.fetchTankById(tankId.value)
  }
})
</script>

<style scoped>
.tank-detail {
  padding: 20px;
  max-width: 800px;
  margin: 0 auto;
}

.tank-info {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 20px;
  margin: 20px 0;
}

.info-section {
  border: 1px solid #ddd;
  border-radius: 8px;
  padding: 16px;
}

.info-section h3 {
  margin-top: 0;
  color: #333;
  border-bottom: 1px solid #eee;
  padding-bottom: 8px;
}

.info-section p {
  margin: 8px 0;
}

.sensor-item {
  padding: 8px;
  background: #f9f9f9;
  border-radius: 4px;
  margin: 8px 0;
}

.actions {
  margin-top: 20px;
}

button {
  background: #007bff;
  color: white;
  border: none;
  padding: 10px 20px;
  border-radius: 4px;
  cursor: pointer;
}

button:hover {
  background: #0056b3;
}
</style>
