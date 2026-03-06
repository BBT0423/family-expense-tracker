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
  email: string;
  userName: string;
  firstName: string;
  lastName: string;
  roles: string[];
  isMfaEnabled: boolean;
  emailConfirmed: boolean;
  createdAt: string;
  lastLogin?: string;
}

export type { UpdateProfileRequest, EnableMfaResponse, UserProfile };
