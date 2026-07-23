// Base path for all API calls. Left as "/api" so the Vite dev-server proxy
// (see vite.config.ts) and the production same-origin deploy both just work.
const API_BASE = "/api";

// ASP.NET Core's default validation failure shape (ValidationProblemDetails):
// { "title": "...", "status": 400, "errors": { "FieldOrCode": ["message", ...] } }
interface ValidationProblemDetails {
  title?: string;
  errors?: Record<string, string[]>;
}

async function parseErrorMessages(res: Response): Promise<string[]> {
  const text = await res.text();
  if (!text) return [res.statusText];

  try {
    const body: ValidationProblemDetails = JSON.parse(text);
    if (body.errors) {
      // Flatten { field: [msg, msg], field2: [msg] } into a list of messages.
      // Identity's messages are already human-readable on their own
      // (e.g. "Passwords must have at least one non alphanumeric character.")
      // — the error codes as keys are the noisy/technical part, so we drop them.
      return Object.values(body.errors).flat();
    }
    return [body.title ?? text];
  } catch {
    // Not JSON — just show whatever came back
    return [text];
  }
}

export class ApiError extends Error {
  status: number;
  messages: string[];
  constructor(status: number, messages: string[]) {
    super(messages.join(" "));
    this.status = status;
    this.messages = messages;
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
    throw new ApiError(res.status, await parseErrorMessages(res));
  }

  // Register may return 204 No Content depending on how the endpoint is
  // implemented — guard against an empty body before parsing JSON.
  const text = await res.text();
  return (text ? JSON.parse(text) : {}) as TResponse;
}