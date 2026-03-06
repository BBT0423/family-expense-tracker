<template>
  <LockScreen :is-locked="isLocked" :on-unlock="unlockApp" />
  <RouterView />
</template>
<script lang="ts" setup>
import { onMounted, onUnmounted } from 'vue';
import { useAuthStore } from '@/stores/authStore';
import { useIdleTimeout } from '@/composables/useIdleTimeout';
import LockScreen from '@/components/LockScreen.vue';
import {
  STORAGE_TOKEN,
  STORAGE_REFRESH_TOKEN,
  STORAGE_EXPIRES_IN,
  STORAGE_USER,
} from '@/constants/storage';
import { getIdleTimeout, isIdleTimeoutEnabled } from '@/constants/idle-timeout';

const authStore = useAuthStore();

// Initialize idle timeout with configuration
const idleTimeoutEnabled = isIdleTimeoutEnabled();
const {
  isLocked,
  init: initIdleTimeout,
  cleanup: cleanupIdleTimeout,
  unlockApp,
} = useIdleTimeout({
  timeout: getIdleTimeout(),
});

// Handle storage events from other tabs
const handleStorageChange = (event: StorageEvent) => {
  // List of auth-related keys
  const authKeys = [STORAGE_TOKEN, STORAGE_REFRESH_TOKEN, STORAGE_EXPIRES_IN, STORAGE_USER];

  if (event.key && authKeys.includes(event.key)) {
    // Sync auth state from storage
    authStore.syncAuthFromStorage();

    // If token was removed (logout in another tab), reload the page
    if (event.key === STORAGE_TOKEN) {
      globalThis.location.reload();
    } else if (event.key === STORAGE_USER && event.newValue) {
      if (authStore.isAuthenticated) {
        // permissionStore.getPermissions()
      }
    }
  }
};

onMounted(async () => {
  // Only fetch permissions if user is authenticated
  if (authStore.isAuthenticated) {
    // await permissionStore.getPermissions()

    // Initialize idle timeout tracking if enabled
    if (idleTimeoutEnabled) {
      initIdleTimeout();
    }
  }

  // Set up storage event listener for cross-tab synchronization
  globalThis.addEventListener('storage', handleStorageChange);
});

onUnmounted(() => {
  // Clean up event listener
  globalThis.removeEventListener('storage', handleStorageChange);

  // Clean up idle timeout
  if (idleTimeoutEnabled) {
    cleanupIdleTimeout();
  }
});
</script>
