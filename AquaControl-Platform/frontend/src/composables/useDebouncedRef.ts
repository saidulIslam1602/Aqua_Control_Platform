import { ref, watch } from 'vue'

export function useDebouncedRef<T>(initialValue: T, delay: number = 300) {
  const debouncedValue = ref<T>(initialValue)
  const immediateValue = ref<T>(initialValue)
  
  let timeoutId: ReturnType<typeof setTimeout> | null = null

  const setValue = (value: T) => {
    immediateValue.value = value
    
    if (timeoutId) {
      clearTimeout(timeoutId)
    }
    
    timeoutId = setTimeout(() => {
      debouncedValue.value = value
    }, delay)
  }

  watch(immediateValue, (newValue) => {
    setValue(newValue)
  })

  return {
    debouncedValue,
    immediateValue,
    setValue
  }
}

