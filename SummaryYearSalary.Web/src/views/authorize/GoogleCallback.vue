<template>
  <div class="google-callback-container">
    <div v-if="error" class="callback-error">
      <p>เกิดข้อผิดพลาดในการเข้าสู่ระบบด้วย Google</p>
      <p class="error-detail">{{ error }}</p>
      <button class="btn btn-primary" @click="goToLogin">กลับหน้าเข้าสู่ระบบ</button>
    </div>
    <div v-else class="callback-loading">
      <div class="spinner"></div>
      <p>กำลังเข้าสู่ระบบ...</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { useAuthStore } from '@/stores/authStore';
import { SecureStorage } from '@/utils/storage';
import { STORAGE_APP_LOCKED, STORAGE_LAST_ACTIVITY } from '@/constants/storage';

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
    },
  });

  SecureStorage.removeItem(STORAGE_APP_LOCKED);
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

<style scoped>
.google-callback-container {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 60vh;
  flex-direction: column;
  gap: 1rem;
  text-align: center;
}

.callback-loading {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1rem;
  color: #718096;
}

.callback-error {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.75rem;
}

.error-detail {
  color: #e53e3e;
  font-size: 14px;
}

.spinner {
  width: 40px;
  height: 40px;
  border: 4px solid #e2e8f0;
  border-top-color: #667eea;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}
</style>
