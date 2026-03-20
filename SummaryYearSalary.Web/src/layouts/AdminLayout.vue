<script setup lang="ts">
import { ref, onMounted, onUnmounted, provide, computed } from 'vue';
import { SecureStorage } from '@/utils/storage';
import { useGlobalUi } from '@/composables/useGlobalUi';
import { STORAGE_THEME } from '@/constants/storage';
import AdminSidebar from '@/components/admin/AdminSidebar.vue';
import AdminHeader from '@/components/admin/AdminHeader.vue';

const collapsed = ref(false);
const mobileOpen = ref(false);
const { loading } = useGlobalUi();

// Theme
const isDark = ref(false);
provide('isDark', isDark);
provide('toggleTheme', toggleTheme);

function toggleTheme() {
  isDark.value = !isDark.value;
  document.body.dataset.theme = isDark.value ? 'dark' : '';
  if (!isDark.value) delete document.body.dataset.theme;
  SecureStorage.setItem(STORAGE_THEME, isDark.value ? 'dark' : 'light');
}

const sidebarWidth = computed(() => (collapsed.value ? '64px' : '240px'));
const isMobile = () => window.matchMedia('(max-width: 767px)').matches;

function toggleCollapsed() {
  if (isMobile()) {
    mobileOpen.value = !mobileOpen.value;
  } else {
    collapsed.value = !collapsed.value;
  }
}

function closeMobileSidebar() {
  mobileOpen.value = false;
}

function handleResize() {
  if (isMobile()) {
    collapsed.value = false; // reset collapsed on mobile — sidebar hides via CSS transform, not collapse prop
    mobileOpen.value = false;
  } else {
    mobileOpen.value = false;
  }
}

onMounted(() => {
  const savedTheme = SecureStorage.getItem(STORAGE_THEME);
  if (savedTheme === 'dark') {
    isDark.value = true;
    document.body.dataset.theme = 'dark';
  }
  if (isMobile()) collapsed.value = false; // ensure expanded mode on initial mobile load
  window.addEventListener('resize', handleResize);
});

onUnmounted(() => {
  window.removeEventListener('resize', handleResize);
});
</script>

<template>
  <ProAlert />
  <ProConfirm />
  <ProLoading v-if="loading" :loading="loading" />

  <!-- Mobile overlay -->
  <div v-if="mobileOpen" class="mobile-overlay" @click="closeMobileSidebar" />

  <el-container class="app-layout">
    <!-- Sidebar -->
    <el-aside
      :width="sidebarWidth"
      :class="['app-sidebar', { 'mobile-open': mobileOpen, 'is-collapsed': collapsed }]"
    >
      <AdminSidebar :collapsed="collapsed" @close="closeMobileSidebar" />
    </el-aside>

    <!-- Main area -->
    <el-container direction="vertical" class="app-body">
      <el-header height="60px" class="app-header">
        <AdminHeader :collapsed="collapsed" @toggle="toggleCollapsed" />
      </el-header>
      <el-main class="app-main">
        <router-view />
      </el-main>
    </el-container>
  </el-container>
</template>

<style scoped>
.app-layout {
  height: 100vh;
  overflow: hidden;
}

.app-sidebar {
  height: 100vh;
  overflow: hidden;
  background: var(--sidebar-bg, #ffffff);
  border-right: 1px solid var(--border);
  transition: width 0.25s ease;
  flex-shrink: 0;
}

/* Mobile: sidebar hidden off-screen by default */
@media (max-width: 767px) {
  .app-sidebar {
    position: fixed;
    left: 0;
    top: 0;
    z-index: 1000;
    width: 240px !important;
    transform: translateX(-100%);
    transition: transform 0.25s ease;
  }
  .app-sidebar.mobile-open {
    transform: translateX(0);
  }
}

.mobile-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.4);
  z-index: 999;
}

.app-body {
  flex: 1;
  overflow: hidden;
  min-width: 0;
  height: 100%;
}

.app-header {
  padding: 0;
  border-bottom: 1px solid var(--border);
  background: #ffffff;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.06);
  display: flex;
  align-items: center;
  flex-shrink: 0;
  z-index: 100;
}

[data-theme='dark'] .app-header {
  background: #1e1e2e;
}

.app-main {
  --el-main-padding: 0;
  padding: 20px;
  background: #f5f7fa;
  overflow-y: auto;
  flex: 1;
  min-height: 0;
  box-sizing: border-box;
}

[data-theme='dark'] .app-main {
  background: #18181b;
}
</style>
