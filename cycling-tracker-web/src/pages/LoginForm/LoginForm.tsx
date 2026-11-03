import { useActionState } from "react";
import { useLocation, useNavigate } from "react-router";

import { ApiError } from "../../api/apiErrors";
import { useAuth } from "../../components/AuthContext";


export function LoginForm() {
  const navigate = useNavigate();
  const { login } = useAuth();

  async function handleLoginAction(_: any, formData: FormData) {
    const email = formData.get("email") as string;
    const password = formData.get("password") as string;

    try {
      await login(email, password);
      navigate('/dashboard');

      return { success: true, error: null };
    } catch (err: any) {
      if (!(err instanceof ApiError)) console.error("Unexpected login error", err);
      return {
        success: false,
        error: err instanceof ApiError ? err.messages : ["Failed to log in"],
      };
    }
  }

  // 2. Pass the action to useActionState
  // It gives you: [the current state returned by the action, the trigger function, a built-in loading boolean]
  const [state, formAction, isPending] = useActionState(handleLoginAction, {
    success: false,
    error: null,
  });

  const location = useLocation();
  const justRegistered = location.state?.registered === true;
  return (
    <form className="card card--form-layout card--narrow" action={formAction}>
      <h1 className="card__title">Log in</h1>

      {justRegistered && <p className="form-success" role="status">Account created. Please log in.</p>}

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
          autoComplete="current-password"
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
        {isPending ? "Logging in…" : "Log in"}
      </button>
    </form>
  );
}
