export async function parseErrorMessages(res: Response): Promise<string[]> {
  const text = await res.text();
  if (!text) return [genericMessage(res.status)];

  try {
    const body: ValidationProblemDetails = JSON.parse(text);
    if (body.errors) {
      // Flatten { field: [msg, msg], field2: [msg] } into a list of messages.
      // Identity's messages are already human-readable on their own
      // (e.g. "Passwords must have at least one non alphanumeric character.")
      // — the error codes as keys are the noisy/technical part, so we drop them.
      const messages = Object.values(body.errors).flat();
      return messages.length ? messages : [genericMessage(res.status)];
    }
    // Structured JSON but no `errors` field (e.g. a plain ProblemDetails) —
    // title is written to be user-facing, safe to show.
    return [body.title ?? genericMessage(res.status)];
  } catch {
    // Not JSON at all — most likely a 5xx dev-mode exception page or raw
    // stack trace. Never show that to the user; log it for us to debug.
    console.error("Unexpected error response from API:", text);
    return [genericMessage(res.status)];
  }
}

// ASP.NET Core's default validation failure shape (ValidationProblemDetails):
// { "title": "...", "status": 400, "errors": { "FieldOrCode": ["message", ...] } }
interface ValidationProblemDetails {
  title?: string;
  errors?: Record<string, string[]>;
}

function genericMessage(status: number): string {
  return status >= 500
    ? "Something went wrong on our end. Try again in a moment."
    : "Something went wrong. Try again.";
}
