import { reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import { useGlobalUi } from '@/composables/useGlobalUi';
import { useAuthStore } from '@/stores/authStore';
import { httpClient } from '@/utils/httpClient';
import { toProblemDetails } from '@/utils/utilities';

export const useRegister = () => {
  const router = useRouter();
  const { showLoading, hideLoading, showMessage } = useGlobalUi();
  const { setAuth } = useAuthStore();

  const form = reactive({
    firstName: '',
    lastName: '',
    email: '',
    password: '',
    confirmPassword: '',
  });

  const errors = ref({
    firstName: '',
    email: '',
    password: '',
    confirmPassword: '',
  });

  const loading = ref(false);

  const validate = (): boolean => {
    errors.value = { firstName: '', email: '', password: '', confirmPassword: '' };
    let valid = true;

    if (!form.firstName.trim()) {
      errors.value.firstName = 'กรุณากรอกชื่อ';
      valid = false;
    }

    if (!form.email) {
      errors.value.email = 'กรุณากรอก email';
      valid = false;
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email)) {
      errors.value.email = 'รูปแบบ email ไม่ถูกต้อง';
      valid = false;
    }

    if (!form.password) {
      errors.value.password = 'กรุณากรอกรหัสผ่าน';
      valid = false;
    } else if (form.password.length < 8) {
      errors.value.password = 'รหัสผ่านต้องมีอย่างน้อย 8 ตัวอักษร';
      valid = false;
    }

    if (!form.confirmPassword) {
      errors.value.confirmPassword = 'กรุณายืนยันรหัสผ่าน';
      valid = false;
    } else if (form.password !== form.confirmPassword) {
      errors.value.confirmPassword = 'รหัสผ่านไม่ตรงกัน';
      valid = false;
    }

    return valid;
  };

  const register = async () => {
    if (!validate()) return;

    loading.value = true;
    showLoading();
    try {
      const { data } = await httpClient.post('/api/authorize/register', {
        firstName: form.firstName,
        lastName: form.lastName,
        email: form.email,
        password: form.password,
        confirmPassword: form.confirmPassword,
      });

      if (data.isSuccess && data.value) {
        setAuth(data.value);
        await router.push('/pages');
        globalThis.location.reload();
      } else {
        showMessage(data.error ?? 'สมัครสมาชิกไม่สำเร็จ กรุณาลองใหม่', 'error');
      }
    } catch (e: any) {
      showMessage(toProblemDetails(e));
    } finally {
      loading.value = false;
      hideLoading();
    }
  };

  return { form, errors, loading, register };
};
