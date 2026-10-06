interface UpdateProfileRequest {
  firstName: string;
  lastName: string;
}

interface EnableMfaResponse {
  sharedKey: string;
  authenticatorUri: string;
}

interface UserProfile {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber?: string;
  createdAt: string;
  lastLoginAt?: string;
  roles: string[];
}

interface CreateUserRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  role: 'Admin' | 'User';
}

interface UpdateUserRequest {
  firstName: string;
  lastName: string;
}

export type {
  UpdateProfileRequest,
  EnableMfaResponse,
  UserProfile,
  CreateUserRequest,
  UpdateUserRequest,
};
