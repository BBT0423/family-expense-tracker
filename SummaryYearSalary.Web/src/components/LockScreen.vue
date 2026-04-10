<template>
  <div v-if="isLocked" class="lock-screen-overlay">
    <div class="lock-screen-container">
      <div class="lock-screen-content">
        <!-- Lock Icon -->
        <div class="lock-icon">
          <svg
            width="64"
            height="64"
            viewBox="0 0 24 24"
            fill="none"
            xmlns="http://www.w3.org/2000/svg"
          >
            <path
              d="M19 11H5C3.89543 11 3 11.8954 3 13V20C3 21.1046 3.89543 22 5 22H19C20.1046 22 21 21.1046 21 20V13C21 11.8954 20.1046 11 19 11Z"
              stroke="currentColor"
              stroke-width="2"
              stroke-linecap="round"
              stroke-linejoin="round"
            />
            <path
              d="M7 11V7C7 5.67392 7.52678 4.40215 8.46447 3.46447C9.40215 2.52678 10.6739 2 12 2C13.3261 2 14.5979 2.52678 15.5355 3.46447C16.4732 4.40215 17 5.67392 17 7V11"
              stroke="currentColor"
              stroke-width="2"
              stroke-linecap="round"
              stroke-linejoin="round"
            />
          </svg>
        </div>

        <!-- User Info -->
        <div class="user-info">
          <div v-if="user?.avatar" class="user-avatar">
            <img :src="user.avatar" :alt="user.name" />
          </div>
          <div v-else class="user-avatar-placeholder">
            {{ userInitials }}
          </div>
          <h2 class="user-name">{{ user?.name || 'User' }}</h2>
          <p class="lock-message">Your session has been locked due to inactivity</p>
        </div>

        <!-- Unlock Form (email users only) -->
        <form v-if="!isPasswordlessUser" class="unlock-form" @submit.prevent="handleUnlock">
          <div class="form-group">
            <label for="password" class="form-label">Password</label>
            <div class="password-input-wrapper">
              <input
                id="password"
                v-model="password"
                :type="showPassword ? 'text' : 'password'"
                class="form-input"
                :class="{ error: error }"
                placeholder="Enter your password"
                :autocomplete="showPassword ? 'off' : 'current-password'"
                :disabled="isUnlocking"
                @input="error = ''"
              />
              <button
                type="button"
                class="toggle-password"
                tabindex="-1"
                @click="showPassword = !showPassword"
              >
                <svg
                  v-if="showPassword"
                  width="20"
                  height="20"
                  viewBox="0 0 24 24"
                  fill="none"
                  xmlns="http://www.w3.org/2000/svg"
                >
                  <path
                    d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"
                    stroke="currentColor"
                    stroke-width="2"
                    stroke-linecap="round"
                    stroke-linejoin="round"
                  />
                  <circle
                    cx="12"
                    cy="12"
                    r="3"
                    stroke="currentColor"
                    stroke-width="2"
                    stroke-linecap="round"
                    stroke-linejoin="round"
                  />
                </svg>
                <svg
                  v-else
                  width="20"
                  height="20"
                  viewBox="0 0 24 24"
                  fill="none"
                  xmlns="http://www.w3.org/2000/svg"
                >
                  <path
                    d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24"
                    stroke="currentColor"
                    stroke-width="2"
                    stroke-linecap="round"
                    stroke-linejoin="round"
                  />
                  <line
                    x1="1"
                    y1="1"
                    x2="23"
                    y2="23"
                    stroke="currentColor"
                    stroke-width="2"
                    stroke-linecap="round"
                    stroke-linejoin="round"
                  />
                </svg>
              </button>
            </div>
            <span v-if="error" class="error-message">{{ error }}</span>
          </div>

          <button type="submit" class="unlock-button" :disabled="!password || isUnlocking">
            <span v-if="isUnlocking" class="spinner"></span>
            <span v-else>Unlock</span>
          </button>
        </form>

        <!-- Passwordless notice (Google / Phone users) -->
        <div v-else class="passwordless-lock-notice">
          <p v-if="authStore.loginProvider === 'google'" class="passwordless-lock-text">
            กรุณาออกจากระบบและ Sign in with Google อีกครั้ง
          </p>
          <p v-else class="passwordless-lock-text">
            กรุณาออกจากระบบและเข้าสู่ระบบด้วยเบอร์โทรอีกครั้ง
          </p>
          <button
            type="button"
            class="unlock-button"
            :disabled="isUnlocking"
            @click="handleLogout"
          >
            ออกจากระบบ
          </button>
        </div>

        <!-- Additional Actions (email users only) -->
        <div v-if="!isPasswordlessUser" class="lock-actions">
          <button class="logout-link" :disabled="isUnlocking" @click="handleLogout">
            Sign in as different user
          </button>
        </div>

        <!-- Lock Time -->
        <div class="lock-time">
          <p>Locked {{ lockedTimeText }}</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onUnmounted } from 'vue';
import { useAuthStore } from '@/stores/authStore';
import { useLockScreenStore } from '@/stores/lockScreenStore';
import { authService } from '@/services/authService';
import { useRouter } from 'vue-router';
import { useGlobalUi } from '@/composables/useGlobalUi';

const props = defineProps<{
  isLocked: boolean;
  onUnlock: () => void;
}>();

const authStore = useAuthStore();
const lockScreenStore = useLockScreenStore();
const router = useRouter();
const { showMessage } = useGlobalUi();

const password = ref('');
const showPassword = ref(false);
const error = ref('');
const isUnlocking = ref(false);
const lockedTimeText = ref('just now');

const user = computed(() => authStore.user);
const isPasswordlessUser = computed(() => authStore.loginProvider !== 'email');

const userInitials = computed(() => {
  if (!user.value?.name) return 'U';
  const names = user.value.name.split(' ');
  if (names.length >= 2) {
    return (names[0][0] + names[1][0]).toUpperCase();
  }
  return user.value.name.substring(0, 2).toUpperCase();
});

// Update locked time text
let timeUpdateInterval: ReturnType<typeof setInterval> | null = null;

const updateLockedTime = () => {
  if (!lockScreenStore.lockTime) {
    lockedTimeText.value = 'just now';
    return;
  }

  const now = Date.now();
  const diff = now - lockScreenStore.lockTime;
  const minutes = Math.floor(diff / 60000);

  if (minutes < 1) {
    lockedTimeText.value = 'just now';
  } else if (minutes === 1) {
    lockedTimeText.value = '1 minute ago';
  } else if (minutes < 60) {
    lockedTimeText.value = `${minutes} minutes ago`;
  } else {
    const hours = Math.floor(minutes / 60);
    if (hours === 1) {
      lockedTimeText.value = '1 hour ago';
    } else {
      lockedTimeText.value = `${hours} hours ago`;
    }
  }
};

watch(
  () => props.isLocked,
  (locked) => {
    if (locked) {
      lockScreenStore.lock();
      password.value = '';
      error.value = '';
      updateLockedTime();

      // Update time every 30 seconds
      timeUpdateInterval = setInterval(updateLockedTime, 30000);
    } else {
      lockScreenStore.unlock();
      if (timeUpdateInterval) {
        clearInterval(timeUpdateInterval);
        timeUpdateInterval = null;
      }
    }
  },
  { immediate: true }
);

onUnmounted(() => {
  if (timeUpdateInterval) {
    clearInterval(timeUpdateInterval);
  }
});

const handleUnlock = async () => {
  if (!password.value || isUnlocking.value) return;

  error.value = '';
  isUnlocking.value = true;

  try {
    // Verify password by attempting login
    const { data } = await authService.login({
      email: user.value?.email || '',
      password: password.value,
    });

    if (data.isSuccess && data.value) {
      // Password is correct, unlock the app
      password.value = '';
      props.onUnlock();
      showMessage('Session unlocked successfully', 'success');
    } else {
      error.value = 'Incorrect password. Please try again.';
    }
  } catch {
    error.value = 'Incorrect password. Please try again.';
  } finally {
    isUnlocking.value = false;
  }
};

const handleLogout = () => {
  authStore.clearAuth();
  lockScreenStore.unlock();
  props.onUnlock(); // Unlock the screen before navigating
  router.push('/authorize/login');
};
</script>

<style scoped>
.lock-screen-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  z-index: 10000;
  display: flex;
  align-items: center;
  justify-content: center;
  animation: fadeIn 0.3s ease-in;
}

@keyframes fadeIn {
  from {
    opacity: 0;
  }
  to {
    opacity: 1;
  }
}

.lock-screen-container {
  width: 100%;
  max-width: 420px;
  padding: 20px;
}

.lock-screen-content {
  background: white;
  border-radius: 16px;
  padding: 40px 32px;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
  animation: slideUp 0.4s ease-out;
}

@keyframes slideUp {
  from {
    transform: translateY(20px);
    opacity: 0;
  }
  to {
    transform: translateY(0);
    opacity: 1;
  }
}

.lock-icon {
  text-align: center;
  margin-bottom: 24px;
  color: #667eea;
}

.user-info {
  text-align: center;
  margin-bottom: 32px;
}

.user-avatar {
  width: 80px;
  height: 80px;
  border-radius: 50%;
  margin: 0 auto 16px;
  overflow: hidden;
  border: 3px solid #667eea;
}

.user-avatar img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.user-avatar-placeholder {
  width: 80px;
  height: 80px;
  border-radius: 50%;
  margin: 0 auto 16px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 32px;
  font-weight: 600;
}

.user-name {
  font-size: 24px;
  font-weight: 600;
  color: #1a202c;
  margin: 0 0 8px;
}

.lock-message {
  color: #718096;
  font-size: 14px;
  margin: 0;
}

.unlock-form {
  margin-bottom: 24px;
}

.passwordless-lock-notice {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 16px;
  padding: 8px 0 24px;
}

.passwordless-lock-text {
  color: #4a5568;
  font-size: 15px;
  text-align: center;
  margin: 0;
  line-height: 1.5;
}

.form-group {
  margin-bottom: 16px;
}

.form-label {
  display: block;
  font-size: 14px;
  font-weight: 500;
  color: #4a5568;
  margin-bottom: 8px;
}

.password-input-wrapper {
  position: relative;
}

.form-input {
  width: 100%;
  padding: 12px 40px 12px 16px;
  font-size: 16px;
  border: 2px solid #e2e8f0;
  border-radius: 8px;
  outline: none;
  transition: border-color 0.2s;
  box-sizing: border-box;
}

.form-input:focus {
  border-color: #667eea;
}

.form-input.error {
  border-color: #f56565;
}

.form-input:disabled {
  background-color: #f7fafc;
  cursor: not-allowed;
}

.toggle-password {
  position: absolute;
  right: 12px;
  top: 50%;
  transform: translateY(-50%);
  background: none;
  border: none;
  color: #718096;
  cursor: pointer;
  padding: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: color 0.2s;
}

.toggle-password:hover {
  color: #4a5568;
}

.error-message {
  display: block;
  color: #f56565;
  font-size: 13px;
  margin-top: 6px;
}

.unlock-button {
  width: 100%;
  padding: 14px;
  font-size: 16px;
  font-weight: 600;
  color: white;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border: none;
  border-radius: 8px;
  cursor: pointer;
  transition:
    transform 0.2s,
    box-shadow 0.2s;
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 50px;
}

.unlock-button:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.4);
}

.unlock-button:active:not(:disabled) {
  transform: translateY(0);
}

.unlock-button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.spinner {
  width: 20px;
  height: 20px;
  border: 3px solid rgba(255, 255, 255, 0.3);
  border-top-color: white;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

.lock-actions {
  text-align: center;
  margin-bottom: 16px;
}

.logout-link {
  background: none;
  border: none;
  color: #667eea;
  font-size: 14px;
  cursor: pointer;
  text-decoration: none;
  transition: color 0.2s;
  padding: 8px;
}

.logout-link:hover:not(:disabled) {
  color: #764ba2;
  text-decoration: underline;
}

.logout-link:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.lock-time {
  text-align: center;
  padding-top: 16px;
  border-top: 1px solid #e2e8f0;
}

.lock-time p {
  color: #a0aec0;
  font-size: 13px;
  margin: 0;
}

/* Mobile responsive */
@media (max-width: 480px) {
  .lock-screen-content {
    padding: 32px 24px;
  }

  .user-name {
    font-size: 20px;
  }

  .lock-message {
    font-size: 13px;
  }
}
</style>
