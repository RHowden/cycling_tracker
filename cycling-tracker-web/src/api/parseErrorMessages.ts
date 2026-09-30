export async function parseErrorMessages(res: Response): Promise<string[]> {
  const text = await res.text();
  if (!text) return [genericMessage(res.status)];

  try {
    const body: ValidationProblemDetails = JSON.parse(text);

    if (body.errors) {
      const messages = Object.values(body.errors).flat();
      return messages.length ? messages : [genericMessage(res.status)];
    }

    if (body.error) {
      // If it's a Microsoft OAuth error object with a description
      if (typeof body.error === 'object' && body.error.message) {
        return [body.error.message];
      }
      // If body.error is just a raw string code
      if (typeof body.error === 'string') {
        return [body.error];
      }
    }
    return [body.title ?? genericMessage(res.status)];
  }
  catch {
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
  error?: string | { message?: string };
}

function genericMessage(status: number): string {
  return status >= 500
    ? "Something went wrong on our end. Try again in a moment."
    : "Something went wrong. Try again.";
}
