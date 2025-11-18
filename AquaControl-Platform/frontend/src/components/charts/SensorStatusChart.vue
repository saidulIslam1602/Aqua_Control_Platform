<template>
  <BaseChart :option="chartOption" :height="height" />
</template>

<script setup lang="ts">
import { computed } from 'vue'
import BaseChart from './BaseChart.vue'
import type { EChartsOption } from 'echarts'

interface Props {
  data: {
    online: number
    offline: number
    error: number
    calibrating: number
  }
  height?: string
}

const props = withDefaults(defineProps<Props>(), {
  height: '350px'
})

const chartOption = computed<EChartsOption>(() => ({
  title: {
    text: 'Sensor Status Overview',
    left: 'center',
    textStyle: {
      fontSize: 18,
      fontWeight: 600,
      color: '#111827'
    }
  },
  tooltip: {
    trigger: 'axis',
    axisPointer: {
      type: 'shadow'
    },
    backgroundColor: 'rgba(255, 255, 255, 0.95)',
    borderColor: '#e5e7eb',
    borderWidth: 1,
    textStyle: {
      color: '#374151'
    }
  },
  legend: {
    data: ['Count'],
    top: 40,
    textStyle: {
      color: '#6b7280'
    }
  },
  grid: {
    left: '3%',
    right: '4%',
    bottom: '3%',
    top: '20%',
    containLabel: true
  },
  xAxis: {
    type: 'category',
    data: ['Online', 'Offline', 'Error', 'Calibrating'],
    axisLine: {
      lineStyle: {
        color: '#e5e7eb'
      }
    },
    axisLabel: {
      color: '#6b7280'
    }
  },
  yAxis: {
    type: 'value',
    name: 'Count',
    nameTextStyle: {
      color: '#6b7280'
    },
    axisLine: {
      lineStyle: {
        color: '#e5e7eb'
      }
    },
    axisLabel: {
      color: '#6b7280'
    },
    splitLine: {
      lineStyle: {
        color: '#f3f4f6',
        type: 'dashed'
      }
    }
  },
  series: [
    {
      name: 'Count',
      type: 'bar',
      data: [
        {
          value: props.data.online,
          itemStyle: { color: '#10b981' }
        },
        {
          value: props.data.offline,
          itemStyle: { color: '#6b7280' }
        },
        {
          value: props.data.error,
          itemStyle: { color: '#ef4444' }
        },
        {
          value: props.data.calibrating,
          itemStyle: { color: '#f59e0b' }
        }
      ],
      barWidth: '60%',
      itemStyle: {
        borderRadius: [8, 8, 0, 0]
      },
      label: {
        show: true,
        position: 'top',
        color: '#111827',
        fontWeight: 600
      },
      emphasis: {
        itemStyle: {
          shadowBlur: 10,
          shadowOffsetX: 0,
          shadowColor: 'rgba(0, 0, 0, 0.2)'
        }
      }
    }
  ]
}))
</script>

