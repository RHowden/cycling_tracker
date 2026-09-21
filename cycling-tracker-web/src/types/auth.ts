export interface RegisterRequest {
  email: string;
  password: string;
}

// Adjust this once we know exactly what /api/auth/register returns
// (e.g. a token, or just the created athlete). Kept minimal for now.
export interface RegisterResponse {
  [key: string]: unknown;
}

export interface LoginRequest {
  email: string;
  password: string;
  twoFactorCode: string | null;
  twoFactorRecoveryCode: string | null;
}

// Shape from ASP.NET Core Identity's built-in MapIdentityApi() login endpoint.
// If AuthController wraps/renames this, update here only.
export interface LoginResponse {
  tokenType: string;
  accessToken: string;
  expiresIn: number;
  refreshToken: string;
}
