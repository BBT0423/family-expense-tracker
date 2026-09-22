<template>
  <div class="auth-login-container">
    <div class="ep-card">
      <div class="ep-card__body" style="text-align: center; padding: 40px 28px">
        <!-- Error state -->
        <template v-if="error">
          <div style="font-size: 40px; margin-bottom: 12px">⚠️</div>
          <div class="ep-h1" style="color: var(--ep-danger); font-size: 18px">
            เข้าสู่ระบบไม่สำเร็จ
          </div>
          <div class="ep-sub">{{ error }}</div>
          <div class="ep-alert ep-alert--danger" style="text-align: left; margin-bottom: 20px">
            เกิดข้อผิดพลาดจาก Google OAuth กรุณาลองใหม่อีกครั้ง
          </div>
          <button class="ep-btn ep-btn--default" @click="goToLogin">← กลับหน้าเข้าสู่ระบบ</button>
        </template>

        <!-- Loading state -->
        <template v-else>
          <div style="margin-bottom: 16px">
            <svg width="40" height="40" viewBox="0 0 24 24" style="margin: 0 auto">
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
          </div>
          <div class="ep-h1" style="font-size: 18px">กำลังเชื่อมต่อ...</div>
          <div class="ep-sub" style="margin-bottom: 24px">
            กำลังรับข้อมูลจาก Google<br />โปรดรอสักครู่
          </div>
          <AppProgress :indeterminate="true" label="กำลังเชื่อมต่อกับ Google..." />
          <div style="font-size: 12px; color: var(--ep-text-4); margin-top: 12px">
            รับ token สำเร็จ → redirect อัตโนมัติ
          </div>
        </template>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { useAuthStore } from '@/stores/authStore';
import { STORAGE_LAST_ACTIVITY } from '@/constants/storage';
import AppProgress from '@/components/AppProgress.vue';

const router = useRouter();
const route = useRoute();
const authStore = useAuthStore();
const error = ref('');

const goToLogin = () => router.push('/authorize/login');

onMounted(async () => {
  const tokenParam = route.query.token as string | undefined;
  const refreshTokenParam = route.query.refreshToken as string | undefined;
  const errorParam = route.query.error as string | undefined;

  if (errorParam) {
    error.value = decodeURIComponent(errorParam).replace(/_/g, ' ');
    return;
  }

  if (!tokenParam || !refreshTokenParam) {
    error.value = 'ไม่ได้รับ token จาก Google';
    return;
  }

  let metadata: Record<string, any> = {};
  const metadataParam = route.query.metadata as string | undefined;
  if (metadataParam) {
    try {
      metadata = JSON.parse(atob(decodeURIComponent(metadataParam)));
    } catch {
      // ignore malformed metadata
    }
  }

  authStore.setAuth({
    token: decodeURIComponent(tokenParam),
    refreshToken: decodeURIComponent(refreshTokenParam),
    expiresIn: 120,
    isMfaEnabled: false,
    user: {
      id: route.query.userId ? decodeURIComponent(route.query.userId as string) : '',
      email: route.query.email ? decodeURIComponent(route.query.email as string) : '',
      name: route.query.name ? decodeURIComponent(route.query.name as string) : '',
      avatar: route.query.avatar ? decodeURIComponent(route.query.avatar as string) : '',
      metadata,
      loginProvider: 'google',
    },
  });

  localStorage.removeItem(STORAGE_LAST_ACTIVITY);

  const redirectUrl = sessionStorage.getItem('REDIRECT_URL');
  if (redirectUrl) {
    sessionStorage.removeItem('REDIRECT_URL');
    await router.push(redirectUrl);
  } else {
    await router.push('/pages');
  }
  globalThis.location.reload();
});
</script>
