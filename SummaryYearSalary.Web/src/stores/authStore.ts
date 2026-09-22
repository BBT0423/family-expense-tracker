import { defineStore } from 'pinia';
import { ref, computed } from 'vue';
import { SecureStorage } from '@/utils/storage';
import {
  STORAGE_TOKEN,
  STORAGE_REFRESH_TOKEN,
  STORAGE_EXPIRES_IN,
  STORAGE_USER,
  STORAGE_APP_LOCKED,
  STORAGE_LAST_ACTIVITY,
  STORAGE_LOGIN_PROVIDER,
} from '@/constants/storage';
import type { LoginResponse, LoginProvider } from '@/types';

export const useAuthStore = defineStore('auth', () => {
  // Auth state
  const token = ref<string | null>(SecureStorage.getItem(STORAGE_TOKEN));
  const refreshToken = ref<string | null>(SecureStorage.getItem(STORAGE_REFRESH_TOKEN));
  const expiresIn = ref<number | null>(
    SecureStorage.getItem(STORAGE_EXPIRES_IN)
      ? Number(SecureStorage.getItem(STORAGE_EXPIRES_IN))
      : null
  );
  const user = ref<LoginResponse['user'] | null>(
    SecureStorage.getItem(STORAGE_USER) ? JSON.parse(SecureStorage.getItem(STORAGE_USER)!) : null
  );
  const loginProvider = ref<LoginProvider | null>(
    localStorage.getItem(STORAGE_LOGIN_PROVIDER) as LoginProvider | null
  );

  const isAuthenticated = computed(() => !!token.value && !!user.value);
  const isAdmin = computed(() => !!user.value?.metadata?.roles?.includes('Admin'));

  /**
   * Redirect URL for post-login navigation
   * Stored in sessionStorage to persist across page reloads (e.g., 401 errors)
   *
   * Best Practices:
   * 1. Only set for PASSIVE logouts (session expired, 401 errors)
   * 2. Never set for ACTIVE logouts (user clicks logout)
   * 3. Clear after successful redirect or on active logout
   * 4. Validate to prevent open redirect vulnerabilities
   * 5. Don't redirect to auth pages (login, forbidden, etc.)
   */

  function setAuth(auth: LoginResponse) {
    token.value = auth.token;
    refreshToken.value = auth.refreshToken;
    expiresIn.value = auth.expiresIn;
    user.value = auth.user;
    const provider = auth.user.loginProvider ?? 'email';
    loginProvider.value = provider;
    localStorage.setItem(STORAGE_LOGIN_PROVIDER, provider);
    localStorage.removeItem(STORAGE_APP_LOCKED);
    SecureStorage.setItem(STORAGE_TOKEN, auth.token);
    SecureStorage.setItem(STORAGE_REFRESH_TOKEN, auth.refreshToken);
    SecureStorage.setItem(STORAGE_EXPIRES_IN, String(auth.expiresIn));
    SecureStorage.setItem(STORAGE_USER, JSON.stringify(auth.user));
  }

  function clearAuth() {
    token.value = null;
    refreshToken.value = null;
    expiresIn.value = null;
    user.value = null;
    loginProvider.value = null;
    SecureStorage.removeItem(STORAGE_TOKEN);
    SecureStorage.removeItem(STORAGE_REFRESH_TOKEN);
    SecureStorage.removeItem(STORAGE_EXPIRES_IN);
    SecureStorage.removeItem(STORAGE_USER);
    SecureStorage.removeItem(STORAGE_APP_LOCKED);
    localStorage.removeItem(STORAGE_LAST_ACTIVITY);
    localStorage.removeItem(STORAGE_LOGIN_PROVIDER);
  }

  // Sync auth state from localStorage (called by storage event listener in App.vue)
  function syncAuthFromStorage() {
    const storedToken = SecureStorage.getItem(STORAGE_TOKEN);
    const storedRefreshToken = SecureStorage.getItem(STORAGE_REFRESH_TOKEN);
    const storedExpiresIn = SecureStorage.getItem(STORAGE_EXPIRES_IN);
    const storedUser = SecureStorage.getItem(STORAGE_USER);

    // Update state
    token.value = storedToken;
    refreshToken.value = storedRefreshToken;
    expiresIn.value = storedExpiresIn ? Number(storedExpiresIn) : null;
    user.value = storedUser ? JSON.parse(storedUser) : null;
    loginProvider.value = localStorage.getItem(STORAGE_LOGIN_PROVIDER) as LoginProvider | null;
  }

  function updateUserMetadata(metadata: Record<string, any>) {
    if (user.value) {
      user.value.metadata = metadata;
      SecureStorage.setItem(STORAGE_USER, JSON.stringify(user.value));
    }
  }

  return {
    token,
    refreshToken,
    expiresIn,
    user,
    loginProvider,
    isAuthenticated,
    isAdmin,
    setAuth,
    clearAuth,
    updateUserMetadata,
    syncAuthFromStorage,
  };
});
