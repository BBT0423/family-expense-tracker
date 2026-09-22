import { httpClient } from '@/utils/httpClient';
import type { NotificationItem, Result } from '@/types';

export const notificationService = {
  async getAll(unreadOnly = false) {
    return httpClient.get<Result<NotificationItem[]>>('/api/notifications', {
      params: { unreadOnly },
    });
  },

  async getUnreadCount() {
    return httpClient.get<Result<number>>('/api/notifications/unread-count');
  },

  async markRead(id: number) {
    return httpClient.post<Result<boolean>>(`/api/notifications/${id}/read`);
  },

  async markAllRead() {
    return httpClient.post<Result<boolean>>('/api/notifications/read-all');
  },
};
