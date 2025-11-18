<template>
  <section class="modern-hero" :class="{ 'has-video': videoUrl }">
    <!-- Background Media -->
    <div class="hero-background">
      <!-- Video Background -->
      <video 
        v-if="videoUrl"
        class="hero-video"
        autoplay
        muted
        loop
        playsinline
        :poster="backgroundImage"
      >
        <source :src="videoUrl" type="video/mp4" />
      </video>
      
      <!-- Image Background -->
      <div 
        v-else
        class="hero-image"
        :style="{ backgroundImage: backgroundImage ? `url(${backgroundImage})` : undefined }"
      >
        <!-- Lazy loaded image for better performance -->
        <img
          v-if="backgroundImage"
          :src="backgroundImage"
          :alt="title"
          loading="lazy"
          class="hero-image-lazy"
        />
      </div>
      
      <!-- Animated overlay gradient -->
      <div class="hero-overlay" :class="{ 'overlay-light': overlayLight }"></div>
      
      <!-- Animated particles/bubbles effect -->
      <div class="hero-particles">
        <div v-for="i in 20" :key="i" class="particle" :style="getParticleStyle(i)"></div>
      </div>
    </div>
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
  backgroundImage?: string
  videoUrl?: string
  overlayLight?: boolean
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
  ],
  backgroundImage: 'https://images.unsplash.com/photo-1559827260-dc66d52bef19?q=80&w=2070&auto=format&fit=crop',
  overlayLight: false
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

// Particle animation helper
const getParticleStyle = (index: number) => {
  const delay = Math.random() * 5
  const duration = 8 + Math.random() * 12
  const size = 4 + Math.random() * 8
  const left = Math.random() * 100
  
  return {
    left: `${left}%`,
    width: `${size}px`,
    height: `${size}px`,
    animationDelay: `${delay}s`,
    animationDuration: `${duration}s`
  }
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
  z-index: 0;

  .hero-video,
  .hero-image {
    position: absolute;
    inset: 0;
    width: 100%;
    height: 100%;
    object-fit: cover;
  }

  .hero-image {
    background-size: cover;
    background-position: center;
    background-repeat: no-repeat;
  }

  .hero-image-lazy {
    opacity: 0;
    animation: fadeIn 1s ease-in-out 0.3s forwards;
  }

  .hero-video {
    filter: brightness(0.85);
  }

  .hero-overlay {
    position: absolute;
    inset: 0;
    background: linear-gradient(
      135deg,
      rgba(0, 10, 30, 0.85) 0%,
      rgba(0, 60, 120, 0.75) 50%,
      rgba(0, 100, 150, 0.65) 100%
    );
    backdrop-filter: blur(2px);

    &.overlay-light {
      background: linear-gradient(
        135deg,
        rgba(255, 255, 255, 0.9) 0%,
        rgba(240, 250, 255, 0.85) 100%
      );
    }
  }

  .hero-particles {
    position: absolute;
    inset: 0;
    overflow: hidden;
    pointer-events: none;

    .particle {
      position: absolute;
      bottom: -20px;
      background: rgba(255, 255, 255, 0.3);
      border-radius: 50%;
      animation: floatUp 15s infinite ease-in-out;
      will-change: transform, opacity;

      &:nth-child(odd) {
        animation-timing-function: ease-in-out;
      }

      &:nth-child(even) {
        animation-timing-function: cubic-bezier(0.42, 0, 0.58, 1);
      }
    }
  }
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
  z-index: 10;

  &:hover {
    opacity: 1;
  }

  @media (max-width: 768px) {
    display: none;
  }
}

.scroll-icon {
  width: 24px;
  height: 40px;
  border: 2px solid currentColor;
  border-radius: 12px;
  position: relative;

  span {
    position: absolute;
    top: 8px;
    left: 50%;
    width: 4px;
    height: 8px;
    background: currentColor;
    border-radius: 2px;
    transform: translateX(-50%);
    animation: scrollDown 2s infinite;
  }
}

.scroll-text {
  font-size: var(--font-size-xs);
  text-transform: uppercase;
  letter-spacing: 0.1em;
  font-weight: var(--font-weight-semibold);
}

// Performance-optimized animations
@keyframes floatUp {
  0% {
    transform: translateY(0) translateX(0) scale(1);
    opacity: 0;
  }
  10% {
    opacity: 0.4;
  }
  50% {
    opacity: 0.6;
    transform: translateY(-50vh) translateX(20px) scale(1.2);
  }
  90% {
    opacity: 0.3;
  }
  100% {
    transform: translateY(-100vh) translateX(-20px) scale(0.8);
    opacity: 0;
  }
}

@keyframes fadeIn {
  to {
    opacity: 1;
  }
}

@keyframes scrollDown {
  0% {
    transform: translateX(-50%) translateY(0);
    opacity: 1;
  }
  100% {
    transform: translateX(-50%) translateY(20px);
    opacity: 0;
  }
}

// Hardware acceleration hints for better performance
.hero-video,
.hero-image,
.particle {
  will-change: transform;
  transform: translateZ(0);
  backface-visibility: hidden;
  perspective: 1000px;
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
