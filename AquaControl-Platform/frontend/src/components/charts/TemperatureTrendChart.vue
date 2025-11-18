<template>
  <BaseChart :option="chartOption" :height="height" />
</template>

<script setup lang="ts">
import { computed } from 'vue'
import BaseChart from './BaseChart.vue'
import type { EChartsOption } from 'echarts'

interface Props {
  data: {
    timestamps: string[]
    temperatures: number[]
    minThreshold?: number
    maxThreshold?: number
  }
  height?: string
}

const props = withDefaults(defineProps<Props>(), {
  height: '400px'
})

const chartOption = computed<EChartsOption>(() => ({
  title: {
    text: 'Temperature Trend',
    left: 'center',
    textStyle: {
      fontSize: 18,
      fontWeight: 600,
      color: '#111827'
    }
  },
  tooltip: {
    trigger: 'axis',
    backgroundColor: 'rgba(255, 255, 255, 0.95)',
    borderColor: '#e5e7eb',
    borderWidth: 1,
    textStyle: {
      color: '#374151'
    },
    formatter: (params: any) => {
      const data = params[0]
      return `
        <div style="padding: 8px;">
          <div style="font-weight: 600; margin-bottom: 4px;">${data.axisValue}</div>
          <div style="display: flex; align-items: center; gap: 8px;">
            <span style="display: inline-block; width: 10px; height: 10px; border-radius: 50%; background: ${data.color};"></span>
            <span>${data.seriesName}: ${data.value}°C</span>
          </div>
        </div>
      `
    }
  },
  legend: {
    data: ['Temperature', 'Min Threshold', 'Max Threshold'],
    top: 40,
    textStyle: {
      color: '#6b7280'
    }
  },
  grid: {
    left: '3%',
    right: '4%',
    bottom: '10%',
    top: '20%',
    containLabel: true
  },
  xAxis: {
    type: 'category',
    boundaryGap: false,
    data: props.data.timestamps,
    axisLine: {
      lineStyle: {
        color: '#e5e7eb'
      }
    },
    axisLabel: {
      color: '#6b7280',
      rotate: 45
    }
  },
  yAxis: {
    type: 'value',
    name: 'Temperature (°C)',
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
      name: 'Temperature',
      type: 'line',
      smooth: true,
      data: props.data.temperatures,
      lineStyle: {
        width: 3,
        color: '#3b82f6'
      },
      itemStyle: {
        color: '#3b82f6'
      },
      areaStyle: {
        color: {
          type: 'linear',
          x: 0,
          y: 0,
          x2: 0,
          y2: 1,
          colorStops: [
            { offset: 0, color: 'rgba(59, 130, 246, 0.3)' },
            { offset: 1, color: 'rgba(59, 130, 246, 0.05)' }
          ]
        }
      },
      emphasis: {
        focus: 'series'
      }
    },
    ...(props.data.minThreshold ? [{
      name: 'Min Threshold',
      type: 'line',
      data: new Array(props.data.timestamps.length).fill(props.data.minThreshold),
      lineStyle: {
        type: 'dashed',
        color: '#f59e0b',
        width: 2
      },
      itemStyle: {
        color: '#f59e0b'
      },
      symbol: 'none'
    }] : []),
    ...(props.data.maxThreshold ? [{
      name: 'Max Threshold',
      type: 'line',
      data: new Array(props.data.timestamps.length).fill(props.data.maxThreshold),
      lineStyle: {
        type: 'dashed',
        color: '#ef4444',
        width: 2
      },
      itemStyle: {
        color: '#ef4444'
      },
      symbol: 'none'
    }] : [])
  ],
  dataZoom: [
    {
      type: 'inside',
      start: 0,
      end: 100
    },
    {
      start: 0,
      end: 100,
      height: 30,
      bottom: 10
    }
  ]
}))
</script>

