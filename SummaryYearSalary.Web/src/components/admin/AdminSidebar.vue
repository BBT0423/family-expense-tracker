<template>
  <aside :class="['sidebar', { open: props.open, collapsed: props.collapsed }]">
    <div class="sidebar-inner">
      <div class="sidebar-content-wrapper">
        <div class="sidebar-content">
          <div class="sidebar-header">
            <div
              style="
                display: flex;
                align-items: center;
                justify-content: space-between;
                width: 100%;
              "
            >
              <SidebarLogo logo="Place logo here" :is-dark="false" />
              <button
                class="toggle-sidebar"
                aria-label="Close sidebar"
                type="button"
                @click="emit('toggle-sidebar')"
              >
                <svg
                  width="24"
                  height="24"
                  viewBox="0 0 24 24"
                  fill="none"
                  stroke="currentColor"
                  stroke-width="2"
                  stroke-linecap="round"
                  stroke-linejoin="round"
                >
                  <polyline points="15 18 9 12 15 6" />
                </svg>
              </button>
            </div>
          </div>
          <nav class="nav-menu">
            <SidebarSection
              v-for="section in menuSections"
              :key="section.title"
              :title="section.title"
            >
              <SidebarNavItem
                v-for="item in section.items"
                :key="item.label"
                :href="item.href"
                :active="activeNav.valueOf() === item.label"
                @click="() => handleNavItemClick(item.label)"
              >
                <template #icon>{{ item.icon }}</template>
                {{ item.label }}
              </SidebarNavItem>
            </SidebarSection>
          </nav>
        </div>
        <div class="sidebar-version">v{{ appVersion }}</div>
      </div>
    </div>
  </aside>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { APP_VERSION } from '@/constants/version';

const appVersion = APP_VERSION;

const props = defineProps<{ open: boolean; collapsed?: boolean }>();
const emit = defineEmits(['toggle-sidebar', 'close-sidebar']);

// Ensure activeNav is a ref and not shadowed
const activeNav = ref<string>('Dashboard');

function handleNavItemClick(label: string) {
  activeNav.value = label;
  emit('close-sidebar');
}

onMounted(() => {
  const path = window.location.pathname.toLowerCase();
  const navMap: Record<string, string> = {
    users: 'Users',
  };

  const found = Object.entries(navMap).find(([key]) => path.includes(key));
  activeNav.value = found ? found[1] : 'Dashboard';
});

interface MenuItem {
  icon: string;
  label: string;
  href: string;
}

const menuSections = computed(() => {
  const sections = [
    {
      title: 'User Management',
      items: [{ icon: '👤', label: 'User Profile', href: '/pages/users/profile' }].filter(
        (item): item is MenuItem => item !== null
      ),
    },
  ];

  // Filter out sections that have no items
  return sections.filter((section) => section.items.length > 0);
});
</script>

<style scoped>
.sidebar-inner {
  display: flex;
  flex-direction: column;
  height: 100%;
}

.sidebar-content-wrapper {
  display: flex;
  flex-direction: column;
  min-height: 100%;
  flex: 1;
}

.sidebar-content {
  flex: 1;
}

.sidebar-version {
  width: 100%;
  padding: 0.48rem 0;
  text-align: center;
  font-size: 0.72rem;
  color: var(--layout-text);
  background: var(--layout-bg);
  border-top: 1px solid var(--layout-border);
  letter-spacing: 0.03em;
  flex-shrink: 0;
  margin-top: auto;
}

.toggle-sidebar {
  background: none;
  border: none;
  padding: 0.25rem;
  margin-left: 0.5rem;
  cursor: pointer;
  color: #4a5568;
  display: none;
}

@media (max-width: 1024px) {
  .toggle-sidebar {
    display: inline-flex;
    align-items: center;
    justify-content: center;
  }
}

.toggle-sidebar svg {
  width: 1.5rem;
  height: 1.5rem;
}
</style>
