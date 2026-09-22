/**
 * Authentication related type definitions
 */

export type LoginProvider = 'email' | 'google' | 'phone';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  refreshToken: string;
  expiresIn: number;
  isMfaEnabled: boolean;
  user: {
    id: string;
    email: string;
    name: string;
    avatar: string;
    metadata: Record<string, any> & { roles?: string[] };
    loginProvider?: LoginProvider;
  };
}

export interface VerifyMfaRequest {
  mfaCode: string;
  userId: string;
}

export interface VerifyMfaResponse {
  token: string;
  refreshToken: string;
  expiresIn: number;
  isMfaEnabled: boolean;
  user: {
    id: string;
    email: string;
    name: string;
    avatar: string;
    metadata: Record<string, any> & { roles?: string[] };
    loginProvider?: LoginProvider;
  };
}

export interface ChangePasswordRequest {
  userId: string;
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
  userId: string;
}

export interface SendPhoneOtpRequest {
  phoneNumber: string;
}

export interface VerifyPhoneOtpRequest {
  phoneNumber: string;
  otpCode: string;
}
