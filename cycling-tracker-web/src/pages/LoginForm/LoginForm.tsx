import { useActionState } from "react";
import { useLocation, useNavigate } from "react-router";

import { ApiError } from "../../api/apiErrors";
import { useAuth } from "../../components/AuthContext";

import "./LoginForm.css";

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
  debugger
  return (
    <form className="login-card" action={formAction}>
      <h1 className="login-title">Log in</h1>

      {justRegistered && <p className="login-title" role="status">Account created. Please log in.</p>}

      <label className="login-field">
        <span>Email</span>
        <input
          type="email"
          name="email"
          required
          autoComplete="email"
        />
      </label>

      <label className="login-field">
        <span>Password</span>
        <input
          type="password"
          name="password"
          required
          autoComplete="current-password"
        />
      </label>

      {state.error && (
        <ul className="login-error">

          {state.error.map((msg) => (
            <li key={msg}>{msg}</li>
          ))}
        </ul>
      )}

      <button type="submit" disabled={isPending}>
        {isPending ? "Logging in…" : "Log in"}
      </button>
    </form>
  );
}
