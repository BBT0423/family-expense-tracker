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
    <form v-else id="loginForm" @submit.prevent="login">
      <ProGrid :columns="1">
        <div class="form-group">
          <label class="form-label" for="username">Email address</label>
          <input
            id="username"
            v-model="pageData.email"
            class="form-input"
            type="text"
            placeholder="Enter your email or employee ID"
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
    <div class="app-version">v{{ appVersion }}</div>
  </div>
</template>

<script setup lang="ts">
import { useLogin } from '@/composables/useLogin';
import { APP_VERSION } from '@/constants/version';
import { ref, computed } from 'vue';

const appVersion = APP_VERSION;
const {
  // state
  pageData,
  loading,
  errors,
  isAuthenticated,
  user,

  // methods
  login,
  continueWithUser,
} = useLogin();

const userName = computed(() => user?.name || 'User');
const showLoginForm = ref(false);
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
</style>
