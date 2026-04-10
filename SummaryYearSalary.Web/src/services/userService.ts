import type { ChangePasswordRequest, EnableMfaResponse, UserProfile, Result } from '@/types';
import { httpClient } from '@/utils/httpClient';

export const userService = {
  /**
   * GET /api/users
   * Get all registered users (family members)
   */
  async getAll() {
    return httpClient.get<Result<UserProfile[]>>('/api/users');
  },

  /**
   * POST /api/user/mfa/enable
   * Enable Multi-Factor Authentication for the current user
   */
  async enableMfa() {
    return httpClient.post<Result<EnableMfaResponse>>(`/api/user/mfa/enable`);
  },

  /**
   * POST /api/user/change-password
   * Change password for the current user
   */
  async changePassword(request: ChangePasswordRequest) {
    return httpClient.post<Result<void>>(`/api/user/change-password`, request);
  },
};
