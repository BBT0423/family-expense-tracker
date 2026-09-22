<template>
  <div class="background-decoration">
    <div class="circle circle-1"></div>
    <div class="circle circle-2"></div>
    <div class="circle circle-3"></div>
  </div>
  <div class="auth-login-outer">
    <router-view />
  </div>
</template>

<script lang="ts" setup>
import '@/assets/ep-auth.css';
import { STORAGE_THEME } from '@/constants/storage';
import { SecureStorage } from '@/utils/storage';
import { onMounted, provide, ref } from 'vue';

// THEME STATE (shared)
const isDark = ref(false);

function toggleTheme() {
  isDark.value = !isDark.value;
  if (isDark.value) {
    document.body.dataset.theme = 'dark';
    SecureStorage.setItem(STORAGE_THEME, 'dark');
  } else {
    delete document.body.dataset.theme;
    SecureStorage.setItem(STORAGE_THEME, 'light');
  }
}

provide('isDark', isDark);
provide('toggleTheme', toggleTheme);

onMounted(() => {
  const savedTheme = SecureStorage.getItem(STORAGE_THEME);
  if (savedTheme === 'dark') {
    isDark.value = true;
    document.body.dataset.theme = 'dark';
  }
});
</script>

<style>
.auth-login-outer {
  position: fixed;
  inset: 0;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: 100vh;
  width: 100vw;
  background: linear-gradient(135deg, #0e7490 0%, #0891b2 100%);
  overflow: hidden;
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
}

/* Background decoration */
.background-decoration {
  position: absolute;
  inset: 0;
  pointer-events: none;
  z-index: 1;
}

.circle {
  position: absolute;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.1);
  animation: float 6s ease-in-out infinite;
}

.circle-1 {
  width: 200px;
  height: 200px;
  top: 10%;
  left: 10%;
  animation-delay: 0s;
}

.circle-2 {
  width: 150px;
  height: 150px;
  top: 60%;
  right: 15%;
  animation-delay: 2s;
}

.circle-3 {
  width: 100px;
  height: 100px;
  bottom: 20%;
  left: 20%;
  animation-delay: 4s;
}

@keyframes float {
  0%,
  100% {
    transform: translateY(0px) scale(1);
    opacity: 0.7;
  }

  50% {
    transform: translateY(-20px) scale(1.05);
    opacity: 0.9;
  }
}

.auth-login-container {
  width: 100%;
  max-width: 400px;
  position: relative;
  z-index: 2;
}

@keyframes slideUp {
  from {
    opacity: 0;
    transform: translateY(30px);
  }

  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.auth-logo {
  text-align: center;
  display: flex;
  flex-direction: column;
  align-items: center;
}

.auth-welcome-text {
  font-size: 24px;
  font-weight: 600;
  color: var(--text);
  text-align: center;
  margin-bottom: 8px;
  margin-top: 16px;
}

.auth-subtitle {
  color: var(--text-light);
  font-size: 15px;
  text-align: center;
  margin-bottom: 24px;
}

.auth-checkbox-group {
  display: flex;
  align-items: center;
  margin-bottom: 24px;
}

.auth-checkbox {
  width: 18px;
  height: 18px;
  margin-right: 12px;
  accent-color: var(--primary);
}

.auth-checkbox-label {
  color: var(--text-light);
  font-size: 15px;
  cursor: pointer;
}

@media (max-width: 480px) {
  .auth-login-container {
    padding: 48px 24px 24px 24px;
    margin: 0;
    box-shadow: none;
    height: 100vh;
    border-radius: 0;
  }

  .auth-welcome-text {
    font-size: 20px;
  }
}

.auth-theme-toggle {
  position: absolute;
  top: 16px;
  right: 16px;
}

.app-version {
  display: flex;
  justify-content: center;
  margin-top: 2rem;
  font-size: 0.85rem;
  color: var(--text-light);
}
</style>
