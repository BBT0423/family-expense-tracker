<script setup lang="ts">
import { ref, onMounted, onUnmounted, provide } from 'vue';
import { SecureStorage } from '@/utils/storage';
import { useGlobalUi } from '@/composables/useGlobalUi';
import { STORAGE_THEME } from '@/constants/storage';
import AdminSidebar from '@/components/admin/AdminSidebar.vue';
import AdminHeader from '@/components/admin/AdminHeader.vue';

const sidebarOpen = ref(false);
const sidebarCollapsed = ref(false); // For desktop toggle
const sidebarRoot = ref<HTMLElement | null>(null);
const headerRoot = ref<HTMLElement | null>(null);
const { loading } = useGlobalUi();

// THEME STATE (shared)
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

onMounted(() => {
  const savedTheme = SecureStorage.getItem(STORAGE_THEME);
  if (savedTheme === 'dark') {
    isDark.value = true;
    document.body.dataset.theme = 'dark';
  }
  document.addEventListener('click', handleClick);
  globalThis.addEventListener('resize', handleResize);

  // Patch: assign DOM elements to sidebarRoot/headerRoot after mount
  sidebarRoot.value = document.querySelector('.sidebar');
  headerRoot.value = document.querySelector('.header');
});

onUnmounted(() => {
  document.removeEventListener('click', handleClick);
  globalThis.removeEventListener('resize', handleResize);
});

const handleToggleSidebar = () => {
  // Check if we're on mobile/tablet (< 1024px)
  if (globalThis.innerWidth < 1024) {
    sidebarOpen.value = !sidebarOpen.value;
  } else {
    // Desktop behavior: toggle collapsed state
    sidebarCollapsed.value = !sidebarCollapsed.value;
  }
};

const handleCloseSidebar = () => {
  sidebarOpen.value = false;
};

function handleClick(e: MouseEvent) {
  // Only handle clicks on mobile/tablet
  if (window.innerWidth < 1024) {
    if (
      sidebarRoot.value &&
      !sidebarRoot.value.contains(e.target as Node) &&
      headerRoot.value &&
      !headerRoot.value.contains(e.target as Node)
    ) {
      handleCloseSidebar();
    }
  }
}

function handleResize() {
  // Reset mobile sidebar state on resize
  if (window.innerWidth < 1024) {
    sidebarOpen.value = false;
  }
  // Reset desktop collapsed state if going to mobile
  if (window.innerWidth < 1024) {
    sidebarCollapsed.value = false;
  }
}
</script>

<template>
  <ProAlert />
  <ProConfirm />
  <ProLoading v-if="loading" :loading="loading" />
  <div class="dashboard">
    <AdminSidebar
      ref="sidebarRootEl"
      :open="sidebarOpen"
      :collapsed="sidebarCollapsed"
      @close-sidebar="handleCloseSidebar"
      @toggle-sidebar="handleToggleSidebar"
    />
    <main id="main" :class="['main', { expanded: sidebarCollapsed }]">
      <AdminHeader ref="headerRootEl" @toggle-sidebar="handleToggleSidebar" />
      <div class="content">
        <router-view />
      </div>
    </main>
  </div>
</template>
