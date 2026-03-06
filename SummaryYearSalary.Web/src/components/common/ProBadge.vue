<template>
  <span class="pro-badge" :class="badgeClass" :style="badgeStyle">
    <span v-if="showDot" class="status-dot" :class="badgeClass" :style="badgeStyle"></span>
    <slot>{{ statusText }}</slot>
  </span>
</template>

<script setup lang="ts">
import { computed } from 'vue';

const props = defineProps<{
  statusId?: number | null;
  statusBgColor?: string;
  statusBorderColor?: string;
  statusFontColor?: string;
  statusFontSize?: number;
  text?: string;
  hasDot?: boolean;
}>();

const showDot = computed(() => props.hasDot);

const statusClassMap = {
  1: 'daylight',
  2: 'midnight',
  100: 'warning',
  150: 'danger',
  200: 'info',
  300: 'warning',
  400: 'warning',
  500: 'success',
  2000: 'cancelled',
} as const;

const statusTextMap = {
  1: 'Day Light',
  2: 'Mid-night',
  100: 'Pending Approval',
  150: 'Rejected',
  200: 'Approved',
  300: 'Preparing',
  400: 'Wait Receiver',
  500: 'Completed',
  2000: 'Cancelled',
} as const;

const badgeClass = computed(() => {
  if (props.statusBgColor || props.statusFontColor || props.statusBorderColor) {
    return {};
  }
  const id = props.statusId;
  if (id == null) {
    return { info: true };
  }
  const statusClass = statusClassMap[id as keyof typeof statusClassMap] || 'info';
  return { [statusClass]: true };
});

const badgeStyle = computed(() => {
  const style: Record<string, string> = {};
  if (props.statusBgColor) style.background = props.statusBgColor;
  if (props.statusBorderColor) style.borderColor = props.statusBorderColor;
  if (props.statusFontColor) style.color = props.statusFontColor;
  if (props.statusFontSize) style.fontSize = props.statusFontSize + 'px';
  return style;
});

const statusText = computed(() => {
  if (props.text) return props.text;
  const id = props.statusId;
  if (id == null) return 'Unknown Status';
  return statusTextMap[id as keyof typeof statusTextMap] || 'Unknown';
});
</script>

<style scoped>
.pro-badge {
  display: inline-flex;
  align-items: center;
  font-size: 0.8em;
  padding: 0.2em 0.7em;
  border-radius: 1em;
  font-weight: 500;
  background: var(--badge-bg, #f5f5f5);
  white-space: nowrap;
  border: 1px solid #bebebe;
}

.status-dot {
  width: 0.7em;
  height: 0.7em;
  border-radius: 50%;
  margin-right: 0.5em;
  display: inline-block;
  background: #bbb;
}

.success {
  background: #d1fae5;
  color: #065f46;
}

.success .status-dot {
  background: #065f46;
}

.warning {
  background: #fef3c7;
  color: #78350f;
}

.warning .status-dot {
  background: #78350f;
}

.danger {
  background: #fee2e2;
  color: #7f1d1d;
}

.danger .status-dot {
  background: #7f1d1d;
}

.info {
  background: #dbeafe;
  color: #1e40af;
}

.info .status-dot {
  background: #1e40af;
}

.cancelled {
  background: #f3f4f6;
  color: #374151;
}

.cancelled .status-dot {
  background: #374151;
}

.daylight {
  background: #fef9c3;
  color: #78350f;
}

.daylight .status-dot {
  background: #78350f;
}

.midnight {
  background: #e0e7ff;
  color: #312e81;
}

.midnight .status-dot {
  background: #312e81;
}
</style>
