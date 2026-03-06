<template>
  <div style="display: flex; align-items: center; gap: 1rem">
    <img alt="user header" src="/innofinite-logo.png" height="31" />
    <div v-if="logo" class="logo-text">{{ logo }}</div>
  </div>
</template>

<script setup lang="ts">
defineProps({
  logo: {
    type: String,
    default: '',
  },
  isDark: {
    type: Boolean,
    default: true,
  },
});
</script>
