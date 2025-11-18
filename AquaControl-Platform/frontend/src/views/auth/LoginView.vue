<template>
  <div class="login-view">
    <el-card class="login-card">
      <template #header>
        <div class="login-header">
          <h2>AquaControl Platform</h2>
          <p>Sign in to your account</p>
        </div>
      </template>

      <el-form
        ref="loginFormRef"
        :model="loginForm"
        :rules="loginRules"
        @submit.prevent="handleLogin"
      >
        <el-form-item prop="username">
          <el-input
            v-model="loginForm.username"
            placeholder="Username"
            :prefix-icon="User"
            size="large"
          />
        </el-form-item>

        <el-form-item prop="password">
          <el-input
            v-model="loginForm.password"
            type="password"
            placeholder="Password"
            :prefix-icon="Lock"
            size="large"
            show-password
            @keyup.enter="handleLogin"
          />
        </el-form-item>

        <el-form-item>
          <el-button
            type="primary"
            size="large"
            :loading="authStore.isLoading"
            @click="handleLogin"
            style="width: 100%"
          >
            Sign In
          </el-button>
        </el-form-item>
      </el-form>
    </el-card>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'
import { ElMessage, type FormInstance, type FormRules } from 'element-plus'
import { User, Lock } from '@element-plus/icons-vue'

const router = useRouter()
const authStore = useAuthStore()

const loginFormRef = ref<FormInstance>()

const loginForm = ref({
  username: '',
  password: ''
})

const loginRules: FormRules = {
  username: [
    { required: true, message: 'Please enter username', trigger: 'blur' }
  ],
  password: [
    { required: true, message: 'Please enter password', trigger: 'blur' },
    { min: 6, message: 'Password must be at least 6 characters', trigger: 'blur' }
  ]
}

const handleLogin = async () => {
  if (!loginFormRef.value) return

  await loginFormRef.value.validate(async (valid) => {
    if (valid) {
      try {
        const success = await authStore.login({
          username: loginForm.value.username,
          password: loginForm.value.password
        })
        
        if (success) {
          ElMessage.success('Login successful')
          // Redirect to dashboard
          await router.push('/dashboard')
        } else {
          ElMessage.error('Invalid username or password')
        }
      } catch (error: any) {
        console.error('Login error:', error)
        ElMessage.error(error.response?.data?.message || error.message || 'Login failed')
      }
    }
  })
}
</script>

<style lang="scss" scoped>
.login-view {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 100vh;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  
  .login-card {
    width: 400px;
    
    .login-header {
      text-align: center;
      
      h2 {
        margin: 0 0 8px 0;
        color: var(--el-text-color-primary);
      }
      
      p {
        margin: 0;
        color: var(--el-text-color-regular);
        font-size: 14px;
      }
    }
  }
}
</style>

