import { useState, FormEvent } from "react";
import { login } from "../../api/auth";
import { ApiError } from "../../api/client";
import type { LoginResponse } from "../../types/auth";
import "./LoginForm.css";

type Status = "idle" | "submitting" | "error";

interface LoginFormProps {
  onSuccess: (result: LoginResponse) => void;
  onNavigateToRegister: () => void;
}

export function LoginForm({ onSuccess, onNavigateToRegister }: LoginFormProps) {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [status, setStatus] = useState<Status>("idle");
  const [errorMessages, setErrorMessages] = useState<string[]>([]);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setStatus("submitting");
    setErrorMessages([]);

    try {
      const result = await login(email, password);
      onSuccess(result);
    } catch (err) {
      setStatus("error");
      setErrorMessages(
        err instanceof ApiError ? err.messages : ["Something went wrong. Try again."],
      );
    }
  }

  return (
    <form className="login-card" onSubmit={handleSubmit}>
      <h1 className="login-title">Log in</h1>

      <label className="login-field">
        <span>Email</span>
        <input
          type="email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          required
          autoComplete="email"
        />
      </label>

      <label className="login-field">
        <span>Password</span>
        <input
          type="password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
          autoComplete="current-password"
        />
      </label>

      {status === "error" && (
        <ul className="login-error">
          {errorMessages.map((msg) => (
            <li key={msg}>{msg}</li>
          ))}
        </ul>
      )}

      <button type="submit" disabled={status === "submitting"}>
        {status === "submitting" ? "Logging in…" : "Log in"}
      </button>

      <button type="button" className="login-link" onClick={onNavigateToRegister}>
        Need an account? Register
      </button>
    </form>
  );
}
