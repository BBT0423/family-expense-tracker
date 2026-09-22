import { defineStore } from 'pinia';
import { ref } from 'vue';
import { userService } from '@/services/userService';
import type { CreateUserRequest, UpdateUserRequest, UserProfile } from '@/types';

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

  async function createUser(payload: CreateUserRequest) {
    const { data } = await userService.create(payload);
    if (data?.isSuccess) await fetchUsers();
    return data;
  }

  async function updateUser(id: string, payload: UpdateUserRequest) {
    const { data } = await userService.update(id, payload);
    if (data?.isSuccess) await fetchUsers();
    return data;
  }

  async function updateUserRole(id: string, role: 'Admin' | 'User') {
    const { data } = await userService.updateRole(id, role);
    if (data?.isSuccess) await fetchUsers();
    return data;
  }

  return { users, loading, fetchUsers, createUser, updateUser, updateUserRole };
});
