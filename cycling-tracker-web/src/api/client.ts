import { z } from "zod";

import { ApiError, ResponseShapeError } from "./apiErrors";
import { parseErrorMessages } from "./parseErrorMessages";

const API_BASE = `${import.meta.env.VITE_API_BASE_URL}/api`;

export async function apiRequestVoid(
  path: string,
  options: { method?: string; body?: unknown } = {},
): Promise<void> {
  await performRequest(path, options);
}

export async function apiRequest<S extends z.ZodType>(
  path: string,
  schema: S,
  options: { method?: string; body?: unknown } = {},
): Promise<z.output<S>> {
  const res = await performRequest(path, options);

  let json: unknown = undefined;

  if (res.status !== 204) {
    const text = await res.text();

    if (!text) {
      throw new ApiError(res.status, ["Received an empty response from the server."]);
    }

    try {
      json = JSON.parse(text);
    } catch {
      throw new ApiError(res.status, ["Received an invalid response from the server."]);
    }
  }

  const result = schema.safeParse(json);
  if (!result.success) {
    throw new ResponseShapeError(path, result.error);
  }
  return result.data;
}

async function performRequest(
  path: string,
  options: { method?: string; body?: unknown } = {},
): Promise<Response> {
  const { method = "GET", body } = options;
  const isForm = body instanceof FormData;

  let res: Response;
  try {
    res = await fetch(`${API_BASE}${path}`, {
      method,
      credentials: "include",
      headers: body && !isForm ? { "Content-Type": "application/json" } : undefined,
      body: !body ? undefined : isForm ? body : JSON.stringify(body),
    });
  } catch {
    throw new ApiError(0, ["Network error — please check your connection."]);
  }

  if (!res.ok) {
    throw new ApiError(res.status, await parseErrorMessages(res));
  }

  return res;
}
