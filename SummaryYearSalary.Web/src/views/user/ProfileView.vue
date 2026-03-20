<template>
  <div class="card mb-3">
    <PageHeader />
  </div>
  <div class="profile-view">
    <div class="profile-container">
      <!-- Profile Information Card -->
      <div class="card mb-3">
        <div class="card-header">
          <h3 class="card-title">Personal Information</h3>
        </div>

        <ProMessage />

        <div class="profile-form">
          <ProGrid :columns="2">
            <div class="form-group">
              <label class="form-label" for="firstName">First Name</label>
              <input
                id="firstName"
                v-model="formData.firstName"
                class="form-input"
                type="text"
                readonly
              />
            </div>

            <div class="form-group">
              <label class="form-label" for="lastName">Last Name</label>
              <input
                id="lastName"
                v-model="formData.lastName"
                class="form-input"
                type="text"
                readonly
              />
            </div>

            <div class="form-group">
              <label class="form-label" for="email">Email Address</label>
              <input id="email" v-model="formData.email" class="form-input" type="email" readonly />
            </div>

            <div class="form-group">
              <label class="form-label" for="userName">Username</label>
              <input
                id="userName"
                v-model="formData.userName"
                class="form-input"
                type="text"
                readonly
              />
            </div>
          </ProGrid>
        </div>
      </div>

      <!-- Security Settings Card -->
      <div class="card mb-3">
        <div class="card-header">
          <h3 class="card-title">Security Settings</h3>
        </div>

        <div class="security-settings">
          <div class="setting-item">
            <div class="setting-info">
              <div class="setting-icon">
                <svg
                  xmlns="http://www.w3.org/2000/svg"
                  width="20"
                  height="20"
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
              <div>
                <h4 class="setting-title">Change Password</h4>
                <p class="setting-description">Update your password to keep your account secure</p>
              </div>
            </div>
            <button class="btn btn-outline" @click="navigateToChangePassword">Change</button>
          </div>

          <div class="setting-item">
            <div class="setting-info">
              <div class="setting-icon">
                <svg
                  xmlns="http://www.w3.org/2000/svg"
                  width="20"
                  height="20"
                  viewBox="0 0 24 24"
                  fill="none"
                  stroke="currentColor"
                  stroke-width="2"
                  stroke-linecap="round"
                  stroke-linejoin="round"
                >
                  <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"></path>
                </svg>
              </div>
              <div>
                <h4 class="setting-title">Two-Factor Authentication</h4>
                <p class="setting-description">
                  {{
                    userInfo?.metadata?.isMfaEnabled
                      ? 'MFA is currently enabled'
                      : 'Add an extra layer of security'
                  }}
                </p>
              </div>
            </div>
            <button
              v-if="!userInfo?.metadata?.isMfaEnabled"
              class="btn btn-outline"
              :disabled="loading"
              @click="enableMfa"
            >
              Enable MFA
            </button>
            <span v-else class="status-badge status-success">Enabled</span>
          </div>
        </div>
      </div>

      <!-- Account Information Card -->
      <div class="card">
        <div class="card-header">
          <h3 class="card-title">Account Information</h3>
        </div>

        <div class="account-info">
          <div class="info-item">
            <span class="info-label">User ID</span>
            <span class="info-value">{{ userInfo?.id }}</span>
          </div>
          <div class="info-item">
            <span class="info-label">Account Created</span>
            <span class="info-value">{{
              formatDate(userInfo?.metadata?.createdAt as string)
            }}</span>
          </div>
          <div class="info-item">
            <span class="info-label">Last Login</span>
            <span class="info-value">{{
              formatDate(userInfo?.metadata?.lastLogin as string) || 'N/A'
            }}</span>
          </div>
          <div class="info-item">
            <span class="info-label">Email Verified</span>
            <span class="info-value">
              <span v-if="userInfo?.metadata?.emailConfirmed" class="status-badge status-success"
                >Verified</span
              >
              <span v-else class="status-badge status-warning">Not Verified</span>
            </span>
          </div>
        </div>
      </div>
    </div>

    <!-- MFA Setup Modal -->
    <ProModal
      v-model:show="showMfaModal"
      title="Set Up Two-Factor Authentication"
      size="md"
      :show-footer="false"
      :close-on-overlay-click="true"
      @close="closeMfaModal"
    >
      <div class="mfa-instructions">
        <p class="instruction-text">
          Scan this QR code with your authenticator app (Google Authenticator, Microsoft
          Authenticator, Authy, etc.)
        </p>
      </div>

      <div class="qr-code-container">
        <canvas ref="qrCanvas"></canvas>
      </div>

      <div v-if="mfaData?.sharedKey" class="manual-entry">
        <p class="manual-entry-label">Or enter this code manually:</p>
        <div class="secret-code">
          <code>{{ mfaData.sharedKey }}</code>
          <button class="btn-copy" title="Copy to clipboard" @click="copySecret">
            <svg
              xmlns="http://www.w3.org/2000/svg"
              width="16"
              height="16"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              stroke-width="2"
              stroke-linecap="round"
              stroke-linejoin="round"
            >
              <rect x="9" y="9" width="13" height="13" rx="2" ry="2"></rect>
              <path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1"></path>
            </svg>
          </button>
        </div>
      </div>

      <div class="modal-actions">
        <button class="btn btn-primary" @click="closeMfaModal">Done</button>
      </div>
    </ProModal>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, reactive, nextTick } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '@/stores/authStore';
import { useGlobalUi } from '@/composables/useGlobalUi';
import { userService } from '@/services/userService';
import { toProblemDetails } from '@/utils/utilities';
import QRCode from 'qrcode';
import { type EnableMfaResponse } from '@/types';

const router = useRouter();

const authStore = useAuthStore();
const { loading, showLoading, hideLoading, showMessage } = useGlobalUi();

const userInfo = computed(() => authStore.user);

const showMfaModal = ref(false);
const mfaData = ref<EnableMfaResponse | null>(null);
const qrCanvas = ref<HTMLCanvasElement | null>(null);

const formData = reactive({
  firstName: '',
  lastName: '',
  email: '',
  userName: '',
});

const loadUserData = () => {
  if (userInfo.value) {
    formData.firstName = (userInfo.value.metadata?.firstName as string) || '';
    formData.lastName = (userInfo.value.metadata?.lastName as string) || '';
    formData.email = userInfo.value.email || '';
    formData.userName = (userInfo.value.metadata?.userName as string) || userInfo.value.email || '';
  }
};

const navigateToChangePassword = () => {
  router.push('/authorize/change-password');
};

onMounted(() => {
  loadUserData();
});

const enableMfa = async () => {
  showLoading();
  try {
    const { data } = await userService.enableMfa();
    if (data.isSuccess && data.value) {
      mfaData.value = data.value;
      showMfaModal.value = true;

      // Generate QR code after modal is shown
      await nextTick();
      await generateQrCode();

      showMessage(
        'MFA setup initiated. Please scan the QR code with your authenticator app.',
        'success'
      );
    } else {
      showMessage(data.error || 'Failed to enable MFA', 'error');
    }
  } catch (error: any) {
    console.error('Enable MFA error:', error);
    showMessage(toProblemDetails(error));
  } finally {
    hideLoading();
  }
};

const generateQrCode = async () => {
  if (!qrCanvas.value || !mfaData.value) return;

  try {
    // Use authenticatorUri from backend response
    const authenticatorUri = mfaData.value.authenticatorUri;

    await QRCode.toCanvas(qrCanvas.value, authenticatorUri, {
      width: 256,
      margin: 2,
      color: {
        dark: '#1a202c',
        light: '#ffffff',
      },
    });
  } catch (error) {
    console.error('QR code generation error:', error);
    showMessage('Failed to generate QR code', 'error');
  }
};

const closeMfaModal = () => {
  showMfaModal.value = false;
  mfaData.value = null;
};

const copySecret = async () => {
  if (!mfaData.value?.sharedKey) return;

  try {
    await navigator.clipboard.writeText(mfaData.value.sharedKey);
    showMessage('Secret copied to clipboard', 'success');
  } catch (error) {
    console.error('Copy error:', error);
    showMessage('Failed to copy secret', 'error');
  }
};

const formatDate = (date: string | undefined) => {
  if (!date) return 'N/A';
  return new Date(date).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  });
};
</script>

<style scoped>
/* Security Settings */
.security-settings {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.setting-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1.25rem;
  background: var(--surface-dark);
  border-radius: 10px;
  transition: all 0.2s ease;
}

.setting-item:hover {
  background: rgba(0, 0, 0, 0.06);
}

[data-theme='dark'] .setting-item:hover {
  background: rgba(255, 255, 255, 0.06);
}

.setting-info {
  display: flex;
  align-items: center;
  gap: 1rem;
  flex: 1;
}

.setting-icon {
  width: 40px;
  height: 40px;
  background: var(--surface);
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--primary-light);
  flex-shrink: 0;
}

.setting-title {
  font-size: 1rem;
  font-weight: 600;
  color: var(--text);
  margin: 0 0 0.25rem 0;
}

.setting-description {
  font-size: 0.85rem;
  color: var(--text-muted);
  margin: 0;
}

/* Account Information */
.account-info {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.info-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 1rem;
  background: var(--surface-dark);
  border-radius: 8px;
}

.info-label {
  font-size: 0.9rem;
  font-weight: 600;
  color: var(--text-light);
}

.info-value {
  font-size: 0.9rem;
  color: var(--text);
  font-weight: 500;
}

.status-badge {
  padding: 0.25rem 0.75rem;
  border-radius: 6px;
  font-size: 0.85rem;
  font-weight: 600;
}

.status-success {
  background: var(--success-light);
  color: var(--success-dark);
}

.status-warning {
  background: rgba(217, 119, 6, 0.15);
  color: var(--warning);
}

/* MFA Modal Styles */
.mfa-instructions {
  margin-bottom: 1.5rem;
}

.instruction-text {
  font-size: 0.95rem;
  color: var(--text-light);
  line-height: 1.6;
  margin: 0;
  text-align: center;
}

.qr-code-container {
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 2rem;
  background: var(--surface-dark);
  border-radius: 12px;
  margin-bottom: 1.5rem;
}

.qr-code-container canvas {
  border-radius: 8px;
  background: white;
  padding: 1rem;
}

.manual-entry {
  background: var(--surface-dark);
  padding: 1.25rem;
  border-radius: 10px;
  text-align: center;
  margin-bottom: 1.5rem;
}

.manual-entry-label {
  font-size: 0.85rem;
  color: var(--text-light);
  margin: 0 0 0.75rem 0;
  font-weight: 600;
}

.secret-code {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.75rem;
  background: var(--surface);
  padding: 0.75rem 1rem;
  border-radius: 8px;
  border: 2px dashed var(--border);
}

.secret-code code {
  font-family: 'Courier New', monospace;
  font-size: 1rem;
  color: var(--primary-light);
  font-weight: 600;
  letter-spacing: 0.05em;
}

.btn-copy {
  background: none;
  border: none;
  cursor: pointer;
  color: var(--primary-light);
  padding: 0.25rem;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 4px;
  transition: all 0.2s ease;
}

.btn-copy:hover {
  background: var(--surface-dark);
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  padding-top: 1rem;
  border-top: 1px solid var(--border);
}
</style>
