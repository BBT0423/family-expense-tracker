<template>
  <div class="user-dropdown" :class="{ show: props.open }">
    <div class="dropdown-header">
      <RouterLink to="/pages/users/profile" style="text-decoration: none">
        <UserInfo :name="props.user?.name" :email="props.user?.email" />
      </RouterLink>
    </div>
    <div class="dropdown-menu">
      <RouterLink v-for="item in menuItems" :key="item.label" :to="item.href" class="dropdown-item">
        <span>{{ item.icon }}</span>
        {{ item.label }}
      </RouterLink>
      <div class="dropdown-divider"></div>
      <button type="button" class="dropdown-item danger font-bold" @click="onSignOut">
        <span>{{ signOutItem.icon }}</span>
        {{ signOutItem.label }}
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useGlobalUi } from '@/composables/useGlobalUi';
import { useRouter, RouterLink } from 'vue-router';
import { resetStoresForLogout } from '@/utils/storeReset';

const props = defineProps<{
  open: boolean;
  user: {
    id: string;
    email: string;
    name: string;
    avatar: string;
    metadata: Record<string, any>;
  } | null;
}>();
const { showConfirm } = useGlobalUi();

const router = useRouter();

const menuItems = [{ icon: '🔒', label: 'Change password', href: '/authorize/change-password' }];

const signOutItem = { icon: '➡️', label: 'Sign Out', href: '#', danger: true };

function onSignOut() {
  showConfirm('Are you sure you want to sign out?', {
    title: 'Confirm Sign Out',
    confirmText: 'Sign Out',
    cancelText: 'Cancel',
    onConfirm: async () => {
      // Use proper store reset for logout
      await resetStoresForLogout();
      router.push('/authorize/login');
    },
  });
}
</script>
<style scoped>
button {
  width: 100%;
  text-align: left;
  background: transparent;
  border: 0;
  cursor: pointer;
}
</style>
