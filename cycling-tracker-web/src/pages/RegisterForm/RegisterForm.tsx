import { useActionState } from "react";
import { register } from "../../api/auth";
import { ApiError } from "../../api/apiErrors";
import { useNavigate } from "react-router";
import "./RegisterForm.css";

export function RegisterForm() {
  const navigate = useNavigate();

  async function handleLoginAction(_: any, formData: FormData) {
    const email = formData.get("email") as string;
    const password = formData.get("password") as string;

    try {
      await register({ email, password });
      navigate("/login");

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
  const [state, formAction, isPending] = useActionState(handleLoginAction, {
    success: false,
    error: null,
  });


  return (
    <form className="register-card" action={formAction}>
      <h1 className="register-title">Create your account</h1>

      <label className="register-field">
        <span>Email</span>
        <input
          type="email"
          name="email"
          required
          autoComplete="email"
        />
      </label>

      <label className="register-field">
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
        <ul className="register-error">
          {state.error.map((msg) => (
            <li key={msg}>{msg}</li>
          ))}
        </ul>
      )}

      <button type="submit" disabled={isPending}>
        {isPending ? "Creating account…" : "Register"}
      </button>
    </form>
  );
}
