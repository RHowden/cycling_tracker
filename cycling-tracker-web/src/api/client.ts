// Base path for all API calls. Left as "/api" so the Vite dev-server proxy
// (see vite.config.ts) and the production same-origin deploy both just work.
const API_BASE = "/api";

export class ApiError extends Error {
  status: number;
  constructor(status: number, message: string) {
    super(message);
    this.status = status;
  }
}

export async function apiPost<TBody, TResponse>(
  path: string,
  body: TBody,
): Promise<TResponse> {
  const res = await fetch(`${API_BASE}${path}`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  });

  if (!res.ok) {
    // Try to surface whatever the API sent back (e.g. "email already in use")
    const text = await res.text();
    throw new ApiError(res.status, text || res.statusText);
  }

  // Register may return 204 No Content depending on how the endpoint is
  // implemented — guard against an empty body before parsing JSON.
  const text = await res.text();
  return (text ? JSON.parse(text) : {}) as TResponse;
}
