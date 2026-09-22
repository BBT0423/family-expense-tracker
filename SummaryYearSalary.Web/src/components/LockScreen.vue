<template>
  <div v-if="isLocked" class="lock-screen-overlay">
    <div class="lock-screen-wrap">
      <div class="ep-card ep-card--dashed-warn">
        <div class="ep-card__body" style="text-align: center">
          <!-- Badge -->
          <div style="margin-bottom: 16px">
            <span class="ep-lock-badge">⏱ หน้าจอถูกล็อกอัตโนมัติ</span>
          </div>

          <!-- Avatar -->
          <div class="ep-avatar" style="width: 72px; height: 72px; font-size: 28px">
            <img
              v-if="user?.avatar"
              :src="user.avatar"
              :alt="user.name"
              style="width: 100%; height: 100%; object-fit: cover; border-radius: 50%"
            />
            <span v-else>{{ userInitials }}</span>
          </div>

          <!-- User info -->
          <div class="ep-h1" style="font-size: 18px; margin-bottom: 4px">
            {{ user?.name || 'User' }}
          </div>
          <div style="font-size: 12px; color: var(--ep-text-3); margin-bottom: 20px">
            {{ user?.email }}
          </div>

          <!-- Email unlock form -->
          <form v-if="!isPasswordlessUser" style="text-align: left" @submit.prevent="handleUnlock">
            <div class="ep-form-item">
              <label class="ep-form-item__label">รหัสผ่าน</label>
              <div class="ep-input">
                <div class="ep-input__wrapper" :class="{ 'ep-input__wrapper--error': error }">
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
                    v-model="password"
                    class="ep-input__inner"
                    :type="showPassword ? 'text' : 'password'"
                    placeholder="กรอกรหัสผ่านเพื่อปลดล็อก"
                    :autocomplete="showPassword ? 'off' : 'current-password'"
                    :disabled="isUnlocking"
                    @input="error = ''"
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
              <div v-if="error" class="ep-form-item__error">{{ error }}</div>
            </div>

            <button
              type="submit"
              class="ep-btn ep-btn--primary"
              :disabled="!password || isUnlocking"
            >
              {{ isUnlocking ? 'กำลังตรวจสอบ...' : 'ปลดล็อก' }}
            </button>
            <button
              type="button"
              class="ep-btn ep-btn--danger"
              :disabled="isUnlocking"
              @click="handleLogout"
            >
              ออกจากระบบ
            </button>
          </form>

          <!-- Passwordless notice (Google / Phone) -->
          <div v-else style="padding: 8px 0 16px">
            <div class="ep-alert ep-alert--warning" style="text-align: left; margin-bottom: 20px">
              <span v-if="authStore.loginProvider === 'google'">
                กรุณาออกจากระบบและ <strong>Sign in with Google</strong> อีกครั้ง
              </span>
              <span v-else>
                กรุณาออกจากระบบและเข้าสู่ระบบด้วย<strong>เบอร์โทร</strong>อีกครั้ง
              </span>
            </div>
            <button class="ep-btn ep-btn--danger" @click="handleLogout">ออกจากระบบ</button>
          </div>

          <!-- Lock time -->
          <div
            style="
              margin-top: 16px;
              padding-top: 14px;
              border-top: 1px solid var(--ep-border-light);
              font-size: 12px;
              color: var(--ep-text-4);
            "
          >
            ล็อกเมื่อ {{ lockedTimeText }}
          </div>
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
const lockedTimeText = ref('เมื่อกี้');

const user = computed(() => authStore.user);
const isPasswordlessUser = computed(() => authStore.loginProvider !== 'email');

const userInitials = computed(() => {
  if (!user.value?.name) return 'U';
  const names = user.value.name.split(' ');
  if (names.length >= 2) return (names[0][0] + names[1][0]).toUpperCase();
  return user.value.name.substring(0, 2).toUpperCase();
});

let timeUpdateInterval: ReturnType<typeof setInterval> | null = null;

const updateLockedTime = () => {
  if (!lockScreenStore.lockTime) {
    lockedTimeText.value = 'เมื่อกี้';
    return;
  }
  const diff = Date.now() - lockScreenStore.lockTime;
  const minutes = Math.floor(diff / 60000);
  if (minutes < 1) lockedTimeText.value = 'เมื่อกี้';
  else if (minutes === 1) lockedTimeText.value = '1 นาทีที่แล้ว';
  else if (minutes < 60) lockedTimeText.value = `${minutes} นาทีที่แล้ว`;
  else {
    const hours = Math.floor(minutes / 60);
    lockedTimeText.value = hours === 1 ? '1 ชั่วโมงที่แล้ว' : `${hours} ชั่วโมงที่แล้ว`;
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
  if (timeUpdateInterval) clearInterval(timeUpdateInterval);
});

const handleUnlock = async () => {
  if (!password.value || isUnlocking.value) return;
  error.value = '';
  isUnlocking.value = true;
  try {
    const { data } = await authService.login({
      email: user.value?.email || '',
      password: password.value,
    });
    if (data.isSuccess && data.value) {
      password.value = '';
      props.onUnlock();
      showMessage('Session unlocked successfully', 'success');
    } else {
      error.value = 'รหัสผ่านไม่ถูกต้อง กรุณาลองใหม่';
    }
  } catch {
    error.value = 'รหัสผ่านไม่ถูกต้อง กรุณาลองใหม่';
  } finally {
    isUnlocking.value = false;
  }
};

const handleLogout = () => {
  authStore.clearAuth();
  lockScreenStore.unlock();
  props.onUnlock();
  router.push('/authorize/login');
};
</script>

<style scoped>
.lock-screen-overlay {
  position: fixed;
  inset: 0;
  background: linear-gradient(135deg, #0e7490 0%, #0891b2 100%);
  z-index: 10000;
  display: flex;
  align-items: center;
  justify-content: center;
  animation: lockFadeIn 0.3s ease-in;
}

@keyframes lockFadeIn {
  from {
    opacity: 0;
  }
  to {
    opacity: 1;
  }
}

.lock-screen-wrap {
  width: 100%;
  max-width: 400px;
  padding: 20px;
}

/* ep-card dashed-warn header color override */
.ep-card--dashed-warn .ep-card__header {
  background: #fdf6ec;
  color: #9a6400;
}

@media (max-width: 480px) {
  .lock-screen-wrap {
    padding: 12px;
  }
}
</style>
