<template>
  <div class="auth-login-container">
    <div class="ep-card">
      <div class="ep-card__body">
        <!-- Header -->
        <div style="text-align: center; margin-bottom: 20px">
          <div class="ep-h1" style="margin-bottom: 4px">เข้าสู่ระบบ</div>
          <div style="font-size: 13px; color: var(--ep-text-3)">Family Expense Tracker</div>
        </div>

        <ProMessage />

        <!-- Already logged-in: continue or switch account -->
        <template v-if="isAuthenticated && !showLoginForm">
          <div class="ep-avatar">{{ userInitial }}</div>
          <div
            style="
              text-align: center;
              font-size: 14px;
              color: var(--ep-text-2);
              margin-bottom: 20px;
            "
          >
            {{ userName }}
          </div>
          <button class="ep-btn ep-btn--primary" @click="continueWithUser">
            <svg width="12" height="12" viewBox="0 0 24 24" fill="currentColor">
              <polygon points="5 3 19 12 5 21 5 3" />
            </svg>
            Continue as {{ userName }}
          </button>
          <button class="ep-btn ep-btn--default" @click="showLoginForm = true">
            เข้าสู่ระบบด้วยบัญชีอื่น
          </button>
        </template>

        <template v-else>
          <!-- Tabs -->
          <div class="ep-tabs">
            <div class="ep-tabs__nav-wrap">
              <div
                class="ep-tabs__item"
                :class="{ 'is-active': activeTab === 'email' }"
                @click="activeTab = 'email'"
              >
                อีเมล / รหัสผ่าน
              </div>
              <div
                class="ep-tabs__item"
                :class="{ 'is-active': activeTab === 'phone' }"
                @click="activeTab = 'phone'"
              >
                เบอร์โทร
              </div>
            </div>
          </div>

          <!-- ── Email / Password ── -->
          <form v-if="activeTab === 'email'" @submit.prevent="login">
            <div class="ep-form">
              <div class="ep-form-item">
                <label class="ep-form-item__label"> <span class="required">*</span>อีเมล </label>
                <div class="ep-input">
                  <div
                    class="ep-input__wrapper"
                    :class="{ 'ep-input__wrapper--error': errors.email }"
                  >
                    <span class="ep-input__prefix">
                      <svg
                        width="14"
                        height="14"
                        viewBox="0 0 24 24"
                        fill="none"
                        stroke="currentColor"
                        stroke-width="2"
                      >
                        <path
                          d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"
                        />
                        <polyline points="22,6 12,12 2,6" />
                      </svg>
                    </span>
                    <input
                      v-model="pageData.email"
                      class="ep-input__inner"
                      type="email"
                      placeholder="you@example.com"
                      autocomplete="username"
                    />
                  </div>
                </div>
                <div v-if="errors.email" class="ep-form-item__error">{{ errors.email }}</div>
              </div>

              <div class="ep-form-item">
                <label class="ep-form-item__label"> <span class="required">*</span>รหัสผ่าน </label>
                <div class="ep-input">
                  <div
                    class="ep-input__wrapper"
                    :class="{ 'ep-input__wrapper--error': errors.password }"
                  >
                    <span class="ep-input__prefix">
                      <svg
                        width="14"
                        height="14"
                        viewBox="0 0 24 24"
                        fill="none"
                        stroke="currentColor"
                        stroke-width="2"
                      >
                        <rect x="3" y="11" width="18" height="11" rx="2" />
                        <path d="M7 11V7a5 5 0 0 1 10 0v4" />
                      </svg>
                    </span>
                    <input
                      id="pw-main"
                      v-model="pageData.password"
                      class="ep-input__inner"
                      :type="showPassword ? 'text' : 'password'"
                      placeholder="รหัสผ่าน"
                      autocomplete="current-password"
                    />
                    <span class="ep-input__suffix" @click="showPassword = !showPassword">
                      <svg
                        v-if="!showPassword"
                        width="14"
                        height="14"
                        viewBox="0 0 24 24"
                        fill="none"
                        stroke="currentColor"
                        stroke-width="2"
                      >
                        <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z" />
                        <circle cx="12" cy="12" r="3" />
                      </svg>
                      <svg
                        v-else
                        width="14"
                        height="14"
                        viewBox="0 0 24 24"
                        fill="none"
                        stroke="currentColor"
                        stroke-width="2"
                      >
                        <path
                          d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94"
                        />
                        <path
                          d="M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19"
                        />
                        <line x1="1" y1="1" x2="23" y2="23" />
                      </svg>
                    </span>
                  </div>
                </div>
                <div v-if="errors.password" class="ep-form-item__error">{{ errors.password }}</div>
              </div>
            </div>

            <div style="margin-top: 20px">
              <button type="submit" class="ep-btn ep-btn--primary" :disabled="loading">
                {{ loading ? 'กำลังเข้าสู่ระบบ...' : 'เข้าสู่ระบบ' }}
              </button>
              <AppProgress v-if="loading" :indeterminate="true" style="margin-top: 8px" />
            </div>
          </form>

          <!-- ── Phone OTP ── -->
          <form v-else @submit.prevent="handlePhoneStep">
            <!-- Step 1: phone number -->
            <template v-if="phoneStep === 'phone'">
              <div class="ep-form">
                <div class="ep-form-item">
                  <label class="ep-form-item__label">
                    <span class="required">*</span>เบอร์โทรศัพท์
                  </label>
                  <div class="ep-input">
                    <div
                      class="ep-input__wrapper"
                      :class="{ 'ep-input__wrapper--error': phoneErrors.phoneNumber }"
                    >
                      <span class="ep-input__prefix">
                        <svg
                          width="14"
                          height="14"
                          viewBox="0 0 24 24"
                          fill="none"
                          stroke="currentColor"
                          stroke-width="2"
                        >
                          <path
                            d="M22 16.92v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07A19.5 19.5 0 0 1 4.69 12 19.79 19.79 0 0 1 1.57 3.36 2 2 0 0 1 3.56 1.14h3a2 2 0 0 1 2 1.72c.127.96.361 1.903.7 2.81a2 2 0 0 1-.45 2.11L7.91 8.69a16 16 0 0 0 6.29 6.29l.91-.91a2 2 0 0 1 2.11-.45c.907.339 1.85.573 2.81.7A2 2 0 0 1 22 16.92z"
                          />
                        </svg>
                      </span>
                      <input
                        v-model="phoneData.phoneNumber"
                        class="ep-input__inner"
                        type="tel"
                        placeholder="0812345678"
                        maxlength="10"
                        autocomplete="tel"
                      />
                    </div>
                  </div>
                  <div v-if="phoneErrors.phoneNumber" class="ep-form-item__error">
                    {{ phoneErrors.phoneNumber }}
                  </div>
                  <div class="ep-form-item__hint">รูปแบบไทย: 08x-xxxx-xxxx</div>
                </div>
              </div>
              <div style="margin-top: 8px">
                <button type="submit" class="ep-btn ep-btn--primary" :disabled="phoneLoading">
                  {{ phoneLoading ? 'กำลังส่ง...' : 'ส่ง OTP' }}
                </button>
                <AppProgress v-if="phoneLoading" :indeterminate="true" style="margin-top: 8px" />
              </div>
            </template>

            <!-- Step 2: OTP cells -->
            <template v-else>
              <div
                style="
                  font-size: 13px;
                  color: var(--ep-text-2);
                  margin-bottom: 14px;
                  line-height: 1.6;
                "
              >
                ส่งรหัส 6 หลักไปที่ email ที่ผูกกับ <strong>{{ phoneData.phoneNumber }}</strong>
              </div>

              <div ref="otpRowRef" class="ep-otp-row">
                <input
                  v-for="(_, i) in 6"
                  :key="i"
                  :ref="(el) => (otpCells[i] = el as HTMLInputElement)"
                  class="ep-otp-cell"
                  :class="{ filled: otpDigits[i] }"
                  type="tel"
                  maxlength="1"
                  inputmode="numeric"
                  :value="otpDigits[i]"
                  @input="onOtpInput($event, i)"
                  @keydown="onOtpKeydown($event, i)"
                  @paste.prevent="onOtpPaste($event)"
                />
              </div>

              <div
                v-if="phoneErrors.otpCode"
                class="ep-form-item__error"
                style="margin-bottom: 8px"
              >
                {{ phoneErrors.otpCode }}
              </div>

              <div class="ep-countdown">
                <span v-if="resendCooldown > 0">
                  ส่งรหัสใหม่ได้ใน
                  <strong style="color: var(--ep-primary)"
                    >{{ String(Math.floor(resendCooldown / 60)).padStart(2, '0') }}:{{
                      String(resendCooldown % 60).padStart(2, '0')
                    }}</strong
                  >
                </span>
                <button
                  v-else
                  type="button"
                  class="ep-btn ep-btn--link"
                  style="width: auto; height: auto; padding: 0"
                  :disabled="phoneLoading"
                  @click="resendOtp"
                >
                  ส่งรหัสใหม่
                </button>
              </div>

              <button
                type="submit"
                class="ep-btn ep-btn--primary"
                :disabled="phoneLoading || otpDigits.join('').length < 6"
              >
                {{ phoneLoading ? 'กำลังตรวจสอบ...' : 'ยืนยัน OTP' }}
              </button>
              <AppProgress v-if="phoneLoading" :indeterminate="true" style="margin-top: 8px" />
              <button type="button" class="ep-btn ep-btn--default" @click="goBackToPhone">
                ← กลับ
              </button>
            </template>
          </form>

          <!-- Google + register link -->
          <div class="ep-divider">หรือ</div>
          <a :href="googleLoginUrl" class="ep-btn ep-btn--google">
            <svg width="16" height="16" viewBox="0 0 24 24">
              <path
                fill="#4285F4"
                d="M23.745 12.27c0-.79-.07-1.54-.19-2.27h-11.3v4.51h6.47c-.29 1.48-1.14 2.73-2.4 3.58v3h3.86c2.26-2.09 3.56-5.17 3.56-8.82z"
              />
              <path
                fill="#34A853"
                d="M12.255 24c3.24 0 5.95-1.08 7.93-2.91l-3.86-3c-1.08.72-2.45 1.16-4.07 1.16-3.13 0-5.78-2.11-6.73-4.96h-3.98v3.09C3.515 21.3 7.615 24 12.255 24z"
              />
              <path
                fill="#FBBC05"
                d="M5.525 14.29c-.25-.72-.38-1.49-.38-2.29s.14-1.57.38-2.29V6.62h-3.98a11.86 11.86 0 0 0 0 10.76l3.98-3.09z"
              />
              <path
                fill="#EA4335"
                d="M12.255 4.75c1.77 0 3.35.61 4.6 1.8l3.42-3.42C18.205 1.19 15.495 0 12.255 0c-4.64 0-8.74 2.7-10.71 6.62l3.98 3.09c.95-2.85 3.6-4.96 6.73-4.96z"
              />
            </svg>
            Sign in with Google
          </a>

          <div class="ep-link">
            ยังไม่มีบัญชี?
            <router-link to="/authorize/register"><b>สมัครสมาชิก</b></router-link>
          </div>
        </template>
      </div>
    </div>
    <div class="app-version">v{{ appVersion }}</div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useLogin } from '@/composables/useLogin';
import { usePhoneLogin } from '@/composables/usePhoneLogin';
import { APP_VERSION } from '@/constants/version';
import { useGlobalUi } from '@/composables/useGlobalUi';
import AppProgress from '@/components/AppProgress.vue';

const appVersion = APP_VERSION;
const activeTab = ref<'email' | 'phone'>('email');
const showLoginForm = ref(false);
const showPassword = ref(false);
const { showMessage } = useGlobalUi();

const { pageData, loading, errors, isAuthenticated, user, login, continueWithUser } = useLogin();
const {
  phoneData,
  phoneLoading,
  phoneErrors,
  phoneStep,
  resendCooldown,
  handlePhoneStep,
  resendOtp,
} = usePhoneLogin();

const userName = computed(() => user?.name || 'User');
const userInitial = computed(() => (user?.name || 'U').charAt(0).toUpperCase());
const googleLoginUrl = `${import.meta.env.VITE_API_BASE_URL}/api/authorize/google`;

// ── OTP cell management ──
const otpDigits = ref<string[]>(Array(6).fill(''));
const otpCells = ref<HTMLInputElement[]>([]);

function syncOtpToModel() {
  phoneData.otpCode = otpDigits.value.join('');
}

function onOtpInput(event: Event, index: number) {
  const input = event.target as HTMLInputElement;
  const digit = input.value.replace(/\D/g, '').slice(-1);
  otpDigits.value[index] = digit;
  input.value = digit;
  syncOtpToModel();
  if (digit && index < 5) otpCells.value[index + 1]?.focus();
}

function onOtpKeydown(event: KeyboardEvent, index: number) {
  if (event.key === 'Backspace' && !otpDigits.value[index] && index > 0) {
    otpDigits.value[index - 1] = '';
    syncOtpToModel();
    otpCells.value[index - 1]?.focus();
  } else if (event.key === 'ArrowLeft' && index > 0) {
    otpCells.value[index - 1]?.focus();
  } else if (event.key === 'ArrowRight' && index < 5) {
    otpCells.value[index + 1]?.focus();
  }
}

function onOtpPaste(event: ClipboardEvent) {
  const digits = (event.clipboardData?.getData('text') ?? '').replace(/\D/g, '').slice(0, 6);
  digits.split('').forEach((d, i) => {
    otpDigits.value[i] = d;
  });
  syncOtpToModel();
  const next = Math.min(digits.length, 5);
  otpCells.value[next]?.focus();
}

function goBackToPhone() {
  otpDigits.value = Array(6).fill('');
  syncOtpToModel();
  // reset phoneStep via composable by clearing phone number and re-entering
  phoneData.phoneNumber = '';
  phoneData.otpCode = '';
  // The composable exposes phoneStep as a ref; we access it directly
  (phoneStep as any).value = 'phone';
}

onMounted(() => {
  if (sessionStorage.getItem('SESSION_EXPIRED') === 'true') {
    sessionStorage.removeItem('SESSION_EXPIRED');
    showMessage('Session หมดอายุ กรุณาเข้าสู่ระบบใหม่', 'warning');
  }
});
</script>

<style scoped>
.app-version {
  text-align: center;
  margin-top: 14px;
  font-size: 11px;
  color: rgba(255, 255, 255, 0.5);
}

.ep-link a {
  color: var(--ep-primary);
  text-decoration: none;
  font-weight: 600;
}
.ep-link a:hover {
  text-decoration: underline;
}
</style>
