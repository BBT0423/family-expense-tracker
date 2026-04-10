import { reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import { useGlobalUi } from '@/composables/useGlobalUi';
import { useAuthStore } from '@/stores/authStore';
import { authService } from '@/services/authService';
import type { LoginRequest } from '@/types/authorize';
import { toProblemDetails } from '@/utils/utilities';
import { STORAGE_LAST_ACTIVITY } from '@/constants/storage';

const REQUIRED_FIELD_SUFFIX = ' is required.';
const CREDENTIAL_LABEL = 'Password';

export const useLogin = () => {
  const router = useRouter();
  const { loading, showLoading, hideLoading, showMessage } = useGlobalUi();
  const { setAuth, isAuthenticated, user } = useAuthStore();
  const pageData = reactive<LoginRequest>({
    email: '',
    password: '',
  });

  const errors = ref({
    email: '',
    password: '',
  });

  const validate = (): boolean => {
    errors.value.email = '';
    errors.value.password = '';
    let valid = true;
    if (!pageData.email) {
      errors.value.email = 'Email or username is required.';
      valid = false;
    } else if (pageData.email.includes('@')) {
      const email = pageData.email;
      const atIndex = email.indexOf('@');
      const lastAtIndex = email.lastIndexOf('@');
      const dotIndex = email.lastIndexOf('.');
      const hasWhitespace = email !== email.trim() || email.includes(' ');
      const isInvalidEmail =
        hasWhitespace ||
        atIndex <= 0 ||
        atIndex !== lastAtIndex ||
        dotIndex <= atIndex + 1 ||
        dotIndex >= email.length - 1;

      if (isInvalidEmail) {
        errors.value.email = 'Invalid email address.';
        valid = false;
      }
    }
    if (!pageData.password) {
      errors.value.password = `${CREDENTIAL_LABEL}${REQUIRED_FIELD_SUFFIX}`;
      valid = false;
    }
    return valid;
  };

  /**
   * Initialize auth state and navigate to appropriate page
   * Returns the destination path for logging/debugging
   */
  const initAuth = async (): Promise<string> => {
    localStorage.removeItem(STORAGE_LAST_ACTIVITY);

    // Handle external callback (ICSS Web, etc.)
    const callbackUrl = router.currentRoute.value.query.callback as string | undefined;
    if (callbackUrl) {
      await router.push('/external-connect');
      return '/external-connect';
    }

    // Navigate to admin pages (profile as default)
    await router.push('/pages');
    return '/pages';
  };

  const login = async () => {
    if (!validate()) return;

    showLoading();
    try {
      const { data } = await authService.login(pageData);

      if (data.isSuccess && data.value) {
        // Check if MFA is enabled
        if (data.value.isMfaEnabled) {
          // User needs to verify OTP
          hideLoading();
          showMessage('Please enter the verification code from your authenticator app', 'info');
          await router.push({
            name: 'Otp',
            query: {
              userId: data.value.user.id,
            },
          });
          return;
        }

        // No MFA required - set auth state and navigate
        setAuth({ ...data.value, user: { ...data.value.user, loginProvider: 'email' } });

        // Navigate to appropriate destination
        const destination = await initAuth();
        // Optional: Log for debugging
        console.log('[Login] Navigated to:', destination);
        globalThis.location.reload(); // Reload to ensure all state is fresh
      } else {
        // Handle login failure
        showMessage(data.error ?? 'Login failed', 'error');
      }
    } catch (e: any) {
      // Handle network/server errors
      console.error('❌ Login error:', e);
      showMessage(toProblemDetails(e));
    } finally {
      hideLoading();
    }
  };

  const continueWithUser = async () => {
    await initAuth();
    showMessage('Login successful', 'success');
  };

  return {
    // state
    pageData,
    loading,
    errors,
    isAuthenticated,
    user,

    // methods
    login,
    validate,
    continueWithUser,
  };
};
