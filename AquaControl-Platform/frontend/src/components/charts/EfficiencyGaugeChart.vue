<template>
  <BaseChart :option="chartOption" :height="height" />
</template>

<script setup lang="ts">
import { computed } from 'vue'
import BaseChart from './BaseChart.vue'
import type { EChartsOption } from 'echarts'

interface Props {
  value: number
  title?: string
  height?: string
}

const props = withDefaults(defineProps<Props>(), {
  title: 'System Efficiency',
  height: '350px'
})

const chartOption = computed<EChartsOption>(() => ({
  series: [
    {
      type: 'gauge',
      startAngle: 180,
      endAngle: 0,
      center: ['50%', '75%'],
      radius: '90%',
      min: 0,
      max: 100,
      splitNumber: 10,
      axisLine: {
        lineStyle: {
          width: 8,
          color: [
            [0.3, '#ef4444'],
            [0.7, '#f59e0b'],
            [1, '#10b981']
          ]
        }
      },
      pointer: {
        icon: 'path://M12.8,0.7l12,40.1H0.7L12.8,0.7z',
        length: '12%',
        width: 20,
        offsetCenter: [0, '-60%'],
        itemStyle: {
          color: 'auto'
        }
      },
      axisTick: {
        length: 12,
        lineStyle: {
          color: 'auto',
          width: 2
        }
      },
      splitLine: {
        length: 20,
        lineStyle: {
          color: 'auto',
          width: 5
        }
      },
      axisLabel: {
        color: '#6b7280',
        fontSize: 14,
        distance: -60,
        rotate: 'tangential',
        formatter: function (value: number) {
          if (value === 0) return '0'
          if (value === 100) return '100'
          return ''
        }
      },
      title: {
        offsetCenter: [0, '-10%'],
        fontSize: 18,
        fontWeight: 600,
        color: '#111827'
      },
      detail: {
        fontSize: 40,
        fontWeight: 'bold',
        offsetCenter: [0, '-35%'],
        valueAnimation: true,
        formatter: function (value: number) {
          return Math.round(value) + '%'
        },
        color: 'auto'
      },
      data: [
        {
          value: props.value,
          name: props.title
        }
      ]
    }
  ]
}))
</script>

