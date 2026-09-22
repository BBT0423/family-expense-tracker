<template>
  <div class="app-progress">
    <div v-if="label || (showPercent && !indeterminate)" class="app-progress__header">
      <span v-if="label" class="app-progress__label">{{ label }}</span>
      <span v-if="showPercent && !indeterminate" class="app-progress__pct" :class="pctClass">
        {{ clampedValue }}%
      </span>
    </div>
    <div class="app-progress__track">
      <div
        class="app-progress__bar"
        :class="[
          `app-progress__bar--${status}`,
          { 'app-progress__bar--indeterminate': indeterminate },
        ]"
        :style="indeterminate ? undefined : { width: clampedValue + '%' }"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';

const props = withDefaults(
  defineProps<{
    modelValue?: number;
    indeterminate?: boolean;
    label?: string;
    showPercent?: boolean;
    status?: 'normal' | 'success' | 'exception';
  }>(),
  {
    modelValue: 0,
    indeterminate: false,
    showPercent: true,
    status: 'normal',
    label: '',
  }
);

const clampedValue = computed(() => Math.min(100, Math.max(0, Math.round(props.modelValue))));

const pctClass = computed(() => ({
  'app-progress__pct--success': props.status === 'success',
  'app-progress__pct--exception': props.status === 'exception',
}));
</script>

<style scoped>
.app-progress {
  width: 100%;
}

.app-progress__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 6px;
}

.app-progress__label {
  font-size: 12px;
  color: #909399;
}

.app-progress__pct {
  font-size: 12px;
  font-weight: 600;
  color: #409eff;
}
.app-progress__pct--success {
  color: #67c23a;
}
.app-progress__pct--exception {
  color: #f56c6c;
}

.app-progress__track {
  width: 100%;
  height: 6px;
  background: #ebeef5;
  border-radius: 100px;
  overflow: hidden;
}

.app-progress__bar {
  height: 100%;
  border-radius: 100px;
  background: #409eff;
  transition: width 0.4s ease;
}

.app-progress__bar--success {
  background: #67c23a;
}

.app-progress__bar--exception {
  background: #f56c6c;
}

.app-progress__bar--indeterminate {
  animation: progressPulse 1.4s ease-in-out infinite;
}

@keyframes progressPulse {
  0% {
    width: 15%;
    margin-left: 0%;
  }
  50% {
    width: 40%;
    margin-left: 40%;
  }
  100% {
    width: 15%;
    margin-left: 0%;
  }
}
</style>
