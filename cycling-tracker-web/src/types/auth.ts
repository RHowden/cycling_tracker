export interface RegisterRequest {
  email: string;
  password: string;
}

export interface LoginRequest {
  email: string;
  password: string;
  twoFactorCode: string | null;
  twoFactorRecoveryCode: string | null;
}
