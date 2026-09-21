import { apiPost } from "./client";
import type {
  RegisterRequest,
  RegisterResponse,
  LoginRequest,
  LoginResponse,
} from "../types/auth";

export function register(data: RegisterRequest): Promise<RegisterResponse> {
  return apiPost<RegisterRequest, RegisterResponse>("/auth/register", data);
}

export function login(email: string, password: string): Promise<LoginResponse> {
  const body: LoginRequest = {
    email,
    password,
    twoFactorCode: null,
    twoFactorRecoveryCode: null,
  };
  return apiPost<LoginRequest, LoginResponse>("/auth/login", body);
}
