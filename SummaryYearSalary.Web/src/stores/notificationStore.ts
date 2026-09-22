import { defineStore } from 'pinia';
import { ref } from 'vue';
import { notificationService } from '@/services/notificationService';
import type { NotificationItem } from '@/types';

export const useNotificationStore = defineStore('notifications', () => {
  const notifications = ref<NotificationItem[]>([]);
  const unreadCount = ref(0);
  const loading = ref(false);

  async function fetchNotifications() {
    loading.value = true;
    try {
      const { data } = await notificationService.getAll();
      if (data?.isSuccess && data.value) {
        notifications.value = data.value;
      }
    } finally {
      loading.value = false;
    }
  }

  async function fetchUnreadCount() {
    const { data } = await notificationService.getUnreadCount();
    if (data?.isSuccess && typeof data.value === 'number') {
      unreadCount.value = data.value;
    }
  }

  async function markRead(id: number) {
    const { data } = await notificationService.markRead(id);
    if (data?.isSuccess) {
      const item = notifications.value.find((n) => n.id === id);
      if (item && !item.isRead) {
        item.isRead = true;
        unreadCount.value = Math.max(0, unreadCount.value - 1);
      }
    }
    return data;
  }

  async function markAllRead() {
    const { data } = await notificationService.markAllRead();
    if (data?.isSuccess) {
      notifications.value.forEach((n) => (n.isRead = true));
      unreadCount.value = 0;
    }
    return data;
  }

  return {
    notifications,
    unreadCount,
    loading,
    fetchNotifications,
    fetchUnreadCount,
    markRead,
    markAllRead,
  };
});
