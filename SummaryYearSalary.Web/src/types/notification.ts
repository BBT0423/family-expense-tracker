export interface NotificationItem {
  id: number;
  type: string;
  title: string;
  message: string;
  relatedEntityId?: number;
  isRead: boolean;
  createdAt: string;
}
