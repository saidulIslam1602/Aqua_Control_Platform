// Global type declarations

declare global {
  interface Window {
    gtag?: (
      command: 'config' | 'event' | 'timing_complete',
      targetId: string,
      config?: Record<string, any>
    ) => void;
  }
}

// Vue app config extensions
declare module '@vue/runtime-core' {
  interface ComponentCustomProperties {
    $isDev: boolean;
    $isProd: boolean;
    $notify: (options: {
      title: string;
      message: string;
      type: 'success' | 'error' | 'warning' | 'info';
      duration: number;
    }) => void;
  }
}

// Performance API extensions
interface PerformanceNavigationTiming {
  navigationStart?: number;
}

interface PerformanceEntry {
  processingStart?: number;
}

export {}
