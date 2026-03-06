<template>
  <div :class="['grid', `grid-${columns}`]" :style="`gap: ${gap}px;`">
    <slot />
  </div>
</template>

<script setup lang="ts">
defineProps({
  columns: { type: Number, default: 4 },
  gap: { type: Number, default: 16 },
});
</script>
