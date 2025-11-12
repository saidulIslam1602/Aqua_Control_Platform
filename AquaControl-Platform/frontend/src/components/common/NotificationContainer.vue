<template>
  <div class="notification-container">
    <transition-group name="notification" tag="div">
      <el-notification
        v-for="notification in notifications"
        :key="notification.id"
        :title="notification.title"
        :message="notification.message"
        :type="notification.type"
        :duration="notification.duration || 4500"
        :show-close="true"
        @close="removeNotification(notification.id)"
        class="notification-item"
      />
    </transition-group>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useNotificationStore } from '@/stores/notificationStore'

const notificationStore = useNotificationStore()

const notifications = computed(() => notificationStore.notifications)

const removeNotification = (id: string) => {
  notificationStore.removeNotification(id)
}
</script>

<style lang="scss" scoped>
.notification-container {
  position: fixed;
  top: 20px;
  right: 20px;
  z-index: 3000;
}

.notification-enter-active,
.notification-leave-active {
  transition: all 0.3s ease;
}

.notification-enter-from {
  opacity: 0;
  transform: translateX(100%);
}

.notification-leave-to {
  opacity: 0;
  transform: translateX(100%);
}
</style>

