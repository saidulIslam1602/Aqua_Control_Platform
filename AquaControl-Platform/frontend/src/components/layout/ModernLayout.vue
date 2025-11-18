<template>
  <div class="modern-layout">
    <!-- Modern Header -->
    <ModernHeader />

    <!-- Main Content Area -->
    <main class="modern-layout__main">
      <slot />
    </main>

    <!-- Modern Footer -->
    <ModernFooter />
  </div>
</template>

<script setup lang="ts">
import { onMounted } from 'vue'
import ModernHeader from './ModernHeader.vue'
import ModernFooter from './ModernFooter.vue'

// Initialize scroll animations on mount
onMounted(() => {
  const observerOptions = {
    threshold: 0.1,
    rootMargin: '0px 0px -50px 0px'
  }

  const observer = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
      if (entry.isIntersecting) {
        entry.target.classList.add('is-visible')
      }
    })
  }, observerOptions)

  // Observe all scroll-animate elements
  document.querySelectorAll('.scroll-animate').forEach(el => {
    observer.observe(el)
  })
})
</script>

<style lang="scss" scoped>
@import '@/styles/design-system/tokens.scss';

.modern-layout {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  background: var(--color-bg-primary);

  &__main {
    flex: 1;
    padding-top: var(--header-height);
    
    // Ensure content flows naturally
    display: flex;
    flex-direction: column;
  }
}
</style>
