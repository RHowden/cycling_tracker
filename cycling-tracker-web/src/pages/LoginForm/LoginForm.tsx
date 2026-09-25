import { useActionState } from "react";
import { useNavigate } from "react-router";

import { login } from "../../api/auth";
import { ApiError } from "../../api/ApiError";

import "./LoginForm.css";

export function LoginForm() {
  const navigate = useNavigate();

  async function handleLoginAction(_: any, formData: FormData) {
    const email = formData.get("email") as string;
    const password = formData.get("password") as string;

    try {
      const result = await login(email, password);
      localStorage.setItem("authToken", result.accessToken);
      navigate('/dashboard');

      return { success: true, error: null };
    } catch (err: any) {
      const apiError = err as ApiError;
    
      return { success: false, error: apiError.messages || "Failed to log in" };
    }
  }

  // 2. Pass the action to useActionState
  // It gives you: [the current state returned by the action, the trigger function, a built-in loading boolean]
  const [state, formAction, isPending] = useActionState(handleLoginAction, {
    success: false,
    error: null,
  });

  return (
    <form className="login-card" action={formAction}>
      <h1 className="login-title">Log in</h1>

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
