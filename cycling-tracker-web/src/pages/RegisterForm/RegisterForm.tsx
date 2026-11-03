import { useActionState } from "react";
import { register } from "../../api/auth";
import { ApiError } from "../../api/apiErrors";
import { useNavigate } from "react-router";

export function RegisterForm() {
  const navigate = useNavigate();

  async function handleRegisterAction(_: any, formData: FormData) {
    const email = formData.get("email") as string;
    const password = formData.get("password") as string;

    try {
      await register({ email, password });
      navigate("/login", { state: { registered: true } });

      return { success: true, error: null };
    } catch (err: any) {
      if (!(err instanceof ApiError)) console.error("Unexpected login error", err);
      return {
        success: false,
        error: err instanceof ApiError ? err.messages : ["Failed to register"],
      };
    }
  }

  // 2. Pass the action to useActionState
  // It gives you: [the current state returned by the action, the trigger function, a built-in loading boolean]
  const [state, formAction, isPending] = useActionState(handleRegisterAction, {
    success: false,
    error: null,
  });


  return (
    <form className="card card--form-layout card--narrow" action={formAction}>
      <h1 className="card__title">Create your account</h1>

      <label className="form-field">
        <span>Email</span>
        <input
          type="email"
          name="email"
          required
          autoComplete="email"
        />
      </label>

      <label className="form-field">
        <span>Password</span>
        <input
          type="password"
          name="password"
          required
          autoComplete="new-password"
          minLength={8}
        />
      </label>

      {state.error && (
        <ul className="form-error">
          {state.error.map((msg) => (
            <li key={msg}>{msg}</li>
          ))}
        </ul>
      )}

      <button type="submit" className="btn btn-primary btn-sharp" disabled={isPending}>
        {isPending ? "Creating account…" : "Register"}
      </button>
    </form>
  );
}
