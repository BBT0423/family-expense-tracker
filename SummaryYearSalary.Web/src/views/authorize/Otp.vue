<template>
  <div class="auth-otp-container">
    <div class="auth-logo">
      <div class="otp-icon">
        <svg
          xmlns="http://www.w3.org/2000/svg"
          width="64"
          height="64"
          viewBox="0 0 24 24"
          fill="none"
          stroke="currentColor"
          stroke-width="2"
          stroke-linecap="round"
          stroke-linejoin="round"
        >
          <rect x="3" y="11" width="18" height="11" rx="2" ry="2"></rect>
          <path d="M7 11V7a5 5 0 0 1 10 0v4"></path>
        </svg>
      </div>
      <h1 class="auth-welcome-text">Two-Factor Authentication</h1>
      <p class="auth-subtitle">Enter the 6-digit code from your authenticator app</p>
    </div>

    <ProMessage />

    <form class="otp-form" @submit.prevent="verifyOtp">
      <div class="otp-input-group">
        <input
          v-for="(digit, index) in otpDigits"
          :key="index"
          :ref="(el) => (otpInputs[index] = el as HTMLInputElement)"
          v-model="otpDigits[index]"
          type="text"
          inputmode="numeric"
          maxlength="1"
          class="otp-input"
          :class="{ 'has-error': hasError }"
          autocomplete="off"
          :disabled="loading"
          @input="handleInput(index, $event)"
          @keydown="handleKeydown(index, $event)"
          @paste="handlePaste($event)"
        />
      </div>

      <div v-if="hasError" class="form-error text-center">
        {{ errorMessage }}
      </div>

      <button
        type="submit"
        class="btn btn-primary btn-full"
        :class="{ 'btn-loading': loading }"
        :disabled="loading || !isOtpComplete"
      >
        <span v-if="loading">Verifying...</span>
        <span v-else>Verify Code</span>
      </button>

      <div class="otp-help">
        <p class="help-text">Can't access your authenticator?</p>
        <button type="button" class="btn-link" @click="goBackToLogin">Back to Login</button>
      </div>
    </form>

    <div class="app-version">v{{ appVersion }}</div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, reactive } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { useGlobalUi } from '@/composables/useGlobalUi';
import { useAuthStore } from '@/stores/authStore';
import { authService } from '@/services/authService';
import { toProblemDetails } from '@/utils/utilities';
import { APP_VERSION } from '@/constants/version';
import { STORAGE_LAST_ACTIVITY } from '@/constants/storage';

const router = useRouter();
const route = useRoute();
const { loading, showLoading, hideLoading, showMessage } = useGlobalUi();
const { setAuth } = useAuthStore();

const appVersion = APP_VERSION;
const otpDigits = reactive<string[]>(Array(6).fill(''));
const otpInputs = ref<HTMLInputElement[]>([]);
const hasError = ref(false);
const errorMessage = ref('');

// Get userId from route query (passed from login)
const userId = computed(() => (route.query.userId as string) || '');

const isOtpComplete = computed(() => {
  return otpDigits.every((digit) => digit.length === 1 && /^\d$/.test(digit));
});

const otpCode = computed(() => otpDigits.join(''));

onMounted(() => {
  // Check if userId exists
  if (!userId.value) {
    showMessage('Invalid access. Please login again.', 'error');
    router.push('/authorize/login');
    return;
  }

  // Focus first input
  if (otpInputs.value[0]) {
    otpInputs.value[0].focus();
  }
});

const handleInput = (index: number, event: Event) => {
  const input = event.target as HTMLInputElement;
  const value = input.value;

  // Reset error state when user types
  hasError.value = false;
  errorMessage.value = '';

  // Only allow digits
  if (value && !/^\d$/.test(value)) {
    otpDigits[index] = '';
    return;
  }

  // Move to next input if digit is entered
  if (value && index < 5) {
    otpInputs.value[index + 1]?.focus();
  }
};

const handleKeydown = (index: number, event: KeyboardEvent) => {
  // Handle backspace
  if (event.key === 'Backspace') {
    if (!otpDigits[index] && index > 0) {
      // Move to previous input if current is empty
      otpInputs.value[index - 1]?.focus();
    }
  }
  // Handle arrow keys
  else if (event.key === 'ArrowLeft' && index > 0) {
    otpInputs.value[index - 1]?.focus();
  } else if (event.key === 'ArrowRight' && index < 5) {
    otpInputs.value[index + 1]?.focus();
  }
};

const handlePaste = (event: ClipboardEvent) => {
  event.preventDefault();
  const pastedData = event.clipboardData?.getData('text') || '';
  const digits = pastedData.replace(/\D/g, '').slice(0, 6);

  if (digits.length === 6) {
    digits.split('').forEach((digit, index) => {
      otpDigits[index] = digit;
    });
    // Focus last input
    otpInputs.value[5]?.focus();
    hasError.value = false;
    errorMessage.value = '';
  }
};

const verifyOtp = async () => {
  if (!isOtpComplete.value) {
    hasError.value = true;
    errorMessage.value = 'Please enter all 6 digits';
    return;
  }

  showLoading();
  try {
    const { data } = await authService.verifyMfa({
      mfaCode: otpCode.value,
      userId: userId.value,
    });

    if (data.isSuccess && data.value) {
      // Set auth state
      setAuth({ ...data.value, user: { ...data.value.user, loginProvider: 'email' } });

      localStorage.removeItem(STORAGE_LAST_ACTIVITY);

      // Navigate to admin pages
      showMessage('Login successful', 'success');
      await router.push('/pages');
      globalThis.location.reload();
    } else {
      // Handle verification failure
      hasError.value = true;
      errorMessage.value = data.error ?? 'Invalid verification code';

      // Clear inputs and focus first
      otpDigits.forEach((_, index) => {
        otpDigits[index] = '';
      });
      otpInputs.value[0]?.focus();
    }
  } catch (e: any) {
    console.error('❌ OTP verification error:', e);
    hasError.value = true;
    const problem = toProblemDetails(e);
    errorMessage.value = problem.detail || 'Verification failed. Please try again.';

    // Clear inputs and focus first
    otpDigits.forEach((_, index) => {
      otpDigits[index] = '';
    });
    otpInputs.value[0]?.focus();
  } finally {
    hideLoading();
  }
};

const goBackToLogin = () => {
  router.push('/authorize/login');
};
</script>

<style scoped>
.auth-otp-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: 100vh;
  padding: 2rem;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  position: relative;
  overflow: hidden;
}

.auth-otp-container::before,
.auth-otp-container::after {
  content: '';
  position: absolute;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.05);
  pointer-events: none;
}

.auth-otp-container::before {
  width: 400px;
  height: 400px;
  top: -150px;
  left: -150px;
}

.auth-otp-container::after {
  width: 300px;
  height: 300px;
  bottom: -100px;
  right: -100px;
}

.auth-logo {
  text-align: center;
  margin-bottom: 2.5rem;
}

.otp-icon {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 80px;
  height: 80px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border-radius: 20px;
  margin-bottom: 1.5rem;
  color: white;
  box-shadow: 0 10px 25px rgba(102, 126, 234, 0.3);
}

.auth-welcome-text {
  font-size: 1.8rem;
  font-weight: 700;
  color: #ffffff;
  margin-bottom: 0.5rem;
  text-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.auth-subtitle {
  font-size: 1rem;
  color: rgba(255, 255, 255, 0.9);
  margin: 0;
}

.otp-form {
  width: 100%;
  max-width: 450px;
}

.otp-input-group {
  display: flex;
  gap: 0.75rem;
  justify-content: center;
  margin-bottom: 1.5rem;
}

.otp-input {
  width: 56px;
  height: 64px;
  font-size: 1.75rem;
  font-weight: 600;
  text-align: center;
  border: 2px solid #e2e8f0;
  border-radius: 12px;
  background: white;
  color: #1a202c;
  transition: all 0.2s ease;
  outline: none;
}

.otp-input:focus {
  border-color: #667eea;
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
  transform: translateY(-2px);
}

.otp-input.has-error {
  border-color: #e53e3e;
  animation: shake 0.3s ease;
}

@keyframes shake {
  0%,
  100% {
    transform: translateX(0);
  }
  25% {
    transform: translateX(-5px);
  }
  75% {
    transform: translateX(5px);
  }
}

.otp-input:disabled {
  background-color: #f7fafc;
  cursor: not-allowed;
  opacity: 0.6;
}

.form-error {
  color: #ffffff;
  font-size: 0.9rem;
  margin-bottom: 1.5rem;
}

.text-center {
  text-align: center;
}

.btn {
  display: inline-block;
  padding: 0.875rem 1.5rem;
  font-size: 1rem;
  font-weight: 600;
  text-align: center;
  border: none;
  border-radius: 10px;
  cursor: pointer;
  transition: all 0.2s ease;
  outline: none;
  text-decoration: none;
}

.btn-primary {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  box-shadow: 0 4px 15px rgba(102, 126, 234, 0.4);
}

.btn-primary:hover:not(:disabled) {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(102, 126, 234, 0.5);
}

.btn-primary:active:not(:disabled) {
  transform: translateY(0);
}

.btn-primary:disabled {
  opacity: 0.6;
  cursor: not-allowed;
  transform: none;
}

.btn-full {
  width: 100%;
}

.btn-loading {
  position: relative;
  color: transparent;
}

.btn-loading::after {
  content: '';
  position: absolute;
  width: 20px;
  height: 20px;
  top: 50%;
  left: 50%;
  margin-left: -10px;
  margin-top: -10px;
  border: 3px solid rgba(255, 255, 255, 0.3);
  border-top-color: white;
  border-radius: 50%;
  animation: spin 0.6s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

.otp-help {
  margin-top: 2rem;
  text-align: center;
}

.help-text {
  font-size: 0.9rem;
  color: rgba(255, 255, 255, 0.85);
  margin-bottom: 0.5rem;
}

.btn-link {
  background: none;
  border: none;
  color: #ffffff;
  font-size: 0.95rem;
  font-weight: 600;
  cursor: pointer;
  padding: 0.5rem;
  text-decoration: underline;
  transition: all 0.2s ease;
}

.btn-link:hover {
  color: #ffffff;
  opacity: 0.8;
  text-decoration: underline;
}

.app-version {
  position: fixed;
  bottom: 1rem;
  right: 1rem;
  font-size: 0.8rem;
  color: rgba(255, 255, 255, 0.8);
}

/* Responsive Design */
@media (max-width: 600px) {
  .otp-input-group {
    gap: 0.5rem;
  }

  .otp-input {
    width: 48px;
    height: 56px;
    font-size: 1.5rem;
  }

  .auth-welcome-text {
    font-size: 1.5rem;
  }

  .otp-icon {
    width: 64px;
    height: 64px;
  }

  .otp-icon svg {
    width: 48px;
    height: 48px;
  }
}
</style>
