<template>
  <div class="nav-section">
    <div class="nav-title">{{ title }}</div>
    <slot />
  </div>
</template>
<script setup lang="ts">
defineProps<{ title: string }>();
</script>
