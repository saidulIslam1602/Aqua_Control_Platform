<template>
  <div 
    class="modern-card" 
    :class="[
      `modern-card--${variant}`,
      { 'modern-card--hover': hover },
      { 'modern-card--clickable': clickable }
    ]"
    @click="handleClick"
  >
    <!-- Card Image/Media -->
    <div v-if="$slots.media || imageSrc" class="modern-card__media">
      <slot name="media">
        <img 
          v-if="imageSrc" 
          :src="imageSrc" 
          :alt="imageAlt" 
          class="card-image"
        />
      </slot>
      <div v-if="badge" class="card-badge">
        <span>{{ badge }}</span>
      </div>
    </div>

    <!-- Card Header -->
    <div v-if="$slots.header || icon || title" class="modern-card__header">
      <slot name="header">
        <div v-if="icon" class="card-icon" :style="{ '--icon-color': iconColor }">
          <el-icon :size="iconSize">
            <component :is="icon" />
          </el-icon>
        </div>
        <h3 v-if="title" class="card-title">{{ title }}</h3>
        <p v-if="subtitle" class="card-subtitle">{{ subtitle }}</p>
      </slot>
    </div>

    <!-- Card Body -->
    <div class="modern-card__body">
      <slot>
        <p v-if="description" class="card-description">{{ description }}</p>
      </slot>
    </div>

    <!-- Card Footer -->
    <div v-if="$slots.footer || showFooter" class="modern-card__footer">
      <slot name="footer">
        <button v-if="actionText" class="card-action" @click.stop="handleAction">
          <span>{{ actionText }}</span>
          <el-icon><ArrowRight /></el-icon>
        </button>
      </slot>
    </div>

    <!-- Hover Overlay -->
    <div v-if="hover" class="card-hover-overlay"></div>
  </div>
</template>

<script setup lang="ts">
import { ArrowRight } from '@element-plus/icons-vue'
import type { Component } from 'vue'

// Props
interface Props {
  variant?: 'default' | 'feature' | 'product' | 'news' | 'glass'
  imageSrc?: string
  imageAlt?: string
  badge?: string
  icon?: Component
  iconColor?: string
  iconSize?: number
  title?: string
  subtitle?: string
  description?: string
  actionText?: string
  hover?: boolean
  clickable?: boolean
  showFooter?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  variant: 'default',
  iconSize: 32,
  hover: true,
  clickable: false,
  showFooter: false
})

// Emits
const emit = defineEmits(['click', 'action'])

// Methods
const handleClick = () => {
  if (props.clickable) {
    emit('click')
  }
}

const handleAction = () => {
  emit('action')
}
</script>

<style lang="scss" scoped>
@import '@/styles/design-system/tokens.scss';

.modern-card {
  position: relative;
  background: var(--card-bg);
  border: 1px solid var(--card-border);
  border-radius: var(--card-radius);
  overflow: hidden;
  transition: all var(--transition-base);

  &--hover {
    &:hover {
      transform: translateY(-4px);
      box-shadow: var(--card-shadow-hover);
      border-color: var(--color-primary-200);

      .card-hover-overlay {
        opacity: 1;
      }

      .card-action {
        color: var(--color-primary-600);
        
        .el-icon {
          transform: translateX(4px);
        }
      }

      .card-image {
        transform: scale(1.05);
      }
    }
  }

  &--clickable {
    cursor: pointer;
  }

  // Variants
  &--feature {
    padding: var(--space-8);
    text-align: center;

    .card-icon {
      margin: 0 auto var(--space-4);
    }
  }

  &--product {
    .modern-card__media {
      aspect-ratio: 16 / 9;
      background: var(--color-neutral-100);
    }
  }

  &--news {
    display: flex;
    flex-direction: column;
    height: 100%;

    .modern-card__body {
      flex: 1;
    }
  }

  &--glass {
    background: rgba(255, 255, 255, 0.7);
    backdrop-filter: blur(10px);
    border: 1px solid rgba(255, 255, 255, 0.3);
    box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
  }

  // Card Sections
  &__media {
    position: relative;
    overflow: hidden;

    .card-image {
      width: 100%;
      height: 100%;
      object-fit: cover;
      transition: transform var(--transition-slow);
    }

    .card-badge {
      position: absolute;
      top: var(--space-4);
      right: var(--space-4);
      padding: var(--space-2) var(--space-4);
      background: rgba(0, 0, 0, 0.7);
      backdrop-filter: blur(10px);
      color: white;
      font-size: var(--font-size-xs);
      font-weight: var(--font-weight-semibold);
      border-radius: var(--border-radius-full);
      text-transform: uppercase;
      letter-spacing: 0.05em;
    }
  }

  &__header {
    padding: var(--space-6);
    padding-bottom: 0;

    .card-icon {
      width: 64px;
      height: 64px;
      display: flex;
      align-items: center;
      justify-content: center;
      background: linear-gradient(
        135deg, 
        var(--icon-color, var(--color-primary-100)), 
        var(--icon-color, var(--color-primary-50))
      );
      border-radius: var(--border-radius-xl);
      color: var(--icon-color, var(--color-primary-600));
      margin-bottom: var(--space-4);
    }

    .card-title {
      font-size: var(--font-size-xl);
      font-weight: var(--font-weight-semibold);
      font-family: var(--font-family-display);
      color: var(--color-text-primary);
      margin: 0 0 var(--space-2);
      line-height: var(--line-height-snug);
    }

    .card-subtitle {
      font-size: var(--font-size-sm);
      color: var(--color-text-tertiary);
      margin: 0;
      font-weight: var(--font-weight-medium);
      text-transform: uppercase;
      letter-spacing: 0.05em;
    }
  }

  &__body {
    padding: var(--space-6);

    .card-description {
      font-size: var(--font-size-base);
      color: var(--color-text-secondary);
      line-height: var(--line-height-relaxed);
      margin: 0;
    }
  }

  &__footer {
    padding: var(--space-6);
    padding-top: 0;
    border-top: 1px solid transparent;

    .card-action {
      display: inline-flex;
      align-items: center;
      gap: var(--space-2);
      padding: 0;
      background: none;
      border: none;
      color: var(--color-text-secondary);
      font-size: var(--font-size-base);
      font-weight: var(--font-weight-semibold);
      cursor: pointer;
      transition: all var(--transition-fast);

      .el-icon {
        transition: transform var(--transition-fast);
      }

      &:hover {
        color: var(--color-primary-600);
      }
    }
  }
}

.card-hover-overlay {
  position: absolute;
  inset: 0;
  background: linear-gradient(
    135deg,
    rgba(0, 135, 255, 0.03),
    rgba(0, 195, 175, 0.03)
  );
  opacity: 0;
  transition: opacity var(--transition-base);
  pointer-events: none;
}
</style>
