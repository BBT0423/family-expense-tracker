import { defineStore } from 'pinia';
import { ref } from 'vue';
import { userService } from '@/services/userService';
import type { UserDirectoryEntry } from '@/types';

/**
 * Name + email of every other user, open to any authenticated user (unlike
 * userManagementStore, which is the Admin-only full user table). Shared by
 * the Dashboard/Reports scope selector and the "log for someone else" form.
 */
export const useUserDirectoryStore = defineStore('userDirectory', () => {
  const entries = ref<UserDirectoryEntry[]>([]);
  const loading = ref(false);
  const loaded = ref(false);

  async function fetchDirectory() {
    if (loaded.value) return;
    loading.value = true;
    try {
      const { data } = await userService.getDirectory();
      if (data?.isSuccess && data.value) {
        entries.value = data.value;
        loaded.value = true;
      }
    } finally {
      loading.value = false;
    }
  }

  return { entries, loading, fetchDirectory };
});
