<template>
  <div class="header-inner">
    <!-- Left: Toggle + Page Title -->
    <div class="header-left">
      <el-button text circle class="toggle-btn" @click="$emit('toggle')">
        <el-icon size="20">
          <Expand v-if="props.collapsed" />
          <Fold v-else />
        </el-icon>
      </el-button>

      <span v-if="pageTitle" class="header-page-title">{{ pageTitle }}</span>
    </div>

    <!-- Right: Theme + User -->
    <div class="header-right">
      <!-- Theme toggle -->
      <el-tooltip :content="isDark ? 'Light mode' : 'Dark mode'" placement="bottom">
        <el-button text circle @click="toggleTheme()">
          <el-icon size="18">
            <Sunny v-if="isDark" />
            <Moon v-else />
          </el-icon>
        </el-button>
      </el-tooltip>

      <!-- User dropdown -->
      <el-dropdown trigger="click" @command="handleCommand">
        <div class="user-trigger">
          <el-avatar :size="32" class="user-avatar">{{ initials }}</el-avatar>
          <span class="user-name">{{ displayName }}</span>
          <el-icon size="12" class="text-muted"><ArrowDown /></el-icon>
        </div>
        <template #dropdown>
          <el-dropdown-menu>
            <div class="dropdown-user-info">
              <div class="dropdown-name">{{ user?.name || 'User' }}</div>
              <div class="dropdown-email">{{ user?.email }}</div>
            </div>
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
    </div>
  </div>

  <NetworkStatusBanner />
</template>

<script setup lang="ts">
import { computed, inject } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useAuthStore } from '@/stores/authStore';

const props = defineProps<{ collapsed?: boolean }>();
defineEmits(['toggle']);

const route = useRoute();
const router = useRouter();
const authStore = useAuthStore();
const { user } = authStore;

const isDark = inject<{ value: boolean }>('isDark');
const toggleTheme = inject<() => void>('toggleTheme', () => {});

const pageTitle = computed(() => route.meta?.title as string | undefined);

const initials = computed(() => {
  const name = user?.name || user?.email || '';
  return name
    .split(/\s+/)
    .slice(0, 2)
    .map((w) => w[0]?.toUpperCase() ?? '')
    .join('');
});

const displayName = computed(() => {
  const name = user?.name || '';
  return name.length > 20 ? name.slice(0, 20) + '…' : name || user?.email || 'User';
});

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
.header-inner {
  display: flex;
  align-items: center;
  justify-content: space-between;
  width: 100%;
  padding: 0 16px;
  height: 60px;
}

.header-left {
  display: flex;
  align-items: center;
  gap: 12px;
}

.toggle-btn {
  color: var(--text);
}

.header-page-title {
  font-size: 0.95rem;
  font-weight: 600;
  color: var(--text);
  white-space: nowrap;
}

.header-right {
  display: flex;
  align-items: center;
  gap: 4px;
}

/* User trigger */
.user-trigger {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  padding: 4px 8px;
  border-radius: 8px;
  transition: background 0.15s;
}

.user-trigger:hover {
  background: var(--surface-dark);
}

.user-avatar {
  background: var(--primary) !important;
  color: #fff !important;
  font-size: 0.8rem;
  font-weight: 700;
  flex-shrink: 0;
}

.user-name {
  font-size: 0.875rem;
  font-weight: 500;
  color: var(--text);
  max-width: 140px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

@media (max-width: 640px) {
  .user-name {
    display: none;
  }
}

/* Dropdown user info header */
.dropdown-user-info {
  padding: 10px 16px 8px;
  border-bottom: 1px solid var(--border);
  margin-bottom: 4px;
}

.dropdown-name {
  font-weight: 600;
  font-size: 0.875rem;
  color: var(--text);
}

.dropdown-email {
  font-size: 0.75rem;
  color: var(--text-muted);
  margin-top: 2px;
}

:deep(.logout-item) {
  color: var(--danger) !important;
}

:deep(.logout-item .el-icon) {
  color: var(--danger) !important;
}
</style>
