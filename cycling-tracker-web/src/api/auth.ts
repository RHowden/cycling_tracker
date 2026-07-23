import { apiPost } from "./client";
import type { RegisterRequest, RegisterResponse } from "../types/auth";

export function register(data: RegisterRequest): Promise<RegisterResponse> {
  return apiPost<RegisterRequest, RegisterResponse>("/auth/register", data);
}
