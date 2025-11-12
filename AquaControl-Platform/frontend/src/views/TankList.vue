<template>
  <div class="tank-list">
    <h1>Tanks</h1>
    
    <div v-if="tankStore.isLoading">
      Loading tanks...
    </div>
    
    <div v-else-if="tankStore.hasError">
      Error: {{ tankStore.state.error }}
    </div>
    
    <div v-else>
      <div class="tank-grid">
        <div 
          v-for="tank in tankStore.filteredTanks" 
          :key="tank.id"
          class="tank-card"
          @click="selectTank(tank)"
        >
          <h3>{{ tank.name }}</h3>
          <p>Type: {{ tank.tankType }}</p>
          <p>Status: {{ tank.status }}</p>
          <p>Location: {{ tank.location.fullAddress }}</p>
          <p>Capacity: {{ tank.capacity.value }} {{ tank.capacity.unit }}</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useTankStore } from '@stores/tankStore'
import type { Tank } from '@types/domain'

const router = useRouter()
const tankStore = useTankStore()

const selectTank = (tank: Tank) => {
  tankStore.setSelectedTank(tank)
  router.push(`/tanks/${tank.id}`)
}

onMounted(() => {
  tankStore.fetchTanks()
})
</script>

<style scoped>
.tank-list {
  padding: 20px;
}

.tank-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 20px;
  margin-top: 20px;
}

.tank-card {
  border: 1px solid #ddd;
  border-radius: 8px;
  padding: 16px;
  cursor: pointer;
  transition: box-shadow 0.2s;
}

.tank-card:hover {
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
}

.tank-card h3 {
  margin-top: 0;
  color: #333;
}

.tank-card p {
  margin: 8px 0;
  color: #666;
}
</style>
