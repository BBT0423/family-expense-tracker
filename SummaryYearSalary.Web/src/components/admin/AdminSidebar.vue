<template>
  <div class="sidebar-wrap" :class="{ 'is-collapsed': props.collapsed }">
    <!-- Logo -->
    <div class="sidebar-logo">
      <el-icon size="26" class="logo-icon"><Wallet /></el-icon>
      <span v-if="!props.collapsed" class="logo-text">Family Expense</span>
    </div>

    <!-- Nav: collapsed mode = custom icon buttons -->
    <div v-if="props.collapsed" class="nav-collapsed">
      <template v-for="section in menuSections" :key="section.title">
        <el-tooltip
          v-for="item in section.items"
          :key="item.path"
          :content="item.label"
          placement="right"
          :show-after="200"
        >
          <div
            class="nav-icon-btn"
            :class="{ active: isActive(item.path) }"
            @click="navigate(item.path)"
          >
            <el-icon size="18"><component :is="item.icon" /></el-icon>
          </div>
        </el-tooltip>
      </template>
    </div>

    <!-- Nav: expanded mode = el-menu -->
    <el-scrollbar v-else class="sidebar-scrollbar">
      <el-menu :default-active="route.path" router :collapse-transition="false" class="sidebar-menu">
        <template v-for="section in menuSections" :key="section.title">
          <div class="menu-section-label">{{ section.title }}</div>
          <el-menu-item
            v-for="item in section.items"
            :key="item.path"
            :index="item.path"
            @click="emit('close')"
          >
            <el-icon><component :is="item.icon" /></el-icon>
            <template #title>{{ item.label }}</template>
          </el-menu-item>
        </template>
      </el-menu>
    </el-scrollbar>

    <!-- Footer -->
    <div class="sidebar-footer">
      <el-icon v-if="props.collapsed" size="14"><InfoFilled /></el-icon>
      <span v-else>v{{ appVersion }}</span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useRoute, useRouter } from 'vue-router';
import { APP_VERSION } from '@/constants/version';

const props = defineProps<{ collapsed?: boolean }>();
const emit = defineEmits(['close']);
const route = useRoute();
const router = useRouter();
const appVersion = APP_VERSION;

const menuSections = [
  {
    title: 'GENERAL',
    items: [{ path: '/pages/users/profile', label: 'User Profile', icon: 'UserFilled' }],
  },
  {
    title: 'DEV',
    items: [{ path: '/pages/ui-test', label: 'UI Test', icon: 'Monitor' }],
  },
];

const isActive = (path: string) => route.path === path || route.path.startsWith(path + '/');

function navigate(path: string) {
  router.push(path);
  emit('close');
}
</script>

<style scoped>
.sidebar-wrap {
  display: flex;
  flex-direction: column;
  height: 100%;
  background: var(--sidebar-bg, #ffffff);
}

[data-theme='dark'] .sidebar-wrap {
  background: #1e1e2e;
}

/* ── Logo ─────────────────────────────── */
.sidebar-logo {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 0 16px;
  height: 60px;
  border-bottom: 1px solid var(--border);
  flex-shrink: 0;
  overflow: hidden;
  transition: padding 0.25s ease;
}

.is-collapsed .sidebar-logo {
  justify-content: center;
  padding: 0;
}

.logo-icon {
  color: var(--primary);
  flex-shrink: 0;
}

.logo-text {
  font-size: 1rem;
  font-weight: 700;
  color: var(--text);
  white-space: nowrap;
  overflow: hidden;
}

/* ── Collapsed nav (custom icon buttons) ─ */
.nav-collapsed {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 8px 0;
  overflow-y: auto;
}

.nav-icon-btn {
  width: 40px;
  height: 40px;
  border-radius: 10px;
  margin: 2px 0;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  color: var(--text);
  transition: background 0.15s ease;
  flex-shrink: 0;
}

.nav-icon-btn:hover:not(.active) {
  background: rgba(0, 84, 233, 0.1);
}

.nav-icon-btn.active {
  background: var(--primary);
  color: #ffffff;
}

.nav-icon-btn.active :deep(.el-icon) {
  color: #ffffff;
}

/* ── Expanded nav (el-menu) ───────────── */
.sidebar-scrollbar {
  flex: 1;
  overflow: hidden;
}

:deep(.el-menu) {
  --el-menu-bg-color: transparent;
  background-color: transparent !important;
  border-right: none !important;
}

:deep(.el-menu-item) {
  height: 44px;
  margin: 2px 8px;
  border-radius: 8px;
  width: calc(100% - 16px);
  color: var(--text);
}

:deep(.el-menu-item.is-active) {
  background: var(--primary) !important;
  color: #ffffff !important;
}

:deep(.el-menu-item:hover:not(.is-active)) {
  background: rgba(0, 84, 233, 0.08) !important;
}

.menu-section-label {
  font-size: 0.7rem;
  font-weight: 700;
  letter-spacing: 0.08em;
  color: var(--text-light);
  padding: 16px 20px 4px;
  text-transform: uppercase;
  opacity: 0.7;
}

/* ── Footer ──────────────────────────── */
.sidebar-footer {
  display: flex;
  align-items: center;
  padding: 10px 20px;
  font-size: 0.72rem;
  color: var(--text-muted);
  border-top: 1px solid var(--border);
  flex-shrink: 0;
}

.is-collapsed .sidebar-footer {
  justify-content: center;
  padding: 10px;
}
</style>
