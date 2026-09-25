// Base path for all API calls. Left as "/api" so the Vite dev-server proxy

import { ApiError } from "./ApiError";
import { parseErrorMessages } from "./parseErrorMessages";

// (see vite.config.ts) and the production same-origin deploy both just work.
const API_BASE = `${import.meta.env.VITE_API_BASE_URL}/api`;

export async function apiRequest<TResponse>(
  path: string,
  options: { method?: string; body?: unknown } = {},
): Promise<TResponse> {
  const { method = "GET", body } = options;
  let res: Response;
  try {
    res = await fetch(`${API_BASE}${path}`, {
      method,
      headers: body ? { "Content-Type": "application/json" } : undefined,
      body: body ? JSON.stringify(body) : undefined,
    });
  } catch {
    throw new ApiError(0, ["Network error — please check your connection."]);
  }

  if (!res.ok) {
    throw new ApiError(res.status, await parseErrorMessages(res));
  }

  if (res.status === 204) {
    return undefined as TResponse;
  }

  const text = await res.text();

  if (!text) {
    throw new ApiError(res.status, ["Received an empty response from the server."]);
  }

  try {
    return JSON.parse(text) as TResponse;
  } catch {
    throw new ApiError(res.status, ["Received an invalid response from the server."]);
  }
}