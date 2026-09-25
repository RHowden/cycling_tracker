import { apiPost } from "./client";
import type {
  RegisterRequest,
  RegisterResponse,
  LoginRequest,
  LoginResponse,
} from "../types/auth";

export function register(data: RegisterRequest): Promise<RegisterResponse> {
  return apiPost<RegisterResponse>("/auth/register", { method: "POST", body: data });
}

export function login(email: string, password: string): Promise<LoginResponse> {
  const data: LoginRequest = {
    email,
    password,
    twoFactorCode: null,
    twoFactorRecoveryCode: null,
  };
  return apiPost<LoginResponse>("/auth/login", { method: "POST", body: data });
}

