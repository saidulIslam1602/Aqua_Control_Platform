<template>
  <div class="modern-tanks-view">
    <!-- Page Header -->
    <section class="page-header section--sm">
      <div class="container">
        <div class="header-content">
          <div class="header-text">
            <h1 class="modern-heading modern-heading--h1">Tank Management</h1>
            <p class="modern-text modern-text--lead">
              Monitor and manage all your aquaculture tanks in real-time
            </p>
          </div>
          <button class="btn btn-primary btn-large" @click="showCreateTankModal">
            <el-icon><Plus /></el-icon>
            <span>Add New Tank</span>
          </button>
        </div>
      </div>
    </section>

    <!-- Filters Section -->
    <section class="filters-section bg--secondary">
      <div class="container">
        <div class="filters-wrapper">
          <!-- Search -->
          <div class="filter-item filter-search">
            <el-input
              v-model="searchTerm"
              placeholder="Search tanks..."
              :prefix-icon="Search"
              size="large"
              clearable
            />
          </div>

          <!-- Tank Type Filter -->
          <div class="filter-item">
            <el-select
              v-model="selectedType"
              placeholder="Tank Type"
              size="large"
              clearable
            >
              <el-option label="All Types" value="" />
              <el-option label="Freshwater" value="Freshwater" />
              <el-option label="Saltwater" value="Saltwater" />
              <el-option label="Breeding" value="Breeding" />
              <el-option label="Grow-out" value="GrowOut" />
            </el-select>
          </div>

          <!-- Status Filter -->
          <div class="filter-item">
            <el-select
              v-model="selectedStatus"
              placeholder="Status"
              size="large"
              clearable
            >
              <el-option label="All Status" value="" />
              <el-option label="Active" :value="true" />
              <el-option label="Inactive" :value="false" />
            </el-select>
          </div>

          <!-- View Toggle -->
          <div class="filter-item view-toggle">
            <el-radio-group v-model="viewMode" size="large">
              <el-radio-button value="grid">
                <el-icon><Grid /></el-icon>
              </el-radio-button>
              <el-radio-button value="list">
                <el-icon><List /></el-icon>
              </el-radio-button>
            </el-radio-group>
          </div>
        </div>
      </div>
    </section>

    <!-- Stats Overview -->
    <section class="section--sm bg--secondary">
      <div class="container">
        <div class="stats-overview">
          <div class="stat-card">
            <div class="stat-icon" style="--stat-color: var(--color-primary-500)">
              <el-icon :size="28"><DataAnalysis /></el-icon>
            </div>
            <div class="stat-content">
              <div class="stat-value">{{ tankStore.tanks.length }}</div>
              <div class="stat-label">Total Tanks</div>
            </div>
          </div>
          <div class="stat-card">
            <div class="stat-icon" style="--stat-color: var(--color-success)">
              <el-icon :size="28"><CircleCheck /></el-icon>
            </div>
            <div class="stat-content">
              <div class="stat-value">{{ tankStore.activeTanks.length }}</div>
              <div class="stat-label">Active Tanks</div>
            </div>
          </div>
          <div class="stat-card">
            <div class="stat-icon" style="--stat-color: var(--color-warning)">
              <el-icon :size="28"><Tools /></el-icon>
            </div>
            <div class="stat-content">
              <div class="stat-value">{{ maintenanceDueCount }}</div>
              <div class="stat-label">Maintenance Due</div>
            </div>
          </div>
          <div class="stat-card">
            <div class="stat-icon" style="--stat-color: var(--color-info)">
              <el-icon :size="28"><Monitor /></el-icon>
            </div>
            <div class="stat-content">
              <div class="stat-value">{{ totalSensors }}</div>
              <div class="stat-label">Active Sensors</div>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- Tanks Grid/List -->
    <section class="section">
      <div class="container">
        <!-- Loading State -->
        <div v-if="tankStore.isLoading" class="loading-state">
          <el-icon class="is-loading" :size="48"><Loading /></el-icon>
          <p>Loading tanks...</p>
        </div>

        <!-- Empty State -->
        <div v-else-if="filteredTanks.length === 0" class="empty-state">
          <el-icon :size="64"><Box /></el-icon>
          <h3>No tanks found</h3>
          <p>{{ searchTerm ? 'Try adjusting your filters' : 'Get started by creating your first tank' }}</p>
          <button v-if="!searchTerm" class="btn btn-primary" @click="showCreateTankModal">
            <el-icon><Plus /></el-icon>
            <span>Create Tank</span>
          </button>
        </div>

        <!-- Grid View -->
        <div v-else-if="viewMode === 'grid'" class="tanks-grid grid grid--3">
          <ModernCard
            v-for="tank in filteredTanks"
            :key="tank.id"
            variant="product"
            :hover="true"
            :clickable="true"
            class="tank-card scroll-animate"
            @click="viewTankDetails(tank.id)"
          >
            <template #media>
              <div class="tank-image">
                <img 
                  :src="getTankImage(tank.tankType)" 
                  :alt="tank.name"
                />
                <div class="tank-status-badge" :class="getStatusClass(tank)">
                  <span class="status-dot"></span>
                  <span>{{ tank.status }}</span>
                </div>
              </div>
            </template>

            <template #header>
              <h3 class="tank-name">{{ tank.name }}</h3>
              <div class="tank-meta">
                <span class="tank-type">
                  <el-icon><Ticket /></el-icon>
                  {{ tank.tankType }}
                </span>
              </div>
            </template>

            <div class="tank-details">
              <div class="detail-row">
                <span class="detail-label">Capacity:</span>
                <span class="detail-value">{{ tank.capacity.value }} {{ tank.capacity.unit }}</span>
              </div>
              <div class="detail-row">
                <span class="detail-label">Location:</span>
                <span class="detail-value">{{ tank.location.building }}, {{ tank.location.room }}</span>
              </div>
              <div class="detail-row">
                <span class="detail-label">Sensors:</span>
                <span class="detail-value">{{ tank.activeSensorCount }}/{{ tank.sensorCount }}</span>
              </div>
            </div>

            <template #footer>
              <div class="tank-actions">
                <button class="action-btn" @click.stop="activateTank(tank.id)" v-if="!tank.isActive" title="Activate">
                  <el-icon><VideoPlay /></el-icon>
                </button>
                <button class="action-btn" @click.stop="deactivateTank(tank.id)" v-else title="Deactivate">
                  <el-icon><VideoPause /></el-icon>
                </button>
                <button class="action-btn" @click.stop="scheduleMaintenance(tank.id)" title="Schedule Maintenance">
                  <el-icon><Calendar /></el-icon>
                </button>
                <button class="action-btn action-btn--primary" @click.stop="viewTankDetails(tank.id)" title="View Details">
                  <el-icon><View /></el-icon>
                </button>
              </div>
            </template>
          </ModernCard>
        </div>

        <!-- List View -->
        <div v-else class="tanks-list">
          <div
            v-for="tank in filteredTanks"
            :key="tank.id"
            class="tank-list-item scroll-animate"
            @click="viewTankDetails(tank.id)"
          >
            <div class="tank-list-status" :class="getStatusClass(tank)">
              <span class="status-dot"></span>
            </div>
            <div class="tank-list-info">
              <h4>{{ tank.name }}</h4>
              <p>{{ tank.tankType }} • {{ tank.location.building }}, {{ tank.location.room }}</p>
            </div>
            <div class="tank-list-stats">
              <div class="stat-item">
                <span class="label">Capacity</span>
                <span class="value">{{ tank.capacity.value }} {{ tank.capacity.unit }}</span>
              </div>
              <div class="stat-item">
                <span class="label">Sensors</span>
                <span class="value">{{ tank.activeSensorCount }}/{{ tank.sensorCount }}</span>
              </div>
              <div class="stat-item">
                <span class="label">Status</span>
                <span class="value">{{ tank.status }}</span>
              </div>
            </div>
            <div class="tank-list-actions">
              <el-icon><ArrowRight /></el-icon>
            </div>
          </div>
        </div>
      </div>
    </section>

    <!-- Create Tank Modal -->
    <el-dialog
      v-model="showCreateModal"
      title="Create New Tank"
      width="600px"
      :close-on-click-modal="false"
    >
      <el-form
        ref="tankFormRef"
        :model="tankForm"
        :rules="tankFormRules"
        label-position="top"
      >
        <el-form-item label="Tank Name" prop="name">
          <el-input
            v-model="tankForm.name"
            placeholder="Enter tank name"
            size="large"
          />
        </el-form-item>

        <el-form-item label="Tank Type" prop="tankType">
          <el-select
            v-model="tankForm.tankType"
            placeholder="Select tank type"
            size="large"
            style="width: 100%"
          >
            <el-option label="Freshwater" value="Freshwater" />
            <el-option label="Saltwater" value="Saltwater" />
            <el-option label="Breeding" value="Breeding" />
            <el-option label="Grow-out" value="GrowOut" />
          </el-select>
        </el-form-item>

        <div class="form-row">
          <el-form-item label="Capacity" prop="capacity">
            <el-input
              v-model.number="tankForm.capacity"
              type="number"
              placeholder="Enter capacity"
              size="large"
            />
          </el-form-item>

          <el-form-item label="Unit" prop="unit">
            <el-select
              v-model="tankForm.unit"
              placeholder="Unit"
              size="large"
            >
              <el-option label="Liters" value="L" />
              <el-option label="Gallons" value="gal" />
              <el-option label="Cubic Meters" value="m³" />
            </el-select>
          </el-form-item>
        </div>

        <div class="form-row">
          <el-form-item label="Building" prop="building">
            <el-input
              v-model="tankForm.building"
              placeholder="Building name"
              size="large"
            />
          </el-form-item>

          <el-form-item label="Room" prop="room">
            <el-input
              v-model="tankForm.room"
              placeholder="Room number"
              size="large"
            />
          </el-form-item>
        </div>

        <el-form-item label="Description">
          <el-input
            v-model="tankForm.description"
            type="textarea"
            :rows="3"
            placeholder="Enter tank description (optional)"
          />
        </el-form-item>
      </el-form>

      <template #footer>
        <div class="dialog-footer">
          <button class="btn btn-outline" @click="closeCreateModal">Cancel</button>
          <button class="btn btn-primary" @click="createTank" :disabled="isCreating">
            <el-icon v-if="isCreating" class="is-loading"><Loading /></el-icon>
            <span>{{ isCreating ? 'Creating...' : 'Create Tank' }}</span>
          </button>
        </div>
      </template>
    </el-dialog>

    <!-- Schedule Maintenance Modal -->
    <ScheduleMaintenanceModal
      v-model="showMaintenanceModal"
      :tank-id="selectedTankForMaintenance"
      @scheduled="handleMaintenanceScheduled"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useTankStore } from '@/stores/tankStore'
import ModernCard from '@/components/common/ModernCard.vue'
import ScheduleMaintenanceModal from '@/components/modals/ScheduleMaintenanceModal.vue'
import {
  Plus,
  Search,
  Grid,
  List,
  DataAnalysis,
  CircleCheck,
  Tools,
  Monitor,
  Loading,
  Box,
  Ticket,
  VideoPlay,
  VideoPause,
  Calendar,
  View,
  ArrowRight
} from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import type { FormInstance } from 'element-plus'

// Router and Store
const router = useRouter()
const tankStore = useTankStore()

// State
const searchTerm = ref('')
const selectedType = ref('')
const selectedStatus = ref<boolean | ''>('')
const viewMode = ref<'grid' | 'list'>('grid')
const showCreateModal = ref(false)
const showMaintenanceModal = ref(false)
const selectedTankForMaintenance = ref<string>('')
const isCreating = ref(false)
const tankFormRef = ref()

// Tank form data
const tankForm = ref({
  name: '',
  tankType: '',
  capacity: 0,
  unit: 'L',
  building: '',
  room: '',
  description: ''
})

// Form validation rules
const tankFormRules = {
  name: [
    { required: true, message: 'Please enter tank name', trigger: 'blur' },
    { min: 3, max: 50, message: 'Name should be 3-50 characters', trigger: 'blur' }
  ],
  tankType: [
    { required: true, message: 'Please select tank type', trigger: 'change' }
  ],
  capacity: [
    { required: true, message: 'Please enter capacity', trigger: 'blur' },
    { type: 'number', min: 1, message: 'Capacity must be greater than 0', trigger: 'blur' }
  ],
  unit: [
    { required: true, message: 'Please select unit', trigger: 'change' }
  ],
  building: [
    { required: true, message: 'Please enter building', trigger: 'blur' }
  ],
  room: [
    { required: true, message: 'Please enter room', trigger: 'blur' }
  ]
}

// Computed
const filteredTanks = computed(() => {
  let tanks = tankStore.tanks

  if (searchTerm.value) {
    const search = searchTerm.value.toLowerCase()
    tanks = tanks.filter(tank =>
      tank.name.toLowerCase().includes(search) ||
      tank.location.building.toLowerCase().includes(search) ||
      tank.location.room.toLowerCase().includes(search)
    )
  }

  if (selectedType.value) {
    tanks = tanks.filter(tank => tank.tankType === selectedType.value)
  }

  if (selectedStatus.value !== '') {
    tanks = tanks.filter(tank => tank.isActive === selectedStatus.value)
  }

  return tanks
})

const maintenanceDueCount = computed(() =>
  tankStore.tanks.filter(tank => tank.isMaintenanceDue).length
)

const totalSensors = computed(() =>
  tankStore.tanks.reduce((sum, tank) => sum + tank.activeSensorCount, 0)
)

// Methods
const getTankImage = (type: string) => {
  const images: Record<string, string> = {
    Freshwater: 'https://images.unsplash.com/photo-1535591273668-578e31182c4f?w=800&q=80&auto=format&fit=crop',
    Saltwater: 'https://images.unsplash.com/photo-1559827260-dc66d52bef19?w=800&q=80&auto=format&fit=crop',
    Breeding: 'https://images.unsplash.com/photo-1544551763-46a013bb70d5?w=800&q=80&auto=format&fit=crop',
    GrowOut: 'https://images.unsplash.com/photo-1593642634367-d91a135587b5?w=800&q=80&auto=format&fit=crop'
  }
  return images[type] || images.Freshwater
}

const getStatusClass = (tank: any) => {
  if (tank.isActive) return 'status-active'
  if (tank.isMaintenanceDue) return 'status-maintenance'
  return 'status-inactive'
}

const viewTankDetails = (id: string) => {
  router.push(`/tanks/${id}`)
}

const showCreateTankModal = () => {
  showCreateModal.value = true
}

const closeCreateModal = () => {
  showCreateModal.value = false
  tankFormRef.value?.resetFields()
  tankForm.value = {
    name: '',
    tankType: '',
    capacity: 0,
    unit: 'L',
    building: '',
    room: '',
    description: ''
  }
}

const createTank = async () => {
  if (!tankFormRef.value) return

  await tankFormRef.value.validate(async (valid: boolean) => {
    if (valid) {
      isCreating.value = true
      try {
        await tankStore.createTank({
          name: tankForm.value.name,
          tankType: tankForm.value.tankType,
          capacity: tankForm.value.capacity,
          capacityUnit: tankForm.value.unit,
          building: tankForm.value.building,
          room: tankForm.value.room,
          zone: '',
          latitude: undefined,
          longitude: undefined,
          description: tankForm.value.description
        })
        
        ElMessage.success('Tank created successfully')
        closeCreateModal()
      } catch (error: any) {
        ElMessage.error(error.message || 'Failed to create tank')
      } finally {
        isCreating.value = false
      }
    }
  })
}

const activateTank = async (id: string) => {
  try {
    await tankStore.updateTankStatus(id, true)
    ElMessage.success('Tank activated successfully')
  } catch (error: any) {
    ElMessage.error(error.message || 'Failed to activate tank')
  }
}

const deactivateTank = async (id: string) => {
  try {
    await tankStore.updateTankStatus(id, false)
    ElMessage.success('Tank deactivated successfully')
  } catch (error: any) {
    ElMessage.error(error.message || 'Failed to deactivate tank')
  }
}

const scheduleMaintenance = (id: string) => {
  selectedTankForMaintenance.value = id
  showMaintenanceModal.value = true
}

const handleMaintenanceScheduled = (data: any) => {
  console.log('Maintenance scheduled:', data)
  // Optionally refresh tanks to show updated maintenance status
  tankStore.refresh()
}

// Lifecycle
onMounted(async () => {
  await tankStore.fetchTanks()
})
</script>

<style lang="scss" scoped>
@import '@/styles/design-system/index.scss';

.modern-tanks-view {
  min-height: 100vh;
}

.page-header {
  background: linear-gradient(135deg, var(--color-primary-50), var(--color-secondary-50));
  
  .header-content {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: var(--space-6);
    
    @media (max-width: 768px) {
      flex-direction: column;
      align-items: flex-start;
    }
  }

  .header-text {
    flex: 1;
  }
}

.filters-section {
  padding: var(--space-6) 0;
}

.filters-wrapper {
  display: flex;
  gap: var(--space-4);
  flex-wrap: wrap;
  align-items: center;

  .filter-item {
    flex: 1;
    min-width: 200px;

    &.filter-search {
      flex: 2;
    }

    &.view-toggle {
      flex: 0;
      min-width: auto;
    }
  }
}

.stats-overview {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: var(--space-4);
}

.stat-card {
  display: flex;
  align-items: center;
  gap: var(--space-4);
  padding: var(--space-5);
  background: white;
  border-radius: var(--border-radius-lg);
  box-shadow: var(--shadow-sm);
  transition: all var(--transition-base);

  &:hover {
    transform: translateY(-2px);
    box-shadow: var(--shadow-md);
  }

  .stat-icon {
    width: 56px;
    height: 56px;
    display: flex;
    align-items: center;
    justify-content: center;
    background: linear-gradient(135deg, var(--stat-color), var(--stat-color));
    border-radius: var(--border-radius-lg);
    color: white;
    opacity: 0.9;
  }

  .stat-content {
    flex: 1;
  }

  .stat-value {
    font-size: var(--font-size-2xl);
    font-weight: var(--font-weight-bold);
    font-family: var(--font-family-display);
    color: var(--color-text-primary);
    line-height: 1;
    margin-bottom: var(--space-1);
  }

  .stat-label {
    font-size: var(--font-size-sm);
    color: var(--color-text-tertiary);
  }
}

.loading-state,
.empty-state {
  text-align: center;
  padding: var(--space-20) var(--space-6);
  color: var(--color-text-tertiary);

  .el-icon {
    margin-bottom: var(--space-4);
    color: var(--color-text-tertiary);
  }

  h3 {
    font-size: var(--font-size-xl);
    color: var(--color-text-primary);
    margin: var(--space-4) 0;
  }

  p {
    margin-bottom: var(--space-6);
  }
}

.tanks-grid {
  animation-delay: 0ms;
}

.tank-card {
  .tank-image {
    position: relative;
    width: 100%;
    height: 200px;
    overflow: hidden;
    background: var(--color-neutral-100);

    img {
      width: 100%;
      height: 100%;
      object-fit: cover;
    }
  }

  .tank-status-badge {
    position: absolute;
    top: var(--space-3);
    right: var(--space-3);
    display: flex;
    align-items: center;
    gap: var(--space-2);
    padding: var(--space-2) var(--space-3);
    background: rgba(0, 0, 0, 0.7);
    backdrop-filter: blur(10px);
    color: white;
    font-size: var(--font-size-xs);
    font-weight: var(--font-weight-semibold);
    border-radius: var(--border-radius-full);
    text-transform: uppercase;
    letter-spacing: 0.05em;

    .status-dot {
      width: 8px;
      height: 8px;
      border-radius: 50%;
      background: currentColor;
    }

    &.status-active {
      background: rgba(16, 185, 129, 0.9);
      .status-dot {
        animation: pulse 2s infinite;
      }
    }

    &.status-maintenance {
      background: rgba(245, 158, 11, 0.9);
    }

    &.status-inactive {
      background: rgba(107, 114, 128, 0.9);
    }
  }

  .tank-name {
    font-size: var(--font-size-lg);
    font-weight: var(--font-weight-semibold);
    color: var(--color-text-primary);
    margin: 0 0 var(--space-2);
  }

  .tank-meta {
    display: flex;
    align-items: center;
    gap: var(--space-2);
    font-size: var(--font-size-sm);
    color: var(--color-text-tertiary);
  }

  .tank-details {
    display: flex;
    flex-direction: column;
    gap: var(--space-3);

    .detail-row {
      display: flex;
      justify-content: space-between;
      font-size: var(--font-size-sm);

      .detail-label {
        color: var(--color-text-tertiary);
      }

      .detail-value {
        color: var(--color-text-primary);
        font-weight: var(--font-weight-medium);
      }
    }
  }

  .tank-actions {
    display: flex;
    gap: var(--space-2);
    padding-top: var(--space-2);
    border-top: 1px solid var(--color-neutral-200);
  }

  .action-btn {
    flex: 1;
    display: flex;
    align-items: center;
    justify-content: center;
    padding: var(--space-2);
    background: var(--color-neutral-50);
    border: none;
    border-radius: var(--border-radius-md);
    color: var(--color-text-secondary);
    cursor: pointer;
    transition: all var(--transition-fast);

    &:hover {
      background: var(--color-neutral-100);
      color: var(--color-primary-600);
      transform: translateY(-2px);
    }

    &--primary {
      background: var(--color-primary-500);
      color: white;

      &:hover {
        background: var(--color-primary-600);
      }
    }
  }
}

.tanks-list {
  display: flex;
  flex-direction: column;
  gap: var(--space-4);
}

.tank-list-item {
  display: flex;
  align-items: center;
  gap: var(--space-4);
  padding: var(--space-5);
  background: white;
  border: 1px solid var(--color-neutral-200);
  border-radius: var(--border-radius-lg);
  cursor: pointer;
  transition: all var(--transition-base);

  &:hover {
    transform: translateX(4px);
    box-shadow: var(--shadow-md);
    border-color: var(--color-primary-200);
  }

  .tank-list-status {
    width: 12px;
    height: 12px;
    border-radius: 50%;

    &.status-active {
      background: var(--color-success);
      animation: pulse 2s infinite;
    }

    &.status-maintenance {
      background: var(--color-warning);
    }

    &.status-inactive {
      background: var(--color-neutral-400);
    }
  }

  .tank-list-info {
    flex: 1;

    h4 {
      font-size: var(--font-size-lg);
      font-weight: var(--font-weight-semibold);
      color: var(--color-text-primary);
      margin: 0 0 var(--space-1);
    }

    p {
      font-size: var(--font-size-sm);
      color: var(--color-text-tertiary);
      margin: 0;
    }
  }

  .tank-list-stats {
    display: flex;
    gap: var(--space-8);

    @media (max-width: 768px) {
      display: none;
    }

    .stat-item {
      text-align: center;

      .label {
        display: block;
        font-size: var(--font-size-xs);
        color: var(--color-text-tertiary);
        margin-bottom: var(--space-1);
      }

      .value {
        display: block;
        font-size: var(--font-size-base);
        font-weight: var(--font-weight-semibold);
        color: var(--color-text-primary);
      }
    }
  }

  .tank-list-actions {
    color: var(--color-text-tertiary);
    transition: transform var(--transition-fast);
  }

  &:hover .tank-list-actions {
    transform: translateX(4px);
  }
}

@keyframes pulse {
  0%, 100% {
    opacity: 1;
  }
  50% {
    opacity: 0.5;
  }
}

.form-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--space-4);

  @media (max-width: 768px) {
    grid-template-columns: 1fr;
  }
}

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: var(--space-3);
}
</style>
