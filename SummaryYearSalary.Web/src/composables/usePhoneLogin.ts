import { reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import { useGlobalUi } from '@/composables/useGlobalUi';
import { useAuthStore } from '@/stores/authStore';
import { authService } from '@/services/authService';
import { toProblemDetails } from '@/utils/utilities';
import { STORAGE_LAST_ACTIVITY } from '@/constants/storage';

export const usePhoneLogin = () => {
  const router = useRouter();
  const { showLoading, hideLoading, showMessage } = useGlobalUi();
  const { setAuth } = useAuthStore();

  const phoneData = reactive({
    phoneNumber: '',
    otpCode: '',
  });

  const phoneErrors = ref({
    phoneNumber: '',
    otpCode: '',
  });

  const phoneStep = ref<'phone' | 'otp'>('phone');
  const phoneLoading = ref(false);
  const resendCooldown = ref(0);
  let cooldownTimer: ReturnType<typeof setInterval> | null = null;

  const validatePhone = (): boolean => {
    phoneErrors.value.phoneNumber = '';
    if (!phoneData.phoneNumber) {
      phoneErrors.value.phoneNumber = 'กรุณากรอกเบอร์โทรศัพท์';
      return false;
    }
    if (!/^(\+66|0)\d{8,9}$/.test(phoneData.phoneNumber)) {
      phoneErrors.value.phoneNumber = 'เบอร์โทรศัพท์ไม่ถูกต้อง (เช่น 0812345678)';
      return false;
    }
    return true;
  };

  const validateOtp = (): boolean => {
    phoneErrors.value.otpCode = '';
    if (!phoneData.otpCode) {
      phoneErrors.value.otpCode = 'กรุณากรอกรหัส OTP';
      return false;
    }
    if (!/^\d{6}$/.test(phoneData.otpCode)) {
      phoneErrors.value.otpCode = 'รหัส OTP ต้องเป็นตัวเลข 6 หลัก';
      return false;
    }
    return true;
  };

  const startCooldown = (seconds = 60) => {
    resendCooldown.value = seconds;
    if (cooldownTimer) clearInterval(cooldownTimer);
    cooldownTimer = setInterval(() => {
      resendCooldown.value--;
      if (resendCooldown.value <= 0 && cooldownTimer) {
        clearInterval(cooldownTimer);
        cooldownTimer = null;
      }
    }, 1000);
  };

  const sendOtp = async () => {
    if (!validatePhone()) return;

    phoneLoading.value = true;
    showLoading();
    try {
      const { data } = await authService.sendPhoneOtp({ phoneNumber: phoneData.phoneNumber });
      if (data.isSuccess) {
        phoneStep.value = 'otp';
        phoneData.otpCode = '';
        startCooldown(60);
        showMessage('ส่งรหัส OTP ไปยัง email แล้ว กรุณาตรวจสอบกล่องจดหมาย', 'info');
      } else {
        showMessage(data.error ?? 'เกิดข้อผิดพลาด กรุณาลองใหม่', 'error');
      }
    } catch (e: any) {
      showMessage(toProblemDetails(e));
    } finally {
      phoneLoading.value = false;
      hideLoading();
    }
  };

  const verifyOtp = async () => {
    if (!validateOtp()) return;

    phoneLoading.value = true;
    showLoading();
    try {
      const { data } = await authService.verifyPhoneOtp({
        phoneNumber: phoneData.phoneNumber,
        otpCode: phoneData.otpCode,
      });

      if (data.isSuccess && data.value) {
        setAuth({ ...data.value, user: { ...data.value.user, loginProvider: 'phone' } });
        localStorage.removeItem(STORAGE_LAST_ACTIVITY);

        const redirectUrl = sessionStorage.getItem('REDIRECT_URL');
        if (redirectUrl) {
          sessionStorage.removeItem('REDIRECT_URL');
          await router.push(redirectUrl);
        } else {
          await router.push('/pages');
        }
        globalThis.location.reload();
      } else {
        phoneErrors.value.otpCode = data.error ?? 'รหัส OTP ไม่ถูกต้อง';
      }
    } catch (e: any) {
      showMessage(toProblemDetails(e));
    } finally {
      phoneLoading.value = false;
      hideLoading();
    }
  };

  const handlePhoneStep = async () => {
    if (phoneStep.value === 'phone') {
      await sendOtp();
    } else {
      await verifyOtp();
    }
  };

  const resendOtp = async () => {
    if (resendCooldown.value > 0) return;
    phoneData.otpCode = '';
    await sendOtp();
  };

  return {
    phoneData,
    phoneLoading,
    phoneErrors,
    phoneStep,
    resendCooldown,
    handlePhoneStep,
    resendOtp,
  };
};
