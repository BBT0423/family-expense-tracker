<template>
  <div id="loginContainer" class="auth-login-container">
    <div class="auth-logo">
      <h1 class="auth-welcome-text">Sign in</h1>
      <p class="auth-subtitle">to continue to your account</p>
    </div>
    <ProMessage />
    <template v-if="isAuthenticated && !showLoginForm">
      <div class="auth-continue-group">
        <button class="btn btn-primary btn-full" @click="continueWithUser">
          Continue as {{ userName }}
        </button>
        <button class="btn btn-secondary btn-full" @click="showLoginForm = true">
          Login with different account
        </button>
      </div>
    </template>
    <template v-else>
      <!-- Login method tabs -->
      <div class="login-tabs">
        <button
          class="login-tab"
          :class="{ active: activeTab === 'email' }"
          @click="activeTab = 'email'"
        >
          Email
        </button>
        <button
          class="login-tab"
          :class="{ active: activeTab === 'phone' }"
          @click="activeTab = 'phone'"
        >
          เบอร์โทร
        </button>
      </div>

      <!-- Email / Password Login -->
      <form v-if="activeTab === 'email'" id="loginForm" @submit.prevent="login">
        <ProGrid :columns="1">
          <div class="form-group">
            <label class="form-label" for="username">Email address</label>
            <input
              id="username"
              v-model="pageData.email"
              class="form-input"
              type="text"
              placeholder="Enter your email"
              autocomplete="username"
              required
            />
            <div v-if="errors.email" class="form-error">{{ errors.email }}</div>
          </div>
          <div class="form-group mb-4">
            <label class="form-label" for="password">Password</label>
            <input
              id="password"
              v-model="pageData.password"
              class="form-input"
              type="password"
              placeholder="Enter your password"
              autocomplete="current-password"
              required
            />
            <div v-if="errors.password" class="form-error">{{ errors.password }}</div>
          </div>

          <button
            type="submit"
            class="btn btn-primary btn-full"
            :class="{ 'btn-loading': loading }"
            :disabled="loading"
          >
            <span v-if="loading">Signing in...</span>
            <span v-else>Sign in</span>
          </button>
        </ProGrid>
      </form>

      <!-- Phone OTP Login -->
      <form v-else id="phoneLoginForm" @submit.prevent="handlePhoneStep">
        <ProGrid :columns="1">
          <template v-if="phoneStep === 'phone'">
            <div class="form-group">
              <label class="form-label" for="phoneNumber">เบอร์โทรศัพท์</label>
              <input
                id="phoneNumber"
                v-model="phoneData.phoneNumber"
                class="form-input"
                type="tel"
                placeholder="0812345678"
                autocomplete="tel"
                required
              />
              <div v-if="phoneErrors.phoneNumber" class="form-error">
                {{ phoneErrors.phoneNumber }}
              </div>
            </div>
            <button
              type="submit"
              class="btn btn-primary btn-full"
              :class="{ 'btn-loading': phoneLoading }"
              :disabled="phoneLoading"
            >
              <span v-if="phoneLoading">กำลังส่ง OTP...</span>
              <span v-else>ส่ง OTP ไปยัง Email</span>
            </button>
          </template>

          <template v-else>
            <p class="otp-hint">ส่งรหัส OTP ไปยัง email ที่ผูกกับเบอร์ {{ phoneData.phoneNumber }} แล้ว</p>
            <div class="form-group">
              <label class="form-label" for="otpCode">รหัส OTP (6 หลัก)</label>
              <input
                id="otpCode"
                v-model="phoneData.otpCode"
                class="form-input otp-input"
                type="text"
                inputmode="numeric"
                maxlength="6"
                placeholder="000000"
                autocomplete="one-time-code"
                required
              />
              <div v-if="phoneErrors.otpCode" class="form-error">{{ phoneErrors.otpCode }}</div>
            </div>
            <button
              type="submit"
              class="btn btn-primary btn-full"
              :class="{ 'btn-loading': phoneLoading }"
              :disabled="phoneLoading"
            >
              <span v-if="phoneLoading">กำลังตรวจสอบ...</span>
              <span v-else>เข้าสู่ระบบ</span>
            </button>
            <button
              type="button"
              class="btn btn-link btn-full"
              :disabled="phoneLoading || resendCooldown > 0"
              @click="resendOtp"
            >
              <span v-if="resendCooldown > 0">ส่งรหัสใหม่ได้ใน {{ resendCooldown }}s</span>
              <span v-else>ส่งรหัสใหม่</span>
            </button>
          </template>
        </ProGrid>
      </form>

      <!-- Google Login -->
      <template v-if="!isAuthenticated || showLoginForm">
        <div class="divider">
          <span>หรือ</span>
        </div>
        <a :href="googleLoginUrl" class="btn-google">
          <svg width="18" height="18" viewBox="0 0 18 18" xmlns="http://www.w3.org/2000/svg">
            <path d="M17.64 9.2c0-.637-.057-1.251-.164-1.84H9v3.481h4.844c-.209 1.125-.843 2.078-1.796 2.716v2.259h2.908c1.702-1.567 2.684-3.875 2.684-6.615z" fill="#4285F4"/>
            <path d="M9 18c2.43 0 4.467-.806 5.956-2.18l-2.908-2.259c-.806.54-1.837.86-3.048.86-2.344 0-4.328-1.584-5.036-3.711H.957v2.332A8.997 8.997 0 0 0 9 18z" fill="#34A853"/>
            <path d="M3.964 10.71A5.41 5.41 0 0 1 3.682 9c0-.593.102-1.17.282-1.71V4.958H.957A8.996 8.996 0 0 0 0 9c0 1.452.348 2.827.957 4.042l3.007-2.332z" fill="#FBBC05"/>
            <path d="M9 3.58c1.321 0 2.508.454 3.44 1.345l2.582-2.58C13.463.891 11.426 0 9 0A8.997 8.997 0 0 0 .957 4.958L3.964 6.29C4.672 4.163 6.656 3.58 9 3.58z" fill="#EA4335"/>
          </svg>
          เข้าสู่ระบบด้วย Google
        </a>
      </template>

      <p class="signup-link">
        ยังไม่มีบัญชี?
        <router-link to="/authorize/register">สมัครสมาชิก</router-link>
      </p>
    </template>

    <div class="app-version">v{{ appVersion }}</div>
  </div>
</template>

<script setup lang="ts">
import { useLogin } from '@/composables/useLogin';
import { usePhoneLogin } from '@/composables/usePhoneLogin';
import { APP_VERSION } from '@/constants/version';
import { ref, computed, onMounted } from 'vue';
import { useGlobalUi } from '@/composables/useGlobalUi';

const appVersion = APP_VERSION;
const activeTab = ref<'email' | 'phone'>('email');
const { showMessage } = useGlobalUi();

const {
  pageData,
  loading,
  errors,
  isAuthenticated,
  user,
  login,
  continueWithUser,
} = useLogin();

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
const showLoginForm = ref(false);
const googleLoginUrl = `${import.meta.env.VITE_API_BASE_URL}/api/authorize/google`;

onMounted(() => {
  if (sessionStorage.getItem('SESSION_EXPIRED') === 'true') {
    sessionStorage.removeItem('SESSION_EXPIRED');
    showMessage('Session หมดอายุ กรุณาเข้าสู่ระบบใหม่', 'warning');
  }
});
</script>

<style scoped>
.form-error {
  color: #e53e3e;
  font-size: 0.97em;
  margin-top: 4px;
  margin-bottom: 2px;
}

.auth-continue-group {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  margin-bottom: 2rem;
}

.login-tabs {
  display: flex;
  gap: 0;
  margin-bottom: 1.5rem;
  border-bottom: 2px solid #e2e8f0;
}

.login-tab {
  flex: 1;
  padding: 10px 16px;
  background: none;
  border: none;
  border-bottom: 2px solid transparent;
  margin-bottom: -2px;
  font-size: 14px;
  font-weight: 500;
  color: #718096;
  cursor: pointer;
  transition: all 0.2s;
}

.login-tab.active {
  color: #667eea;
  border-bottom-color: #667eea;
}

.otp-hint {
  font-size: 13px;
  color: #718096;
  text-align: center;
  margin-bottom: 0.5rem;
}

.otp-input {
  text-align: center;
  letter-spacing: 8px;
  font-size: 20px;
  font-weight: 600;
}

.btn-link {
  background: none;
  border: none;
  color: #667eea;
  font-size: 14px;
  cursor: pointer;
  padding: 8px;
  text-align: center;
  width: 100%;
}

.btn-link:hover:not(:disabled) {
  text-decoration: underline;
}

.btn-link:disabled {
  color: #a0aec0;
  cursor: not-allowed;
}

.divider {
  display: flex;
  align-items: center;
  text-align: center;
  margin: 1.25rem 0;
  color: #a0aec0;
  font-size: 13px;
}

.divider::before,
.divider::after {
  content: '';
  flex: 1;
  border-bottom: 1px solid #e2e8f0;
}

.divider span {
  padding: 0 12px;
}

.btn-google {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  width: 100%;
  padding: 11px 16px;
  background: white;
  border: 1.5px solid #e2e8f0;
  border-radius: 8px;
  font-size: 14px;
  font-weight: 500;
  color: #4a5568;
  text-decoration: none;
  cursor: pointer;
  transition: background 0.2s, border-color 0.2s;
  margin-bottom: 0.5rem;
}

.btn-google:hover {
  background: #f7fafc;
  border-color: #cbd5e0;
}

.signup-link {
  text-align: center;
  font-size: 14px;
  color: #718096;
  margin-top: 1rem;
}

.signup-link a {
  color: #667eea;
  text-decoration: none;
  font-weight: 500;
}

.signup-link a:hover {
  text-decoration: underline;
}
</style>
