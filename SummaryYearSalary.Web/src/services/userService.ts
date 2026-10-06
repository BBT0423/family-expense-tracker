import type {
  ChangePasswordRequest,
  CreateUserRequest,
  EnableMfaResponse,
  UpdateUserRequest,
  UserProfile,
  Result,
} from '@/types';
import { httpClient } from '@/utils/httpClient';

export const userService = {
  /**
   * GET /api/users (Admin only)
   * Get all registered users in the system
   */
  async getAll() {
    return httpClient.get<Result<UserProfile[]>>('/api/users');
  },

  /**
   * POST /api/users (Admin only)
   * Create a new user account
   */
  async create(payload: CreateUserRequest) {
    return httpClient.post<Result<UserProfile>>('/api/users', payload);
  },

  /**
   * PUT /api/users/{id} (Admin only)
   * Update another user's name
   */
  async update(id: string, payload: UpdateUserRequest) {
    return httpClient.put<Result<boolean>>(`/api/users/${id}`, payload);
  },

  /**
   * PUT /api/users/{id}/role (Admin only)
   * Promote/demote another user between "User" and "Admin"
   */
  async updateRole(id: string, role: 'Admin' | 'User') {
    return httpClient.put<Result<boolean>>(`/api/users/${id}/role`, { role });
  },

  /**
   * POST /api/user/mfa/enable
   * Enable Multi-Factor Authentication for the current user
   */
  async enableMfa() {
    return httpClient.post<Result<EnableMfaResponse>>(`/api/user/mfa/enable`);
  },

  /**
   * POST /api/user/mfa/confirm
   * Confirm MFA setup with a code from the authenticator app; MFA is only active after this succeeds
   */
  async confirmMfa(code: string) {
    return httpClient.post<Result<boolean>>(`/api/user/mfa/confirm`, { code });
  },

  /**
   * POST /api/user/change-password
   * Change password for the current user
   */
  async changePassword(request: ChangePasswordRequest) {
    return httpClient.post<Result<void>>(`/api/user/change-password`, request);
  },
};
