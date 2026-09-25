import { apiRequest } from "./client";
import type {
  RegisterRequest,
  RegisterResponse,
  LoginRequest,
  LoginResponse,
} from "../types/auth";

export function register(data: RegisterRequest): Promise<RegisterResponse> {
  return apiRequest<RegisterResponse>("/auth/register", { method: "POST", body: data });
}

export function login(email: string, password: string): Promise<LoginResponse> {
  const data: LoginRequest = {
    email,
    password,
    twoFactorCode: null,
    twoFactorRecoveryCode: null,
  };
  return apiRequest<LoginResponse>("/auth/login", { method: "POST", body: data });
}

