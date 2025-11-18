<template>
  <section class="modern-hero">
    <div class="hero-background">
      <div class="gradient-overlay"></div>
      <div class="animated-shapes">
        <div class="shape shape-1"></div>
        <div class="shape shape-2"></div>
        <div class="shape shape-3"></div>
      </div>
    </div>

    <div class="hero-content">
      <div class="hero-container">
        <!-- Badge -->
        <div class="hero-badge animate-fade-in-down">
          <span class="badge-icon">🌊</span>
          <span class="badge-text">{{ badgeText }}</span>
        </div>

        <!-- Main Heading -->
        <h1 class="hero-title animate-fade-in-up">
          <span class="title-line">{{ title }}</span>
          <span v-if="subtitle" class="title-gradient">{{ subtitle }}</span>
        </h1>

        <!-- Description -->
        <p class="hero-description animate-fade-in-up">
          {{ description }}
        </p>

        <!-- CTA Buttons -->
        <div class="hero-actions animate-fade-in-up">
          <button 
            class="btn btn-primary btn-large"
            @click="handlePrimaryAction"
          >
            <span>{{ primaryButtonText }}</span>
            <el-icon><ArrowRight /></el-icon>
          </button>
          <button 
            class="btn btn-secondary btn-large"
            @click="handleSecondaryAction"
          >
            <el-icon><VideoPlay /></el-icon>
            <span>{{ secondaryButtonText }}</span>
          </button>
        </div>

        <!-- Stats -->
        <div v-if="showStats" class="hero-stats animate-fade-in-up">
          <div 
            v-for="stat in stats" 
            :key="stat.label"
            class="stat-item"
          >
            <div class="stat-value">{{ stat.value }}</div>
            <div class="stat-label">{{ stat.label }}</div>
          </div>
        </div>
      </div>
    </div>

    <!-- Scroll Indicator -->
    <div class="scroll-indicator" @click="scrollToContent">
      <div class="scroll-icon">
        <span></span>
      </div>
      <span class="scroll-text">Scroll to explore</span>
    </div>
  </section>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ArrowRight, VideoPlay } from '@element-plus/icons-vue'

// Props
interface Props {
  badgeText?: string
  title: string
  subtitle?: string
  description: string
  primaryButtonText?: string
  secondaryButtonText?: string
  showStats?: boolean
  stats?: Array<{ value: string; label: string }>
}

const props = withDefaults(defineProps<Props>(), {
  badgeText: 'Pioneering Smart Aquaculture',
  primaryButtonText: 'Get Started',
  secondaryButtonText: 'Watch Demo',
  showStats: true,
  stats: () => [
    { value: '24/7', label: 'Real-time Monitoring' },
    { value: '99.9%', label: 'System Uptime' },
    { value: '1000+', label: 'Sensors Connected' }
  ]
})

// Emits
const emit = defineEmits(['primary-action', 'secondary-action'])

// Methods
const handlePrimaryAction = () => {
  emit('primary-action')
}

const handleSecondaryAction = () => {
  emit('secondary-action')
}

const scrollToContent = () => {
  const headerHeight = 80
  window.scrollTo({
    top: window.innerHeight - headerHeight,
    behavior: 'smooth'
  })
}

// Lifecycle
onMounted(() => {
  // Initialize scroll animations
  const observerOptions = {
    threshold: 0.1,
    rootMargin: '0px'
  }

  const observer = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
      if (entry.isIntersecting) {
        entry.target.classList.add('is-visible')
      }
    })
  }, observerOptions)

  document.querySelectorAll('.scroll-animate').forEach(el => {
    observer.observe(el)
  })
})
</script>

<style lang="scss" scoped>
@import '@/styles/design-system/tokens.scss';
@import '@/styles/design-system/animations.scss';

.modern-hero {
  position: relative;
  min-height: calc(100vh - var(--header-height));
  display: flex;
  align-items: center;
  justify-content: center;
  overflow: hidden;
  padding: var(--space-20) 0;

  @media (max-width: 768px) {
    min-height: auto;
    padding: var(--space-16) 0;
  }
}

.hero-background {
  position: absolute;
  inset: 0;
  background: linear-gradient(
    135deg,
    #f8f9fa 0%,
    #e9ecef 50%,
    #dee2e6 100%
  );

  .gradient-overlay {
    position: absolute;
    inset: 0;
    background: linear-gradient(
      135deg,
      rgba(0, 135, 255, 0.05) 0%,
      rgba(0, 195, 175, 0.05) 100%
    );
  }

  .animated-shapes {
    position: absolute;
    inset: 0;
    overflow: hidden;

    .shape {
      position: absolute;
      border-radius: 50%;
      filter: blur(60px);
      opacity: 0.3;
      animation: float 20s ease-in-out infinite;

      &-1 {
        width: 400px;
        height: 400px;
        background: linear-gradient(135deg, var(--color-primary-400), var(--color-primary-600));
        top: -200px;
        left: -100px;
      }

      &-2 {
        width: 500px;
        height: 500px;
        background: linear-gradient(135deg, var(--color-secondary-400), var(--color-secondary-600));
        bottom: -200px;
        right: -150px;
        animation-delay: -10s;
      }

      &-3 {
        width: 300px;
        height: 300px;
        background: linear-gradient(135deg, var(--color-primary-300), var(--color-secondary-400));
        top: 50%;
        left: 50%;
        transform: translate(-50%, -50%);
        animation-delay: -5s;
      }
    }
  }
}

.hero-content {
  position: relative;
  z-index: 1;
  width: 100%;
}

.hero-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 0 var(--container-padding);
  text-align: center;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--space-8);
}

.hero-badge {
  display: inline-flex;
  align-items: center;
  gap: var(--space-2);
  padding: var(--space-2) var(--space-5);
  background: rgba(255, 255, 255, 0.9);
  backdrop-filter: blur(10px);
  border: 1px solid var(--color-neutral-200);
  border-radius: var(--border-radius-full);
  box-shadow: var(--shadow-sm);
  animation-delay: 0ms;

  .badge-icon {
    font-size: var(--font-size-lg);
  }

  .badge-text {
    font-size: var(--font-size-sm);
    font-weight: var(--font-weight-semibold);
    color: var(--color-text-primary);
    letter-spacing: 0.02em;
  }
}

.hero-title {
  font-family: var(--font-family-display);
  font-size: var(--font-size-5xl);
  font-weight: var(--font-weight-extrabold);
  line-height: var(--line-height-tight);
  color: var(--color-text-primary);
  margin: 0;
  max-width: 900px;
  animation-delay: 100ms;

  @media (max-width: 768px) {
    font-size: var(--font-size-4xl);
  }

  .title-line {
    display: block;
    margin-bottom: var(--space-3);
  }

  .title-gradient {
    display: block;
    background: linear-gradient(135deg, var(--color-primary-600), var(--color-secondary-500));
    -webkit-background-clip: text;
    -webkit-text-fill-color: transparent;
    background-clip: text;
  }
}

.hero-description {
  font-size: var(--font-size-xl);
  line-height: var(--line-height-relaxed);
  color: var(--color-text-secondary);
  max-width: 700px;
  margin: 0;
  animation-delay: 200ms;

  @media (max-width: 768px) {
    font-size: var(--font-size-lg);
  }
}

.hero-actions {
  display: flex;
  align-items: center;
  gap: var(--space-4);
  flex-wrap: wrap;
  justify-content: center;
  animation-delay: 300ms;

  @media (max-width: 640px) {
    flex-direction: column;
    width: 100%;

    .btn {
      width: 100%;
    }
  }
}

.btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: var(--space-2);
  padding: var(--button-padding-y) var(--button-padding-x);
  font-size: var(--font-size-base);
  font-weight: var(--font-weight-semibold);
  border-radius: var(--button-radius);
  border: 2px solid transparent;
  cursor: pointer;
  transition: all var(--transition-base);
  text-decoration: none;
  white-space: nowrap;

  &-large {
    padding: var(--space-4) var(--space-8);
    font-size: var(--font-size-lg);
  }

  &-primary {
    background: linear-gradient(135deg, var(--color-primary-600), var(--color-primary-700));
    color: white;
    box-shadow: 0 4px 12px rgba(0, 135, 255, 0.3);

    &:hover {
      transform: translateY(-2px);
      box-shadow: 0 8px 24px rgba(0, 135, 255, 0.4);
    }

    &:active {
      transform: translateY(0);
    }
  }

  &-secondary {
    background: white;
    color: var(--color-text-primary);
    border-color: var(--color-neutral-300);

    &:hover {
      border-color: var(--color-primary-500);
      color: var(--color-primary-600);
      transform: translateY(-2px);
      box-shadow: var(--shadow-lg);
    }
  }
}

.hero-stats {
  display: flex;
  gap: var(--space-12);
  justify-content: center;
  flex-wrap: wrap;
  animation-delay: 400ms;

  @media (max-width: 640px) {
    gap: var(--space-8);
  }
}

.stat-item {
  text-align: center;

  .stat-value {
    font-size: var(--font-size-3xl);
    font-weight: var(--font-weight-bold);
    font-family: var(--font-family-display);
    background: linear-gradient(135deg, var(--color-primary-600), var(--color-secondary-500));
    -webkit-background-clip: text;
    -webkit-text-fill-color: transparent;
    background-clip: text;
    line-height: 1.2;
    margin-bottom: var(--space-2);

    @media (max-width: 640px) {
      font-size: var(--font-size-2xl);
    }
  }

  .stat-label {
    font-size: var(--font-size-sm);
    color: var(--color-text-tertiary);
    font-weight: var(--font-weight-medium);
  }
}

.scroll-indicator {
  position: absolute;
  bottom: var(--space-8);
  left: 50%;
  transform: translateX(-50%);
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--space-2);
  cursor: pointer;
  opacity: 0.6;
  transition: opacity var(--transition-base);

  &:hover {
    opacity: 1;
  }

  .scroll-icon {
    width: 24px;
    height: 40px;
    border: 2px solid var(--color-text-tertiary);
    border-radius: 12px;
    display: flex;
    justify-content: center;
    padding-top: 8px;

    span {
      width: 4px;
      height: 8px;
      background-color: var(--color-text-tertiary);
      border-radius: 2px;
      animation: scroll-down 2s infinite;
    }
  }

  .scroll-text {
    font-size: var(--font-size-xs);
    color: var(--color-text-tertiary);
    text-transform: uppercase;
    letter-spacing: 0.1em;
  }
}

@keyframes scroll-down {
  0% {
    opacity: 0;
    transform: translateY(0);
  }
  50% {
    opacity: 1;
  }
  100% {
    opacity: 0;
    transform: translateY(12px);
  }
}
</style>
