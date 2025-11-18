<template>
  <footer class="modern-footer">
    <div class="footer-container">
      <!-- Main Footer Content -->
      <div class="footer-main">
        <!-- Brand Column -->
        <div class="footer-column footer-brand">
          <div class="footer-logo">
            <div class="logo-icon">
              <svg viewBox="0 0 40 40" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path d="M20 5L35 15V25L20 35L5 25V15L20 5Z" fill="url(#footer-logo-gradient)"/>
                <path d="M20 12L28 17V23L20 28L12 23V17L20 12Z" fill="white" opacity="0.3"/>
                <defs>
                  <linearGradient id="footer-logo-gradient" x1="5" y1="5" x2="35" y2="35">
                    <stop offset="0%" stop-color="var(--color-primary-500)"/>
                    <stop offset="100%" stop-color="var(--color-secondary-500)"/>
                  </linearGradient>
                </defs>
              </svg>
            </div>
            <span class="logo-text">AquaControl</span>
          </div>
          <p class="footer-description">
            {{ companyDescription }}
          </p>
          <!-- Social Links -->
          <div class="social-links">
            <a 
              v-for="social in socialLinks" 
              :key="social.name"
              :href="social.url"
              :title="social.name"
              class="social-link"
              target="_blank"
              rel="noopener noreferrer"
            >
              <el-icon :size="20">
                <component :is="social.icon" />
              </el-icon>
            </a>
          </div>
        </div>

        <!-- Solutions Column -->
        <div class="footer-column">
          <h3 class="footer-column-title">Solutions</h3>
          <ul class="footer-links">
            <li v-for="link in solutionsLinks" :key="link.label">
              <a :href="link.path" @click.prevent="navigate(link.path)">
                {{ link.label }}
              </a>
            </li>
          </ul>
        </div>

        <!-- Company Column -->
        <div class="footer-column">
          <h3 class="footer-column-title">Company</h3>
          <ul class="footer-links">
            <li v-for="link in companyLinks" :key="link.label">
              <a :href="link.path" @click.prevent="navigate(link.path)">
                {{ link.label }}
              </a>
            </li>
          </ul>
        </div>

        <!-- Resources Column -->
        <div class="footer-column">
          <h3 class="footer-column-title">Resources</h3>
          <ul class="footer-links">
            <li v-for="link in resourceLinks" :key="link.label">
              <a :href="link.path" @click.prevent="navigate(link.path)">
                {{ link.label }}
              </a>
            </li>
          </ul>
        </div>

        <!-- Contact Column -->
        <div class="footer-column">
          <h3 class="footer-column-title">Contact</h3>
          <div class="contact-info">
            <div class="contact-item">
              <el-icon><Location /></el-icon>
              <span>{{ address }}</span>
            </div>
            <div class="contact-item">
              <el-icon><Phone /></el-icon>
              <a :href="`tel:${phone}`">{{ phone }}</a>
            </div>
            <div class="contact-item">
              <el-icon><Message /></el-icon>
              <a :href="`mailto:${email}`">{{ email }}</a>
            </div>
          </div>
        </div>
      </div>

      <!-- Footer Bottom -->
      <div class="footer-bottom">
        <div class="footer-bottom-content">
          <p class="copyright">
            © {{ currentYear }} {{ companyName }}. All rights reserved.
          </p>
          <div class="footer-bottom-links">
            <a href="/privacy-policy" @click.prevent="navigate('/privacy-policy')">Privacy Policy</a>
            <span class="separator">•</span>
            <a href="/terms-of-service" @click.prevent="navigate('/terms-of-service')">Terms of Service</a>
            <span class="separator">•</span>
            <a href="/cookies" @click.prevent="navigate('/cookies')">Cookie Policy</a>
          </div>
        </div>
      </div>
    </div>
  </footer>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { 
  Location, 
  Phone, 
  Message,
  Connection,
  Position,
  VideoPlay
} from '@element-plus/icons-vue'

// Props
interface Props {
  companyName?: string
  companyDescription?: string
  address?: string
  phone?: string
  email?: string
}

withDefaults(defineProps<Props>(), {
  companyName: 'AquaControl Platform',
  companyDescription: 'Advanced aquaculture management system providing real-time monitoring, analytics, and control for modern fish farming operations.',
  address: '123 Innovation Drive, Tech City, TC 12345',
  phone: '+1 (555) 123-4567',
  email: 'info@aquacontrol.com'
})

// Router
const router = useRouter()

// Computed
const currentYear = computed(() => new Date().getFullYear())

// Navigation Links
const solutionsLinks = [
  { label: 'Tank Management', path: '/tanks' },
  { label: 'Sensor Monitoring', path: '/sensors' },
  { label: 'Analytics Dashboard', path: '/analytics' },
  { label: 'Alert System', path: '/alerts' },
  { label: 'Data Engineering', path: '/data-engineering' }
]

const companyLinks = [
  { label: 'About Us', path: '/about' },
  { label: 'Careers', path: '/careers' },
  { label: 'News & Blog', path: '/blog' },
  { label: 'Press Kit', path: '/press' },
  { label: 'Contact', path: '/contact' }
]

const resourceLinks = [
  { label: 'Documentation', path: '/docs' },
  { label: 'API Reference', path: '/api-docs' },
  { label: 'Support Center', path: '/support' },
  { label: 'Community', path: '/community' },
  { label: 'Status', path: '/status' }
]

const socialLinks = [
  { name: 'LinkedIn', url: 'https://linkedin.com', icon: Connection },
  { name: 'Twitter', url: 'https://twitter.com', icon: Position },
  { name: 'YouTube', url: 'https://youtube.com', icon: VideoPlay },
  { name: 'GitHub', url: 'https://github.com', icon: Connection }
]

// Methods
const navigate = (path: string) => {
  router.push(path)
}
</script>

<style lang="scss" scoped>
@import '@/styles/design-system/tokens.scss';

.modern-footer {
  background: linear-gradient(180deg, var(--color-bg-dark) 0%, #051320 100%);
  color: var(--color-text-inverse);
  padding: var(--space-20) 0 var(--space-8);
  margin-top: var(--section-spacing-lg);

  @media (max-width: 768px) {
    padding: var(--space-16) 0 var(--space-6);
  }
}

.footer-container {
  max-width: var(--container-max-width);
  margin: 0 auto;
  padding: 0 var(--container-padding);
}

.footer-main {
  display: grid;
  grid-template-columns: 2fr repeat(4, 1fr);
  gap: var(--space-12);
  margin-bottom: var(--space-16);

  @media (max-width: 1200px) {
    grid-template-columns: 2fr repeat(3, 1fr);
    
    .footer-column:last-child {
      grid-column: 1 / -1;
    }
  }

  @media (max-width: 768px) {
    grid-template-columns: 1fr;
    gap: var(--space-10);
  }
}

.footer-column {
  &-title {
    font-size: var(--font-size-base);
    font-weight: var(--font-weight-semibold);
    font-family: var(--font-family-display);
    color: var(--color-text-inverse);
    margin: 0 0 var(--space-6);
    text-transform: uppercase;
    letter-spacing: 0.05em;
  }
}

.footer-brand {
  .footer-logo {
    display: flex;
    align-items: center;
    gap: var(--space-3);
    margin-bottom: var(--space-4);

    .logo-icon {
      width: 40px;
      height: 40px;

      svg {
        width: 100%;
        height: 100%;
      }
    }

    .logo-text {
      font-size: var(--font-size-xl);
      font-weight: var(--font-weight-bold);
      font-family: var(--font-family-display);
      color: var(--color-text-inverse);
    }
  }

  .footer-description {
    font-size: var(--font-size-sm);
    line-height: var(--line-height-relaxed);
    color: rgba(255, 255, 255, 0.7);
    margin: 0 0 var(--space-6);
    max-width: 400px;
  }
}

.social-links {
  display: flex;
  gap: var(--space-3);
}

.social-link {
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(255, 255, 255, 0.1);
  border-radius: var(--border-radius-lg);
  color: rgba(255, 255, 255, 0.7);
  transition: all var(--transition-fast);

  &:hover {
    background: rgba(255, 255, 255, 0.2);
    color: var(--color-text-inverse);
    transform: translateY(-2px);
  }
}

.footer-links {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: var(--space-3);

  li {
    margin: 0;
  }

  a {
    color: rgba(255, 255, 255, 0.7);
    text-decoration: none;
    font-size: var(--font-size-sm);
    transition: color var(--transition-fast);
    display: inline-block;

    &:hover {
      color: var(--color-text-inverse);
      transform: translateX(4px);
    }
  }
}

.contact-info {
  display: flex;
  flex-direction: column;
  gap: var(--space-4);
}

.contact-item {
  display: flex;
  align-items: flex-start;
  gap: var(--space-3);
  font-size: var(--font-size-sm);
  color: rgba(255, 255, 255, 0.7);

  .el-icon {
    margin-top: 2px;
    flex-shrink: 0;
  }

  a {
    color: rgba(255, 255, 255, 0.7);
    text-decoration: none;
    transition: color var(--transition-fast);

    &:hover {
      color: var(--color-text-inverse);
    }
  }
}

.footer-bottom {
  padding-top: var(--space-8);
  border-top: 1px solid rgba(255, 255, 255, 0.1);

  &-content {
    display: flex;
    justify-content: space-between;
    align-items: center;
    flex-wrap: wrap;
    gap: var(--space-6);

    @media (max-width: 768px) {
      flex-direction: column;
      text-align: center;
    }
  }

  &-links {
    display: flex;
    align-items: center;
    gap: var(--space-4);
    font-size: var(--font-size-sm);

    @media (max-width: 640px) {
      flex-wrap: wrap;
      justify-content: center;
    }

    a {
      color: rgba(255, 255, 255, 0.7);
      text-decoration: none;
      transition: color var(--transition-fast);

      &:hover {
        color: var(--color-text-inverse);
      }
    }

    .separator {
      color: rgba(255, 255, 255, 0.3);
    }
  }
}

.copyright {
  font-size: var(--font-size-sm);
  color: rgba(255, 255, 255, 0.5);
  margin: 0;
}
</style>
