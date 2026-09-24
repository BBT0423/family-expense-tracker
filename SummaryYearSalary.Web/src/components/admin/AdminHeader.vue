<template>
  <header class="header">
    <div class="header-left">
      <button
        type="button"
        class="menu-btn"
        aria-label="Open menu"
        @click="$emit('toggle-sidebar')"
      >
        <el-icon size="20"><Menu /></el-icon>
      </button>
      <nav class="header-crumb" aria-label="Breadcrumb">
        Family Expense
        <span v-if="pageTitle" class="header-crumb-current">/ {{ pageTitle }}</span>
      </nav>
    </div>

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

      <!-- Import job progress -->
      <el-dropdown v-if="importStatus !== 'idle'" trigger="click" placement="bottom-end">
        <el-button text circle>
          <el-progress
            v-if="importStatus === 'running'"
            type="circle"
            :percentage="importPercent"
            :width="22"
            :stroke-width="3"
            :show-text="false"
          />
          <el-icon v-else-if="importStatus === 'done'" size="18" color="var(--el-color-success)"
            ><CircleCheck
          /></el-icon>
          <el-icon v-else size="18" color="var(--el-color-danger)"><CircleClose /></el-icon>
        </el-button>
        <template #dropdown>
          <div class="notif-panel import-panel">
            <div class="notif-panel-header">
              <span>สถานะนำเข้าข้อมูล</span>
            </div>
            <div class="import-panel-body">
              <template v-if="importStatus === 'running'">
                <div class="import-status-text">
                  กำลังนำเข้า {{ importCompleted }}/{{ importTotal }} รายการ ({{ importPercent }}%)
                </div>
                <el-progress :percentage="importPercent" />
              </template>
              <template v-else-if="importStatus === 'done'">
                <div class="import-status-text">นำเข้าสำเร็จ {{ importCompleted }} รายการ</div>
                <el-button size="small" text @click="importJobStore.reset()">ปิด</el-button>
              </template>
              <template v-else-if="importStatus === 'error'">
                <div class="import-status-text error">
                  นำเข้าล้มเหลวหลังจากสำเร็จไปแล้ว {{ importCompleted }} รายการ
                </div>
                <div class="import-status-detail">{{ importErrorMessage }}</div>
                <el-button size="small" text @click="importJobStore.reset()">ปิด</el-button>
              </template>
            </div>
          </div>
        </template>
      </el-dropdown>

      <!-- Notification bell -->
      <el-dropdown trigger="click" placement="bottom-end" @visible-change="onNotifToggle">
        <el-badge :value="unreadCount" :hidden="unreadCount === 0" :max="9" class="notif-badge">
          <el-button text circle>
            <el-icon size="18"><Bell /></el-icon>
          </el-button>
        </el-badge>
        <template #dropdown>
          <div class="notif-panel">
            <div class="notif-panel-header">
              <span>การแจ้งเตือน</span>
              <el-button
                v-if="unreadCount > 0"
                text
                size="small"
                type="primary"
                @click.stop="handleMarkAllRead"
              >
                อ่านทั้งหมด
              </el-button>
            </div>
            <div class="notif-list">
              <div v-if="notificationStore.loading" class="notif-empty">กำลังโหลด...</div>
              <div v-else-if="notifications.length === 0" class="notif-empty">
                ไม่มีการแจ้งเตือน
              </div>
              <div
                v-for="n in notifications"
                :key="n.id"
                class="notif-item"
                :class="{ unread: !n.isRead }"
                @click="handleNotificationClick(n)"
              >
                <div class="notif-title">{{ n.title }}</div>
                <div class="notif-message">{{ n.message }}</div>
                <div class="notif-time">{{ formatDateTime(n.createdAt) }}</div>
              </div>
            </div>
          </div>
        </template>
      </el-dropdown>
    </div>
  </header>

  <NetworkStatusBanner />
</template>

<script setup lang="ts">
import { computed, inject, onMounted, onUnmounted } from 'vue';
import { storeToRefs } from 'pinia';
import { useRoute, useRouter } from 'vue-router';
import { CircleCheck, CircleClose } from '@element-plus/icons-vue';
import type { NotificationItem } from '@/types';
import { useNotificationStore } from '@/stores/notificationStore';
import { useImportJobStore } from '@/stores/importJobStore';
import { formatDateTime } from '@/utils/formatters';

defineEmits(['toggle-sidebar']);

const route = useRoute();
const router = useRouter();

const notificationStore = useNotificationStore();
const { notifications, unreadCount } = storeToRefs(notificationStore);

const importJobStore = useImportJobStore();
const {
  status: importStatus,
  percent: importPercent,
  completed: importCompleted,
  total: importTotal,
  errorMessage: importErrorMessage,
} = storeToRefs(importJobStore);

let pollHandle: ReturnType<typeof setInterval> | undefined;

onMounted(() => {
  notificationStore.fetchUnreadCount();
  pollHandle = setInterval(() => notificationStore.fetchUnreadCount(), 45000);
});

onUnmounted(() => {
  if (pollHandle) clearInterval(pollHandle);
});

function onNotifToggle(visible: boolean) {
  if (visible) notificationStore.fetchNotifications();
}

function handleNotificationClick(n: NotificationItem) {
  notificationStore.markRead(n.id);
  if (n.type === 'TransactionProposed') router.push('/pages/proposals');
}

function handleMarkAllRead() {
  notificationStore.markAllRead();
}

const isDark = inject<{ value: boolean }>('isDark');
const toggleTheme = inject<() => void>('toggleTheme', () => {});

const pageTitle = computed(() => route.meta?.title as string | undefined);
</script>

<style scoped>
.header-crumb {
  font-size: 13px;
  color: var(--text-muted);
  white-space: nowrap;
}

.header-crumb-current {
  color: var(--text);
  font-weight: 600;
}

.header-right {
  gap: 4px;
}

/* Notification bell */
.notif-badge {
  display: inline-flex;
}

.notif-panel {
  width: 320px;
  max-height: 420px;
  display: flex;
  flex-direction: column;
}

.import-panel-body {
  padding: 12px 16px;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.import-status-text {
  font-size: 0.85rem;
  color: var(--text);
}

.import-status-text.error {
  color: var(--el-color-danger);
}

.import-status-detail {
  font-size: 0.78rem;
  color: var(--text-muted);
}

.notif-panel-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 10px 16px;
  font-weight: 600;
  font-size: 0.9rem;
  color: var(--text);
  border-bottom: 1px solid var(--border);
}

.notif-empty {
  padding: 24px 16px;
  text-align: center;
  color: var(--text-muted);
  font-size: 0.85rem;
}

.notif-list {
  overflow-y: auto;
}

.notif-item {
  padding: 10px 16px;
  border-bottom: 1px solid var(--border);
  cursor: pointer;
}

.notif-item:hover {
  background: var(--surface-dark);
}

.notif-item.unread {
  background: var(--active-nav-bg);
}

.notif-title {
  font-size: 0.85rem;
  font-weight: 600;
  color: var(--text);
}

.notif-message {
  font-size: 0.8rem;
  color: var(--text-light);
  margin-top: 2px;
}

.notif-time {
  font-size: 0.72rem;
  color: var(--text-muted);
  margin-top: 4px;
}
</style>
