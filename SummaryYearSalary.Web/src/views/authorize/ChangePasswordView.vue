<template>
  <div id="loginContainer" class="auth-login-container">
    <div class="auth-logo">
      <h1 class="auth-welcome-text">Change Password</h1>
      <p class="auth-subtitle">Enter current password and choose a new one.</p>
    </div>
    <ProMessage />
    <form id="loginForm" @submit.prevent="changePassword">
      <ProGrid :columns="1">
        <ProFormGroup label="Current Password" for="currentPassword" required>
          <input
            id="currentPassword"
            v-model="pageData.currentPassword"
            class="form-input"
            type="password"
            placeholder="Enter your current password"
          />
          <div v-if="errors.currentPassword" class="form-error">{{ errors.currentPassword }}</div>
        </ProFormGroup>

        <ProFormGroup label="New Password" for="newPassword" required>
          <input
            id="newPassword"
            v-model="pageData.newPassword"
            class="form-input"
            type="password"
            placeholder="Enter your new password"
          />
          <div v-if="errors.newPassword" class="form-error">{{ errors.newPassword }}</div>
        </ProFormGroup>

        <ProFormGroup label="Confirm New Password" for="confirmPassword" required>
          <input
            id="confirmPassword"
            v-model="pageData.confirmPassword"
            class="form-input"
            type="password"
            placeholder="Confirm your new password"
          />
          <div v-if="errors.confirmPassword" class="form-error">{{ errors.confirmPassword }}</div>
        </ProFormGroup>

        <div class="password-requirements">
          <div class="requirements-title">Password Requirements</div>
          <div class="requirements-list">
            <div class="requirement-item" :class="{ met: validationState.hasMinLength }">
              <div class="requirement-check">
                <span v-if="validationState.hasMinLength">✓</span>
                <span v-else>✗</span>
              </div>
              <span>At least 8 characters</span>
            </div>

            <div class="requirement-item" :class="{ met: validationState.hasNoUserId }">
              <div class="requirement-check">
                <span v-if="validationState.hasNoUserId">✓</span>
                <span v-else>✗</span>
              </div>
              <span>No User ID in password</span>
            </div>

            <div class="requirement-item" :class="{ met: validationState.hasLowercase }">
              <div class="requirement-check">
                <span v-if="validationState.hasLowercase">✓</span>
                <span v-else>✗</span>
              </div>
              <span>At least one lowercase letter</span>
            </div>

            <div class="requirement-item" :class="{ met: validationState.hasUppercase }">
              <div class="requirement-check">
                <span v-if="validationState.hasUppercase">✓</span>
                <span v-else>✗</span>
              </div>
              <span>At least one uppercase letter</span>
            </div>

            <div class="requirement-item" :class="{ met: validationState.hasDigit }">
              <div class="requirement-check">
                <span v-if="validationState.hasDigit">✓</span>
                <span v-else>✗</span>
              </div>
              <span>At least one digit</span>
            </div>

            <div class="requirement-item" :class="{ met: validationState.hasSpecialChar }">
              <div class="requirement-check">
                <span v-if="validationState.hasSpecialChar">✓</span>
                <span v-else>✗</span>
              </div>
              <span>At least one special character</span>
            </div>

            <div class="requirement-item" :class="{ met: validationState.hasUniqueChars }">
              <div class="requirement-check">
                <span v-if="validationState.hasUniqueChars">✓</span>
                <span v-else>✗</span>
              </div>
              <span>At least 4 unique characters</span>
            </div>
          </div>
        </div>

        <div class="button-group mt-4">
          <button type="button" class="btn btn-secondary" @click.prevent="skipPage">
            <span>Skip</span>
          </button>
          <button
            type="submit"
            class="btn btn-primary"
            :class="{ 'btn-loading': loading }"
            :disabled="loading"
          >
            <span v-if="loading">Changing password...</span>
            <span v-else>Change Password</span>
          </button>
        </div>
      </ProGrid>
    </form>
    <div class="app-version">v{{ appVersion }}</div>
  </div>
</template>

<script setup lang="ts">
import { useChangePassword } from '@/composables/useChangePassword';
import { APP_VERSION } from '@/constants/version';

const appVersion = APP_VERSION;
const { pageData, changePassword, skipPage, errors, loading, validationState } =
  useChangePassword();
</script>
<style scoped>
.auth-login-container {
  max-width: 420px;
}
.form-error {
  color: #e53e3e;
  font-size: 0.75em;
  margin-top: 4px;
  margin-bottom: 2px;
}

.button-group {
  display: flex;
  gap: 12px;
  align-items: center;
}

.button-group .btn {
  flex: 1;
  min-width: 0;
}

.password-requirements {
  margin: 16px 0;
  background: #f8f9fa;
  border-radius: 6px;
  border: 1px solid #dee2e6;
  padding: 12px;
}

.requirements-title {
  font-weight: 600;
  font-size: 0.85em;
  color: #495057;
  margin-bottom: 8px;
}

.requirements-list {
  display: flex;
  flex-direction: column;
}

.requirement-item {
  display: flex;
  align-items: center;
  padding: 1px 0;
  transition: all 0.2s ease;
  font-size: 0.8em;
  color: #495057;
}

.requirement-check {
  width: 18px;
  height: 18px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-right: 8px;
  flex-shrink: 0;
  font-size: 1em;
  font-weight: bold;
  transition: all 0.2s ease;
}

.requirement-item.met .requirement-check {
  color: #28a745;
}

.requirement-item:not(.met) .requirement-check {
  color: #dc3545;
}

.requirement-item span {
  line-height: 1.3;
}

/* Mobile optimization */
@media (max-height: 600px) {
  .password-requirements {
    margin: 10px 0;
    padding: 8px;
  }

  .requirements-list {
    gap: 3px;
  }

  .requirement-item {
    padding: 4px 6px;
    font-size: 0.75em;
  }

  .auth-logo h1 {
    font-size: 1.5rem;
    margin-bottom: 0.5rem;
  }

  .auth-subtitle {
    font-size: 0.9rem;
    margin-bottom: 0.5rem;
  }

  .auth-login-container {
    padding: 1rem;
  }
}

@media (max-height: 500px) {
  .auth-logo {
    margin-bottom: 1rem;
  }

  .password-requirements {
    margin: 6px 0;
    padding: 6px;
  }

  .requirements-title {
    font-size: 0.8em;
    margin-bottom: 6px;
  }

  .button-group {
    gap: 8px;
  }

  .btn {
    padding: 8px 16px;
    font-size: 0.9rem;
  }
}

/* For very small screens, stack buttons vertically */
@media (max-width: 360px) {
  .button-group {
    flex-direction: column;
    gap: 8px;
  }

  .button-group .btn {
    width: 100%;
  }

  .password-requirements {
    padding: 6px;
  }

  .requirement-item {
    padding: 3px 4px;
    font-size: 0.7em;
  }

  .requirement-check {
    width: 14px;
    height: 14px;
    margin-right: 6px;
  }
}
</style>
