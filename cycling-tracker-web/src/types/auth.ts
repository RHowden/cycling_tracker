export interface RegisterRequest {
  email: string;
  password: string;
}

// Adjust this once we know exactly what /api/auth/register returns
// (e.g. a token, or just the created athlete). Kept minimal for now.
export interface RegisterResponse {
  [key: string]: unknown;
}
