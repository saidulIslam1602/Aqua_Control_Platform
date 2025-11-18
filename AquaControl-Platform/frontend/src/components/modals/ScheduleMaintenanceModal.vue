<template>
  <el-dialog
    v-model="isVisible"
    title="Schedule Maintenance"
    width="600px"
    :close-on-click-modal="false"
    @close="handleClose"
  >
    <el-form
      ref="formRef"
      :model="form"
      :rules="rules"
      label-position="top"
    >
      <el-form-item label="Tank" prop="tankId">
        <el-select
          v-model="form.tankId"
          placeholder="Select tank"
          size="large"
          style="width: 100%"
          :disabled="!!tankId"
        >
          <el-option
            v-for="tank in tanks"
            :key="tank.id"
            :label="tank.name"
            :value="tank.id"
          />
        </el-select>
      </el-form-item>

      <el-form-item label="Maintenance Date" prop="maintenanceDate">
        <el-date-picker
          v-model="form.maintenanceDate"
          type="datetime"
          placeholder="Select date and time"
          size="large"
          style="width: 100%"
          :disabled-date="disabledDate"
        />
      </el-form-item>

      <el-form-item label="Maintenance Type" prop="maintenanceType">
        <el-select
          v-model="form.maintenanceType"
          placeholder="Select maintenance type"
          size="large"
          style="width: 100%"
        >
          <el-option label="Routine Inspection" value="routine" />
          <el-option label="Deep Cleaning" value="cleaning" />
          <el-option label="Equipment Check" value="equipment" />
          <el-option label="Water Quality Test" value="water_quality" />
          <el-option label="Sensor Calibration" value="sensor_calibration" />
          <el-option label="Emergency Repair" value="emergency" />
          <el-option label="Other" value="other" />
        </el-select>
      </el-form-item>

      <el-form-item label="Assigned To" prop="assignedTo">
        <el-input
          v-model="form.assignedTo"
          placeholder="Enter technician name"
          size="large"
        />
      </el-form-item>

      <el-form-item label="Estimated Duration (hours)" prop="estimatedDuration">
        <el-input-number
          v-model="form.estimatedDuration"
          :min="0.5"
          :max="24"
          :step="0.5"
          size="large"
          style="width: 100%"
        />
      </el-form-item>

      <el-form-item label="Priority" prop="priority">
        <el-radio-group v-model="form.priority" size="large">
          <el-radio-button value="low">Low</el-radio-button>
          <el-radio-button value="medium">Medium</el-radio-button>
          <el-radio-button value="high">High</el-radio-button>
          <el-radio-button value="critical">Critical</el-radio-button>
        </el-radio-group>
      </el-form-item>

      <el-form-item label="Notes">
        <el-input
          v-model="form.notes"
          type="textarea"
          :rows="4"
          placeholder="Enter maintenance notes or special instructions..."
        />
      </el-form-item>

      <el-form-item>
        <el-checkbox v-model="form.notifyAssignee">
          Send notification to assigned technician
        </el-checkbox>
      </el-form-item>
    </el-form>

    <template #footer>
      <div class="dialog-footer">
        <button class="btn btn-outline" @click="handleClose">Cancel</button>
        <button class="btn btn-primary" @click="handleSubmit" :disabled="isSubmitting">
          <el-icon v-if="isSubmitting" class="is-loading"><Loading /></el-icon>
          <span>{{ isSubmitting ? 'Scheduling...' : 'Schedule Maintenance' }}</span>
        </button>
      </div>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'
import { useTankStore } from '@/stores/tankStore'
import { tankService } from '@/services/api/tankService'
import { ElMessage } from 'element-plus'
import { Loading } from '@element-plus/icons-vue'
import type { FormInstance } from 'element-plus'

interface Props {
  modelValue: boolean
  tankId?: string
}

interface Emits {
  (e: 'update:modelValue', value: boolean): void
  (e: 'scheduled', data: any): void
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

const tanks = computed(() => tankStore.tanks)

const form = ref({
  tankId: props.tankId || '',
  maintenanceDate: new Date(),
  maintenanceType: 'routine',
  assignedTo: '',
  estimatedDuration: 2,
  priority: 'medium',
  notes: '',
  notifyAssignee: true
})

const rules = {
  tankId: [
    { required: true, message: 'Please select a tank', trigger: 'change' }
  ],
  maintenanceDate: [
    { required: true, message: 'Please select maintenance date', trigger: 'change' }
  ],
  maintenanceType: [
    { required: true, message: 'Please select maintenance type', trigger: 'change' }
  ],
  assignedTo: [
    { required: true, message: 'Please enter technician name', trigger: 'blur' }
  ],
  estimatedDuration: [
    { required: true, message: 'Please enter estimated duration', trigger: 'blur' }
  ],
  priority: [
    { required: true, message: 'Please select priority', trigger: 'change' }
  ]
}

const disabledDate = (date: Date) => {
  // Disable past dates
  return date < new Date(new Date().setHours(0, 0, 0, 0))
}

const handleClose = () => {
  formRef.value?.resetFields()
  form.value = {
    tankId: props.tankId || '',
    maintenanceDate: new Date(),
    maintenanceType: 'routine',
    assignedTo: '',
    estimatedDuration: 2,
    priority: 'medium',
    notes: '',
    notifyAssignee: true
  }
  isVisible.value = false
}

const handleSubmit = async () => {
  if (!formRef.value) return

  await formRef.value.validate(async (valid) => {
    if (valid) {
      isSubmitting.value = true
      try {
        // Call API to schedule maintenance
        await tankService.scheduleMaintenance(
          form.value.tankId,
          form.value.maintenanceDate.toISOString()
        )

        ElMessage.success('Maintenance scheduled successfully')
        
        emit('scheduled', {
          ...form.value,
          maintenanceDate: form.value.maintenanceDate.toISOString()
        })
        
        handleClose()
      } catch (error: any) {
        ElMessage.error(error.message || 'Failed to schedule maintenance')
      } finally {
        isSubmitting.value = false
      }
    }
  })
}

// Watch for tankId prop changes
watch(() => props.tankId, (newValue) => {
  if (newValue) {
    form.value.tankId = newValue
  }
}, { immediate: true })
</script>

<style lang="scss" scoped>
.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: var(--space-3);
}
</style>

