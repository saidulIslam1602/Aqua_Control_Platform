<template>
  <el-dialog
    v-model="isVisible"
    title="Calibrate Sensor"
    width="600px"
    :close-on-click-modal="false"
    @close="handleClose"
  >
    <div v-if="sensor" class="calibration-info">
      <div class="sensor-info">
        <h4>{{ sensor.sensorType }} - {{ sensor.model }}</h4>
        <p>Serial: {{ sensor.serialNumber }}</p>
      </div>
      
      <el-alert
        v-if="sensor.calibrationDate"
        type="info"
        :closable="false"
        show-icon
      >
        Last calibrated: {{ formatDate(sensor.calibrationDate) }}
      </el-alert>
    </div>

    <el-form
      ref="formRef"
      :model="form"
      :rules="rules"
      label-position="top"
    >
      <el-form-item label="Calibration Date" prop="calibrationDate">
        <el-date-picker
          v-model="form.calibrationDate"
          type="datetime"
          placeholder="Select date and time"
          size="large"
          style="width: 100%"
        />
      </el-form-item>

      <el-form-item label="Reference Value" prop="calibrationValue">
        <el-input-number
          v-model="form.calibrationValue"
          :precision="2"
          :step="0.1"
          size="large"
          style="width: 100%"
          placeholder="Enter reference value"
        />
      </el-form-item>

      <el-form-item label="Calibrated By" prop="calibratedBy">
        <el-input
          v-model="form.calibratedBy"
          placeholder="Enter technician name"
          size="large"
        />
      </el-form-item>

      <el-form-item label="Calibration Notes">
        <el-input
          v-model="form.notes"
          type="textarea"
          :rows="4"
          placeholder="Enter calibration notes, observations, or adjustments made..."
        />
      </el-form-item>

      <el-form-item>
        <el-checkbox v-model="form.setNextCalibration">
          Schedule next calibration (in 90 days)
        </el-checkbox>
      </el-form-item>
    </el-form>

    <template #footer>
      <div class="dialog-footer">
        <button class="btn btn-outline" @click="handleClose">Cancel</button>
        <button class="btn btn-primary" @click="handleSubmit" :disabled="isSubmitting">
          <el-icon v-if="isSubmitting" class="is-loading"><Loading /></el-icon>
          <span>{{ isSubmitting ? 'Calibrating...' : 'Calibrate Sensor' }}</span>
        </button>
      </div>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useSensorStore } from '@/stores/sensorStore'
import { ElMessage } from 'element-plus'
import { Loading } from '@element-plus/icons-vue'
import type { FormInstance } from 'element-plus'
import type { Sensor } from '@/types/domain'

interface Props {
  modelValue: boolean
  sensor: Sensor | null
}

interface Emits {
  (e: 'update:modelValue', value: boolean): void
  (e: 'calibrated'): void
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

const sensorStore = useSensorStore()
const formRef = ref<FormInstance>()
const isSubmitting = ref(false)

const isVisible = computed({
  get: () => props.modelValue,
  set: (value) => emit('update:modelValue', value)
})

const form = ref({
  calibrationDate: new Date(),
  calibrationValue: 0,
  calibratedBy: '',
  notes: '',
  setNextCalibration: true
})

const rules = {
  calibrationDate: [
    { required: true, message: 'Please select calibration date', trigger: 'change' }
  ],
  calibrationValue: [
    { required: true, message: 'Please enter reference value', trigger: 'blur' }
  ],
  calibratedBy: [
    { required: true, message: 'Please enter technician name', trigger: 'blur' },
    { min: 2, max: 100, message: 'Name should be 2-100 characters', trigger: 'blur' }
  ]
}

const formatDate = (dateString: string) => {
  return new Date(dateString).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

const handleClose = () => {
  formRef.value?.resetFields()
  form.value = {
    calibrationDate: new Date(),
    calibrationValue: 0,
    calibratedBy: '',
    notes: '',
    setNextCalibration: true
  }
  isVisible.value = false
}

const handleSubmit = async () => {
  if (!formRef.value || !props.sensor) return

  await formRef.value.validate(async (valid) => {
    if (valid) {
      isSubmitting.value = true
      try {
        await sensorStore.calibrateSensor(props.sensor!.id, {
          calibrationDate: form.value.calibrationDate,
          calibratedBy: form.value.calibratedBy,
          calibrationValue: form.value.calibrationValue,
          notes: form.value.notes
        })

        emit('calibrated')
        handleClose()
      } catch (error: any) {
        ElMessage.error(error.message || 'Failed to calibrate sensor')
      } finally {
        isSubmitting.value = false
      }
    }
  })
}
</script>

<style lang="scss" scoped>
.calibration-info {
  margin-bottom: var(--space-6);
  
  .sensor-info {
    margin-bottom: var(--space-4);
    
    h4 {
      font-size: var(--font-size-lg);
      font-weight: var(--font-weight-semibold);
      color: #111827;
      margin: 0 0 var(--space-2);
    }
    
    p {
      font-size: var(--font-size-sm);
      color: #6b7280;
      margin: 0;
    }
  }
}

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: var(--space-3);
}
</style>

