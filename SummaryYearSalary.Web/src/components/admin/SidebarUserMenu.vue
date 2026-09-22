<template>
  <el-dropdown trigger="click" placement="top-start" style="width: 100%" @command="handleCommand">
    <div class="user-menu" role="button" tabindex="0">
      <div class="user-avatar">{{ initials }}</div>
      <div class="user-menu-text">
        <div class="user-menu-name">{{ displayName }}</div>
        <div class="user-menu-email">{{ user?.email }}</div>
      </div>
      <el-icon class="user-menu-chevron" size="14"><ArrowUp /></el-icon>
    </div>
    <template #dropdown>
      <el-dropdown-menu>
        <el-dropdown-item command="profile">
          <el-icon><UserFilled /></el-icon>
          Profile
        </el-dropdown-item>
        <el-dropdown-item command="password">
          <el-icon><Lock /></el-icon>
          Change Password
        </el-dropdown-item>
        <el-dropdown-item divided command="logout" class="logout-item">
          <el-icon><SwitchButton /></el-icon>
          Sign Out
        </el-dropdown-item>
      </el-dropdown-menu>
    </template>
  </el-dropdown>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '@/stores/authStore';

const router = useRouter();
const authStore = useAuthStore();
const user = computed(() => authStore.user);

const initials = computed(() => {
  const name = user.value?.name || user.value?.email || '';
  return (
    name
      .split(/\s+/)
      .slice(0, 2)
      .map((w) => w[0]?.toUpperCase() ?? '')
      .join('') || '?'
  );
});

const displayName = computed(() => user.value?.name || user.value?.email || 'User');

function handleCommand(cmd: string) {
  if (cmd === 'profile') router.push('/pages/users/profile');
  else if (cmd === 'password') router.push('/authorize/change-password');
  else if (cmd === 'logout') {
    authStore.clearAuth();
    router.push('/authorize/login');
  }
}
</script>

<style scoped>
:deep(.logout-item),
:deep(.logout-item .el-icon) {
  color: var(--danger) !important;
}
</style>
