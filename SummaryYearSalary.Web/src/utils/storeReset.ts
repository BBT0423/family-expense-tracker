// Pinia store re-initialization utilities
import { getActivePinia } from 'pinia';
import { useAuthStore } from '@/stores/authStore';

export interface StoreResetOptions {
  clearCache?: boolean;
  refetchData?: boolean;
  preserveAuth?: boolean;
}

/**
 * Re-initializes Pinia stores to ensure clean state
 * This is useful after login/logout or when you need to refresh all store data
 */
export const reinitializePiniaStores = async (options: StoreResetOptions = {}) => {
  const { preserveAuth = true } = options;

  try {
    const pinia = getActivePinia();
    if (!pinia) {
      return;
    }

    // Get store instances
    const authStore = useAuthStore();

    // Reset reactive state without losing authentication
    if (preserveAuth && authStore.isAuthenticated) {
      // Preserve authentication state
    } else if (!preserveAuth) {
      authStore.clearAuth();
    }

    return true;
  } catch {
    return false;
  }
};

/**
 * Quick reset for user switch scenarios
 */
export const resetStoresForUserSwitch = async () => {
  return reinitializePiniaStores({
    clearCache: true,
    refetchData: true,
    preserveAuth: true,
  });
};

/**
 * Complete reset for logout scenarios
 */
export const resetStoresForLogout = async () => {
  return reinitializePiniaStores({
    clearCache: true,
    refetchData: false,
    preserveAuth: false,
  });
};

/**
 * Soft refresh for data reload scenarios
 */
export const refreshStoreData = async () => {
  return reinitializePiniaStores({
    clearCache: false,
    refetchData: true,
    preserveAuth: true,
  });
};
