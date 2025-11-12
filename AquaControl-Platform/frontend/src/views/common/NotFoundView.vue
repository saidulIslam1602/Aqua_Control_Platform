<template>
  <div class="not-found-view">
    <el-result
      icon="error"
      title="404"
      sub-title="Page Not Found"
    >
      <template #extra>
        <el-button type="primary" @click="goHome">Back Home</el-button>
      </template>
    </el-result>
  </div>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router'

const router = useRouter()

const goHome = () => {
  router.push('/')
}
</script>

<style lang="scss" scoped>
.not-found-view {
  display: flex;
  align-items: center;
  justify-content: center;
  height: 100%;
}
</style>

