<template>
  <aside :class="['sidebar', { open: props.open, collapsed: props.collapsed }]">
    <button
      class="sidebar-collapse-toggle"
      type="button"
      :aria-label="props.collapsed ? 'Expand sidebar' : 'Collapse sidebar'"
      @click="emit('toggle-sidebar')"
    >
      <svg width="14" height="14" viewBox="0 0 24 24" fill="none">
        <path
          d="M15 6l-6 6 6 6"
          stroke="currentColor"
          stroke-width="2"
          stroke-linecap="round"
          stroke-linejoin="round"
        />
      </svg>
    </button>

    <div class="sidebar-header">
      <SidebarLogo />
      <button
        class="sidebar-close"
        type="button"
        aria-label="Close sidebar"
        @click="emit('close-sidebar')"
      >
        <el-icon size="20"><Close /></el-icon>
      </button>
    </div>

    <div class="sidebar-content">
      <nav class="nav-menu" data-tour="sidebar-nav">
        <SidebarSection v-for="section in menuSections" :key="section.title" :title="section.title">
          <div
            v-for="item in section.items"
            :key="item.path"
            @mouseenter="onNavHover(item, $event)"
            @mouseleave="clearNavHover"
          >
            <SidebarNavItem
              :href="item.path"
              :active="isActive(item.path)"
              @click="emit('close-sidebar')"
            >
              <template #icon>
                <el-icon size="18"><component :is="item.icon" /></el-icon>
              </template>
              {{ item.label }}
            </SidebarNavItem>
          </div>
        </SidebarSection>
      </nav>
    </div>

    <div class="sidebar-version">v{{ appVersion }}</div>

    <Teleport to="body">
      <div
        v-if="props.collapsed && hoverItem && hoverRect"
        class="nav-tooltip"
        :style="tooltipStyle"
      >
        {{ hoverItem.label }}
      </div>
    </Teleport>
  </aside>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue';
import { useRoute } from 'vue-router';
import { APP_VERSION } from '@/constants/version';
import SidebarLogo from '@/components/admin/SidebarLogo.vue';
import SidebarSection from '@/components/admin/SidebarSection.vue';
import SidebarNavItem from '@/components/admin/SidebarNavItem.vue';

const props = defineProps<{ open: boolean; collapsed?: boolean }>();
const emit = defineEmits(['toggle-sidebar', 'close-sidebar']);

const route = useRoute();
const appVersion = APP_VERSION;

interface MenuItem {
  path: string;
  label: string;
  icon: string;
}

const menuSections: { title: string; items: MenuItem[] }[] = [
  {
    title: 'OVERVIEW',
    items: [{ path: '/pages/dashboard', label: 'แดชบอร์ด', icon: 'DataLine' }],
  },
  {
    title: 'รายการ',
    items: [
      { path: '/pages/transactions', label: 'รายการธุรกรรม', icon: 'List' },
      { path: '/pages/transactions/add', label: 'เพิ่มรายการ', icon: 'Plus' },
      { path: '/pages/import', label: 'นำเข้าข้อมูล', icon: 'Upload' },
    ],
  },
  {
    title: 'จัดการ',
    items: [
      { path: '/pages/settings', label: 'ตั้งค่า', icon: 'Setting' },
      { path: '/pages/help', label: 'คู่มือการใช้งาน', icon: 'Reading' },
    ],
  },
];

// Longest matching path wins so /transactions/add doesn't also highlight /transactions.
const allPaths = menuSections.flatMap((s) => s.items.map((i) => i.path));
const activePath = computed(() => {
  const matches = allPaths.filter((p) => route.path === p || route.path.startsWith(p + '/'));
  return matches.sort((a, b) => b.length - a.length)[0];
});
const isActive = (path: string) => activePath.value === path;

// Collapsed-mode tooltip
const hoverItem = ref<MenuItem | null>(null);
const hoverRect = ref<DOMRect | null>(null);
let clearHoverTimeout: ReturnType<typeof setTimeout> | null = null;

function onNavHover(item: MenuItem, event: MouseEvent) {
  if (clearHoverTimeout) clearTimeout(clearHoverTimeout);
  hoverItem.value = item;
  hoverRect.value = (event.currentTarget as HTMLElement).getBoundingClientRect();
}

function clearNavHover() {
  clearHoverTimeout = setTimeout(() => {
    hoverItem.value = null;
    hoverRect.value = null;
  }, 80);
}

const tooltipStyle = computed(() => {
  if (!hoverRect.value) return {};
  return {
    top: `${hoverRect.value.top + hoverRect.value.height / 2}px`,
    left: `${hoverRect.value.right + 12}px`,
  };
});
</script>
