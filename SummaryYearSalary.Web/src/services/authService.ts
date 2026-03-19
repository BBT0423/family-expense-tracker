import { httpClient } from '@/utils/httpClient';
import type {
  LoginRequest,
  LoginResponse,
  RefreshTokenRequest,
  SendPhoneOtpRequest,
  VerifyMfaRequest,
  VerifyMfaResponse,
  VerifyPhoneOtpRequest,
} from '@/types/authorize';
import type { Result } from '@/types';

export const authService = {
  /**
   * POST /api/authorize/login
   * Authenticate user and return JWT tokens
   */
  async login(payload: LoginRequest) {
    return httpClient.post<Result<LoginResponse>>('/api/authorize/login', payload);
  },

  /**
   * POST /api/authorize/refresh
   * Refresh access token using refresh token
   */
  async refreshToken(payload: RefreshTokenRequest) {
    return httpClient.post<Result<LoginResponse>>('/api/authorize/refresh', payload);
  },

  /**
   * POST /api/authorize/mfa/verify
   * Verify MFA code and complete authentication
   */
  async verifyMfa(payload: VerifyMfaRequest) {
    return httpClient.post<Result<VerifyMfaResponse>>('/api/authorize/mfa/verify', payload);
  },

  /**
   * POST /api/authorize/forgot-password
   * Initiate password reset process
   */
  async forgotPassword(payload: { email: string }) {
    return httpClient.post<Result<void>>('/api/authorize/forgot-password', payload);
  },

  /**
   * POST /api/authorize/reset-password
   * Complete password reset process
   */
  async resetPassword(payload: { email: string; token: string; newPassword: string }) {
    return httpClient.post<Result<void>>('/api/authorize/reset-password', payload);
  },

  /**
   * POST /api/authorize/phone/send-otp
   * Send OTP to email linked with the phone number
   */
  async sendPhoneOtp(payload: SendPhoneOtpRequest) {
    return httpClient.post<Result<void>>('/api/authorize/phone/send-otp', payload);
  },

  /**
   * POST /api/authorize/phone/verify-otp
   * Verify OTP and complete phone login
   */
  async verifyPhoneOtp(payload: VerifyPhoneOtpRequest) {
    return httpClient.post<Result<LoginResponse>>('/api/authorize/phone/verify-otp', payload);
  },
};
