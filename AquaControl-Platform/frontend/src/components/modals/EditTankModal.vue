<template>
  <el-dialog
    v-model="isVisible"
    title="Edit Tank"
    width="600px"
    :close-on-click-modal="false"
    @close="handleClose"
  >
    <div v-if="tank" class="tank-info">
      <el-alert
        type="info"
        :closable="false"
        show-icon
      >
        Editing: {{ tank.name }}
      </el-alert>
    </div>

    <el-form
      ref="formRef"
      :model="form"
      :rules="rules"
      label-position="top"
    >
      <el-form-item label="Tank Name" prop="name">
        <el-input
          v-model="form.name"
          placeholder="Enter tank name"
          size="large"
        />
      </el-form-item>

      <el-form-item label="Tank Type" prop="tankType">
        <el-select
          v-model="form.tankType"
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
            v-model.number="form.capacity"
            type="number"
            placeholder="Enter capacity"
            size="large"
          />
        </el-form-item>

        <el-form-item label="Unit" prop="unit">
          <el-select
            v-model="form.unit"
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
            v-model="form.building"
            placeholder="Building name"
            size="large"
          />
        </el-form-item>

        <el-form-item label="Room" prop="room">
          <el-input
            v-model="form.room"
            placeholder="Room number"
            size="large"
          />
        </el-form-item>
      </div>

      <el-form-item label="Zone (Optional)">
        <el-input
          v-model="form.zone"
          placeholder="Zone or section"
          size="large"
        />
      </el-form-item>

      <el-form-item label="Description">
        <el-input
          v-model="form.description"
          type="textarea"
          :rows="3"
          placeholder="Enter tank description (optional)"
        />
      </el-form-item>
    </el-form>

    <template #footer>
      <div class="dialog-footer">
        <button class="btn btn-outline" @click="handleClose">Cancel</button>
        <button class="btn btn-primary" @click="handleSubmit" :disabled="isSubmitting">
          <el-icon v-if="isSubmitting" class="is-loading"><Loading /></el-icon>
          <span>{{ isSubmitting ? 'Updating...' : 'Update Tank' }}</span>
        </button>
      </div>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { useTankStore } from '@/stores/tankStore'
import { ElMessage } from 'element-plus'
import { Loading } from '@element-plus/icons-vue'
import type { FormInstance } from 'element-plus'
import type { Tank } from '@/types/domain'

interface Props {
  modelValue: boolean
  tank: Tank | null
}

interface Emits {
  (e: 'update:modelValue', value: boolean): void
  (e: 'updated'): void
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

const tankStore = useTankStore()
const formRef = ref<FormInstance>()
const isSubmitting = ref(false)

const isVisible = computed({
  get: () => props.modelValue,
  set: (value) => emit('update:modelValue', value)
})

const form = ref({
  name: '',
  tankType: '',
  capacity: 0,
  unit: 'L',
  building: '',
  room: '',
  zone: '',
  description: ''
})

const rules = {
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

const loadTankData = () => {
  if (props.tank) {
    form.value = {
      name: props.tank.name,
      tankType: props.tank.tankType,
      capacity: props.tank.capacity.value,
      unit: props.tank.capacity.unit,
      building: props.tank.location.building,
      room: props.tank.location.room,
      zone: props.tank.location.zone || '',
      description: ''
    }
  }
}

const handleClose = () => {
  formRef.value?.resetFields()
  isVisible.value = false
}

const handleSubmit = async () => {
  if (!formRef.value || !props.tank) return

  await formRef.value.validate(async (valid) => {
    if (valid) {
      isSubmitting.value = true
      try {
        await tankStore.updateTank(props.tank!.id, {
          id: props.tank!.id,
          name: form.value.name,
          tankType: form.value.tankType as any,
          capacity: form.value.capacity,
          capacityUnit: form.value.unit,
          building: form.value.building,
          room: form.value.room,
          zone: form.value.zone,
          latitude: undefined,
          longitude: undefined
        })

        emit('updated')
        handleClose()
      } catch (error: any) {
        ElMessage.error(error.message || 'Failed to update tank')
      } finally {
        isSubmitting.value = false
      }
    }
  })
}

// Watch for tank prop changes
watch(() => props.tank, (newValue) => {
  if (newValue && props.modelValue) {
    loadTankData()
  }
}, { immediate: true, deep: true })

// Watch for modal visibility
watch(() => props.modelValue, (newValue) => {
  if (newValue && props.tank) {
    loadTankData()
  }
})
</script>

<style lang="scss" scoped>
.tank-info {
  margin-bottom: var(--space-6);
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

