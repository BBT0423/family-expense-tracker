<script setup lang="ts">
import { ref, onMounted, onUnmounted, provide } from 'vue';
import { SecureStorage } from '@/utils/storage';
import { useGlobalUi } from '@/composables/useGlobalUi';
import { STORAGE_THEME } from '@/constants/storage';
import AdminSidebar from '@/components/admin/AdminSidebar.vue';
import AdminHeader from '@/components/admin/AdminHeader.vue';

const sidebarOpen = ref(false); // mobile/tablet drawer
const sidebarCollapsed = ref(false); // desktop collapse
const { loading } = useGlobalUi();

// Theme (shared with header)
const isDark = ref(false);
provide('isDark', isDark);
provide('toggleTheme', toggleTheme);

function toggleTheme() {
  isDark.value = !isDark.value;
  if (isDark.value) {
    document.body.dataset.theme = 'dark';
    SecureStorage.setItem(STORAGE_THEME, 'dark');
  } else {
    delete document.body.dataset.theme;
    SecureStorage.setItem(STORAGE_THEME, 'light');
  }
}

const isCompact = () => globalThis.innerWidth <= 1024;

function handleToggleSidebar() {
  if (isCompact()) sidebarOpen.value = !sidebarOpen.value;
  else sidebarCollapsed.value = !sidebarCollapsed.value;
}

function handleCloseSidebar() {
  sidebarOpen.value = false;
}

function handleResize() {
  if (isCompact()) {
    sidebarCollapsed.value = false;
  } else {
    sidebarOpen.value = false;
  }
}

onMounted(() => {
  const savedTheme = SecureStorage.getItem(STORAGE_THEME);
  if (savedTheme === 'dark') {
    isDark.value = true;
    document.body.dataset.theme = 'dark';
  }
  globalThis.addEventListener('resize', handleResize);
});

onUnmounted(() => {
  globalThis.removeEventListener('resize', handleResize);
});
</script>

<template>
  <ProAlert />
  <ProConfirm />
  <ProLoading v-if="loading" :loading="loading" />

  <div class="dashboard">
    <div v-if="sidebarOpen" class="sidebar-overlay" @click="handleCloseSidebar" />
    <AdminSidebar
      :open="sidebarOpen"
      :collapsed="sidebarCollapsed"
      @close-sidebar="handleCloseSidebar"
      @toggle-sidebar="handleToggleSidebar"
    />
    <main id="main" :class="['main', { expanded: sidebarCollapsed }]">
      <AdminHeader @toggle-sidebar="handleToggleSidebar" />
      <div class="content">
        <router-view />
      </div>
    </main>
  </div>
</template>
