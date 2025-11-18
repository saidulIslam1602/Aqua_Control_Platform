<template>
  <BaseChart :option="chartOption" :height="height" />
</template>

<script setup lang="ts">
import { computed } from 'vue'
import BaseChart from './BaseChart.vue'
import type { EChartsOption } from 'echarts'

interface Props {
  data: {
    excellent: number
    good: number
    fair: number
    poor: number
  }
  height?: string
}

const props = withDefaults(defineProps<Props>(), {
  height: '400px'
})

const chartOption = computed<EChartsOption>(() => ({
  title: {
    text: 'Water Quality Distribution',
    left: 'center',
    textStyle: {
      fontSize: 18,
      fontWeight: 600,
      color: '#111827'
    }
  },
  tooltip: {
    trigger: 'item',
    backgroundColor: 'rgba(255, 255, 255, 0.95)',
    borderColor: '#e5e7eb',
    borderWidth: 1,
    textStyle: {
      color: '#374151'
    },
    formatter: '{a} <br/>{b}: {c} ({d}%)'
  },
  legend: {
    orient: 'vertical',
    right: 20,
    top: 'center',
    textStyle: {
      color: '#6b7280'
    }
  },
  series: [
    {
      name: 'Water Quality',
      type: 'pie',
      radius: ['40%', '70%'],
      center: ['40%', '55%'],
      avoidLabelOverlap: false,
      itemStyle: {
        borderRadius: 10,
        borderColor: '#fff',
        borderWidth: 2
      },
      label: {
        show: false,
        position: 'center'
      },
      emphasis: {
        label: {
          show: true,
          fontSize: 24,
          fontWeight: 'bold',
          color: '#111827'
        }
      },
      labelLine: {
        show: false
      },
      data: [
        { 
          value: props.data.excellent, 
          name: 'Excellent',
          itemStyle: { color: '#10b981' }
        },
        { 
          value: props.data.good, 
          name: 'Good',
          itemStyle: { color: '#3b82f6' }
        },
        { 
          value: props.data.fair, 
          name: 'Fair',
          itemStyle: { color: '#f59e0b' }
        },
        { 
          value: props.data.poor, 
          name: 'Poor',
          itemStyle: { color: '#ef4444' }
        }
      ]
    }
  ]
}))
</script>

