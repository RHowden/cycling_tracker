import { apiRequestVoid } from "./client";
import type {
  RegisterRequest,
  LoginRequest,
} from "../types/auth";

export function register(data: RegisterRequest): Promise<void> {
  return apiRequestVoid("/auth/register", { method: "POST", body: data });
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

