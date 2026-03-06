<template>
  <output v-if="isOffline || isPoorConnection" class="network-banner" aria-live="polite">
    <span v-if="isOffline">You are offline. Some features may be unavailable.</span>
    <span v-else>Your connection is poor. You may experience delays.</span>
  </output>
</template>

<script lang="ts" setup>
import { useNetworkStatus } from '@/composables/useNetworkStatus';
const { isOffline, isPoorConnection } = useNetworkStatus();
</script>

<style scoped>
.network-banner {
  position: sticky;
  top: 0;
  z-index: 9999;
  width: 100%;
  padding: 10px 16px;
  text-align: center;
  font-size: 14px;
  background: #ffe8a1;
  color: #5c4100;
  border-bottom: 1px solid #f2c94c;
}
</style>
