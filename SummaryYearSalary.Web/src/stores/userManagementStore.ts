import { defineStore } from 'pinia';
import { ref } from 'vue';
import { userService } from '@/services/userService';
import type { UserProfile } from '@/types';

export const useUserManagementStore = defineStore('userManagement', () => {
  const users = ref<UserProfile[]>([]);
  const loading = ref(false);

  async function fetchUsers() {
    loading.value = true;
    try {
      const { data } = await userService.getAll();
      if (data?.isSuccess && data.value) {
        users.value = data.value;
      }
    } finally {
      loading.value = false;
    }
  }

  return { users, loading, fetchUsers };
});
