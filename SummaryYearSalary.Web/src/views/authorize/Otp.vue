<template>
  <div class="auth-login-container">
    <div class="ep-card">
      <div class="ep-card__body" style="text-align: center">
        <div style="font-size: 40px; margin-bottom: 10px; animation: epBounceIn 0.4s both">🔐</div>
        <div class="ep-h1">ยืนยัน 2-Factor</div>
        <div class="ep-sub">
          กรอกรหัส 6 หลักจาก<br /><strong>Google Authenticator</strong>
        </div>

        <ProMessage />

        <form @submit.prevent="verifyOtp">
          <div
            ref="otpRowRef"
            class="ep-otp-row"
            :class="{ 'anim-shake': hasError }"
            style="margin-bottom: 16px"
          >
            <input
              v-for="(digit, index) in otpDigits"
              :key="index"
              :ref="(el) => (otpInputs[index] = el as HTMLInputElement)"
              v-model="otpDigits[index]"
              class="ep-otp-cell"
              :class="{ filled: otpDigits[index] }"
              type="tel"
              inputmode="numeric"
              maxlength="1"
              autocomplete="off"
              :disabled="loading"
              @input="handleInput(index, $event)"
              @keydown="handleKeydown(index, $event)"
              @paste="handlePaste($event)"
            />
          </div>

          <div
            v-if="hasError"
            class="ep-form-item__error"
            style="margin-bottom: 12px; text-align: center"
          >
            {{ errorMessage }}
          </div>

          <div style="margin-top: 8px">
            <button
              type="submit"
              class="ep-btn ep-btn--primary"
              :disabled="loading || !isOtpComplete"
            >
              {{ loading ? 'กำลังตรวจสอบ...' : 'ยืนยัน' }}
            </button>
            <AppProgress v-if="loading" :indeterminate="true" style="margin-top: 8px" />
            <button
              type="button"
              class="ep-btn ep-btn--danger"
              :disabled="loading"
              @click="goBackToLogin"
            >
              ← ยกเลิก
            </button>
          </div>

          <div class="ep-alert ep-alert--info" style="margin-top: 16px; text-align: left">
            💡 เปิด Google Authenticator แล้วกรอกรหัส 6 หลักที่แสดง
          </div>
        </form>
      </div>
    </div>
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
import AppProgress from '@/components/AppProgress.vue';

const router = useRouter();
const route = useRoute();
const { loading, showLoading, hideLoading, showMessage } = useGlobalUi();
const { setAuth } = useAuthStore();

const appVersion = APP_VERSION;
const otpDigits = reactive<string[]>(Array(6).fill(''));
const otpInputs = ref<HTMLInputElement[]>([]);
const otpRowRef = ref<HTMLElement | null>(null);
const hasError = ref(false);
const errorMessage = ref('');

const userId = computed(() => (route.query.userId as string) || '');

const isOtpComplete = computed(() => {
  return otpDigits.every((digit) => digit.length === 1 && /^\d$/.test(digit));
});

const otpCode = computed(() => otpDigits.join(''));

onMounted(() => {
  if (!userId.value) {
    showMessage('Invalid access. Please login again.', 'error');
    router.push('/authorize/login');
    return;
  }
  otpInputs.value[0]?.focus();
});

const handleInput = (index: number, event: Event) => {
  const input = event.target as HTMLInputElement;
  const value = input.value;

  hasError.value = false;
  errorMessage.value = '';

  if (value && !/^\d$/.test(value)) {
    otpDigits[index] = '';
    return;
  }

  if (value && index < 5) {
    otpInputs.value[index + 1]?.focus();
  }
};

const handleKeydown = (index: number, event: KeyboardEvent) => {
  if (event.key === 'Backspace') {
    if (!otpDigits[index] && index > 0) {
      otpInputs.value[index - 1]?.focus();
    }
  } else if (event.key === 'ArrowLeft' && index > 0) {
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
    otpInputs.value[5]?.focus();
    hasError.value = false;
    errorMessage.value = '';
  }
};

const triggerShake = () => {
  hasError.value = true;
  if (otpRowRef.value) {
    otpRowRef.value.classList.remove('anim-shake');
    void otpRowRef.value.offsetWidth; // reflow to restart animation
    otpRowRef.value.classList.add('anim-shake');
  }
};

const verifyOtp = async () => {
  if (!isOtpComplete.value) {
    errorMessage.value = 'กรุณากรอกรหัส 6 หลัก';
    triggerShake();
    return;
  }

  showLoading();
  try {
    const { data } = await authService.verifyMfa({
      mfaCode: otpCode.value,
      userId: userId.value,
    });

    if (data.isSuccess && data.value) {
      setAuth({ ...data.value, user: { ...data.value.user, loginProvider: 'email' } });
      localStorage.removeItem(STORAGE_LAST_ACTIVITY);
      showMessage('เข้าสู่ระบบสำเร็จ', 'success');
      await router.push('/pages');
      globalThis.location.reload();
    } else {
      errorMessage.value = data.error ?? 'รหัสยืนยันไม่ถูกต้อง';
      triggerShake();
      otpDigits.forEach((_, index) => {
        otpDigits[index] = '';
      });
      otpInputs.value[0]?.focus();
    }
  } catch (e: any) {
    console.error('❌ OTP verification error:', e);
    const problem = toProblemDetails(e);
    errorMessage.value = problem.detail || 'ยืนยันไม่สำเร็จ กรุณาลองอีกครั้ง';
    triggerShake();
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
.app-version {
  text-align: center;
  margin-top: 14px;
  font-size: 11px;
  color: rgba(255, 255, 255, 0.5);
}
</style>
