import { ref, computed } from 'vue'

interface LoadingState {
  [key: string]: boolean
}

export function useLoadingState() {
  const loadingStates = ref<LoadingState>({})
  
  const isLoading = computed(() => Object.values(loadingStates.value).some(state => state))
  
  const setLoading = (key: string, loading: boolean): void => {
    loadingStates.value[key] = loading
  }
  
  const isLoadingKey = (key: string): boolean => {
    return loadingStates.value[key] || false
  }
  
  const clearLoading = (): void => {
    loadingStates.value = {}
  }
  
  const withLoading = async <T>(
    key: string, 
    asyncFunction: () => Promise<T>
  ): Promise<T> => {
    try {
      setLoading(key, true)
      return await asyncFunction()
    } finally {
      setLoading(key, false)
    }
  }
  
  return {
    isLoading: readonly(isLoading),
    loadingStates: readonly(loadingStates),
    setLoading,
    isLoadingKey,
    clearLoading,
    withLoading
  }
}

function readonly<T>(ref: any): T {
  return ref as T
}
