import { useRouter } from 'vue-router';
import { useGlobalUi } from './useGlobalUi';
import { reactive, watch } from 'vue';
import type { ChangePasswordRequest } from '@/types/authorize';
import { toProblemDetails } from '@/utils/utilities';
import { userService } from '@/services/userService';
import { useAuthStore } from '@/stores/authStore';
import { validatePasswordStrength } from '@/utils/passwordValidation';

const REQUIRED_FIELD_SUFFIX = ' is required.';
const CURRENT_CREDENTIAL_LABEL = 'Current password';
const NEW_CREDENTIAL_LABEL = 'New password';
const CONFIRMATION_MISMATCH_MESSAGE = 'Values do not match.';

export const useChangePassword = () => {
  const router = useRouter();
  const { loading, showLoading, hideLoading, showMessage, clearAll } = useGlobalUi();
  const authStore = useAuthStore();
  const { user } = authStore;
  const pageData = reactive<ChangePasswordRequest>({
    currentPassword: '',
    newPassword: '',
    confirmPassword: '',
    userId: user?.id || '',
  });

  const errors = reactive({
    currentPassword: '',
    newPassword: '',
    confirmPassword: '',
  });

  // Reactive validation states for each requirement
  const validationState = reactive({
    hasMinLength: false, // >= 8 characters
    hasLowercase: false, // At least one lowercase letter
    hasUppercase: false, // At least one uppercase letter
    hasDigit: false, // At least one digit
    hasSpecialChar: false, // At least one special character
    hasNoUserId: true, // Password should not contain user ID
    hasUniqueChars: false, // At least 4 unique characters
  });

  // Watch password changes and update validation states
  watch(
    () => pageData.newPassword,
    (password) => {
      // Reset errors when password changes
      errors.newPassword = '';
      errors.confirmPassword = '';
      errors.currentPassword = '';
      // Clear global messages
      clearAll();

      // Check minimum length (RequiredLength = 8)
      validationState.hasMinLength = password.length >= 8;

      // Check lowercase (RequireLowercase = true)
      validationState.hasLowercase = /[a-z]/.test(password);

      // Check uppercase (RequireUppercase = true)
      validationState.hasUppercase = /[A-Z]/.test(password);

      // Check digit (RequireDigit = true)
      validationState.hasDigit = /\d/.test(password);

      // Check special character (RequireNonAlphanumeric = true)
      validationState.hasSpecialChar = /[!@#$%^&*()_+\-=[\]{};':"\\|,.<>/?~`]/.test(password);

      // Check User ID not in password
      validationState.hasNoUserId =
        !user?.id || !password.toLowerCase().includes(user.id.toLowerCase());

      // Check unique characters (RequiredUniqueChars = 4)
      const uniqueChars = new Set(password).size;
      validationState.hasUniqueChars = uniqueChars >= 4;
    }
  );

  const validate = (): boolean => {
    errors.currentPassword = '';
    errors.newPassword = '';
    errors.confirmPassword = '';
    let valid = true;

    if (!pageData.currentPassword) {
      errors.currentPassword = `${CURRENT_CREDENTIAL_LABEL}${REQUIRED_FIELD_SUFFIX}`;
      valid = false;
    }

    if (pageData.newPassword) {
      const passwordValidation = validatePasswordStrength(pageData.newPassword, user?.id);
      if (!passwordValidation.isValid) {
        errors.newPassword = passwordValidation.error;
        valid = false;
      }
    } else {
      errors.newPassword = `${NEW_CREDENTIAL_LABEL}${REQUIRED_FIELD_SUFFIX}`;
      valid = false;
    }

    if (pageData.confirmPassword !== pageData.newPassword) {
      errors.confirmPassword = CONFIRMATION_MISMATCH_MESSAGE;
      valid = false;
    }

    return valid;
  };

  const changePassword = async () => {
    if (!validate()) {
      showMessage('Please fix the errors before submitting.');
      return;
    }

    showLoading();
    try {
      const { data } = await userService.changePassword({
        userId: pageData.userId,
        currentPassword: pageData.currentPassword,
        newPassword: pageData.newPassword,
        confirmPassword: pageData.confirmPassword,
      });

      if (data.isSuccess) {
        showMessage('Password changed successfully.', 'success');
        // Navigate back to profile or pages
        router.push('/pages/profile');
      } else {
        showMessage(data.error || 'Failed to change password', 'error');
      }
    } catch (error: any) {
      console.error('Change password error:', error);
      showMessage(toProblemDetails(error));
    } finally {
      hideLoading();
    }
  };

  const skipPage = () => {
    router.push('/pages/profile');
  };

  return {
    pageData,
    errors,
    validationState,
    validate,
    skipPage,
    changePassword,
    loading,
  };
};
