import { apiRequest, apiRequestVoid } from "./client";
import type {
  RegisterRequest,
  RegisterResponse,
  LoginRequest,
} from "../types/auth";

export function register(data: RegisterRequest): Promise<RegisterResponse> {
  return apiRequest<RegisterResponse>("/auth/register", { method: "POST", body: data });
}

export function login(email: string, password: string): Promise<void> {
  const data: LoginRequest = {
    email,
    password,
    twoFactorCode: null,
    twoFactorRecoveryCode: null,
  };
  return apiRequestVoid("/auth/login?useCookies=true", { method: "POST", body: data });
}

