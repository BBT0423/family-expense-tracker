import { ref, nextTick } from 'vue';
import { useAuthStore } from '@/stores/authStore';
import { STORAGE_APP_LOCKED, STORAGE_LAST_ACTIVITY } from '@/constants/storage';

const IDLE_ACTIVITY_KEY = STORAGE_LAST_ACTIVITY;
const IDLE_LOCKED_KEY = STORAGE_APP_LOCKED;
const DEFAULT_TIMEOUT = 15 * 60 * 1000; // 15 minutes default

export interface IdleTimeoutConfig {
  timeout?: number; // in milliseconds
  events?: string[];
}

/**
 * Composable for handling idle timeout and app locking
 * Works across multiple browser tabs using localStorage
 */
export const useIdleTimeout = (config: IdleTimeoutConfig = {}) => {
  const authStore = useAuthStore();
  const isLocked = ref(false);
  const lastActivity = ref(Date.now());

  const timeout = config.timeout || DEFAULT_TIMEOUT;
  const events = config.events || [
    'mousedown',
    'mousemove',
    'keypress',
    'scroll',
    'touchstart',
    'click',
  ];

  let idleTimer: ReturnType<typeof setTimeout> | null = null;
  let broadcastChannel: BroadcastChannel | null = null;

  /**
   * Update last activity timestamp in localStorage
   */
  const updateActivity = () => {
    const now = Date.now();
    lastActivity.value = now;
    localStorage.setItem(IDLE_ACTIVITY_KEY, now.toString());

    // Broadcast activity to other tabs
    if (broadcastChannel) {
      broadcastChannel.postMessage({ type: 'activity', timestamp: now });
    }

    // Reset timer
    resetTimer();
  };

  /**
   * Lock the application
   */
  const lockApp = async () => {
    if (!authStore.isAuthenticated) return;

    isLocked.value = true;
    localStorage.setItem(IDLE_LOCKED_KEY, 'true');

    // Force Vue reactivity update
    await nextTick();

    // Broadcast lock to other tabs
    if (broadcastChannel) {
      broadcastChannel.postMessage({ type: 'lock' });
    }

    // Clear timer
    if (idleTimer) {
      clearTimeout(idleTimer);
      idleTimer = null;
    }
  };

  /**
   * Unlock the application
   */
  const unlockApp = () => {
    isLocked.value = false;
    localStorage.removeItem(IDLE_LOCKED_KEY);
    updateActivity();

    // Broadcast unlock to other tabs
    if (broadcastChannel) {
      broadcastChannel.postMessage({ type: 'unlock' });
    }
  };

  /**
   * Reset the idle timer
   */
  const resetTimer = () => {
    if (idleTimer) {
      clearTimeout(idleTimer);
    }

    if (!isLocked.value && authStore.isAuthenticated) {
      idleTimer = setTimeout(() => {
        lockApp();
      }, timeout);
    }
  };

  /**
   * Check if the app should be locked based on last activity
   */
  const checkIdleStatus = () => {
    if (!authStore.isAuthenticated) return;

    const storedActivity = localStorage.getItem(IDLE_ACTIVITY_KEY);
    const storedLocked = localStorage.getItem(IDLE_LOCKED_KEY);

    // Check if already locked
    if (storedLocked === 'true') {
      isLocked.value = true;
      return;
    }

    // Check if should be locked based on inactivity
    if (storedActivity) {
      const lastActivityTime = Number.parseInt(storedActivity, 10);
      const timeSinceActivity = Date.now() - lastActivityTime;

      if (timeSinceActivity >= timeout) {
        lockApp();
      } else {
        // Update local state and reset timer
        lastActivity.value = lastActivityTime;
        resetTimer();
      }
    } else {
      // Initialize activity timestamp
      updateActivity();
    }
  };

  /**
   * Handle messages from other tabs
   */
  const handleBroadcastMessage = async (event: MessageEvent) => {
    const { type, timestamp } = event.data;

    switch (type) {
      case 'activity':
        lastActivity.value = timestamp;
        if (isLocked.value) {
          // Don't unlock, but update activity for when unlocked
          localStorage.setItem(IDLE_ACTIVITY_KEY, timestamp.toString());
        } else {
          resetTimer();
        }
        break;
      case 'lock':
        isLocked.value = true;
        await nextTick(); // Force reactivity update
        if (idleTimer) {
          clearTimeout(idleTimer);
          idleTimer = null;
        }
        break;
      case 'unlock':
        isLocked.value = false;
        await nextTick(); // Force reactivity update
        updateActivity();
        break;
    }
  };

  /**
   * Handle storage events from other tabs
   */
  const handleStorageChange = async (event: StorageEvent) => {
    if (event.key === IDLE_LOCKED_KEY) {
      if (event.newValue === 'true') {
        isLocked.value = true;
        await nextTick(); // Force reactivity update
        if (idleTimer) {
          clearTimeout(idleTimer);
          idleTimer = null;
        }
      } else if (event.newValue === null) {
        isLocked.value = false;
        await nextTick(); // Force reactivity update
        updateActivity();
      }
    } else if (event.key === IDLE_ACTIVITY_KEY && event.newValue) {
      const timestamp = Number.parseInt(event.newValue, 10);
      lastActivity.value = timestamp;
      if (!isLocked.value) {
        resetTimer();
      }
    }
  };

  /**
   * Initialize idle timeout tracking
   */
  const init = () => {
    if (!authStore.isAuthenticated) return;

    // Set up BroadcastChannel for cross-tab communication
    try {
      broadcastChannel = new BroadcastChannel('idle-timeout-channel');
      broadcastChannel.onmessage = handleBroadcastMessage;
    } catch {
      console.warn('BroadcastChannel not supported, falling back to storage events');
    }

    // Check initial idle status
    checkIdleStatus();

    // Add activity listeners
    events.forEach((event) => {
      window.addEventListener(event, updateActivity, { passive: true });
    });

    // Listen for storage changes (for tabs that don't support BroadcastChannel)
    window.addEventListener('storage', handleStorageChange);

    // Check periodically if we should lock
    const checkInterval = setInterval(() => {
      if (!authStore.isAuthenticated) {
        clearInterval(checkInterval);
        return;
      }

      // Check if app should be locked based on localStorage state
      const storedLocked = localStorage.getItem(IDLE_LOCKED_KEY);
      if (storedLocked === 'true' && !isLocked.value) {
        isLocked.value = true;
        if (idleTimer) {
          clearTimeout(idleTimer);
          idleTimer = null;
        }
      }

      const storedActivity = localStorage.getItem(IDLE_ACTIVITY_KEY);
      if (storedActivity && !isLocked.value) {
        const lastActivityTime = Number.parseInt(storedActivity, 10);
        const timeSinceActivity = Date.now() - lastActivityTime;

        if (timeSinceActivity >= timeout) {
          lockApp();
        }
      }
    }, 5000); // Check every 5 seconds

    // Store interval for cleanup
    (window as any).__idleCheckInterval = checkInterval;
  };

  /**
   * Clean up event listeners and timers
   */
  const cleanup = () => {
    // Remove activity listeners
    events.forEach((event) => {
      window.removeEventListener(event, updateActivity);
    });

    // Remove storage listener
    window.removeEventListener('storage', handleStorageChange);

    // Clear timer
    if (idleTimer) {
      clearTimeout(idleTimer);
      idleTimer = null;
    }

    // Close broadcast channel
    if (broadcastChannel) {
      broadcastChannel.close();
      broadcastChannel = null;
    }

    // Clear check interval
    if ((window as any).__idleCheckInterval) {
      clearInterval((window as any).__idleCheckInterval);
      delete (window as any).__idleCheckInterval;
    }
  };

  /**
   * Force check lock status (useful for debugging)
   */
  const forceCheckLockStatus = () => {
    checkIdleStatus();
  };

  // Expose to window for debugging
  if (typeof window !== 'undefined') {
    (window as any).__forceCheckLockStatus = forceCheckLockStatus;
  }

  return {
    isLocked,
    lastActivity,
    lockApp,
    unlockApp,
    updateActivity,
    init,
    cleanup,
    forceCheckLockStatus,
  };
};
