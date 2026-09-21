import { useState, FormEvent } from "react";
import { register } from "../../api/auth";
import { ApiError } from "../../api/client";
import "./RegisterForm.css";

type Status = "idle" | "submitting" | "success" | "error";

interface RegisterFormProps {
  onNavigateToLogin: () => void;
}

export function RegisterForm({ onNavigateToLogin }: RegisterFormProps) {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [status, setStatus] = useState<Status>("idle");
  const [errorMessages, setErrorMessages] = useState<string[]>([]);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setStatus("submitting");
    setErrorMessages([]);

    try {
      await register({ email, password });
      setStatus("success");
    } catch (err) {
      setStatus("error");
      setErrorMessages(
        err instanceof ApiError ? err.messages : ["Something went wrong. Try again."],
      );
    }
  }

  if (status === "success") {
    return (
      <div className="register-card">
        <p className="register-success">Account created.</p>
        <button type="button" className="register-link" onClick={onNavigateToLogin}>
          Log in now
        </button>
      </div>
    );
  }

  return (
    <form className="register-card" onSubmit={handleSubmit}>
      <h1 className="register-title">Create your account</h1>

      <label className="register-field">
        <span>Email</span>
        <input
          type="email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          required
          autoComplete="email"
        />
      </label>

      <label className="register-field">
        <span>Password</span>
        <input
          type="password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          required
          autoComplete="new-password"
          minLength={8}
        />
      </label>

      {status === "error" && (
        <ul className="register-error">
          {errorMessages.map((msg) => (
            <li key={msg}>{msg}</li>
          ))}
        </ul>
      )}

      <button type="submit" disabled={status === "submitting"}>
        {status === "submitting" ? "Creating account…" : "Register"}
      </button>

      <button type="button" className="register-link" onClick={onNavigateToLogin}>
        Already have an account? Log in
      </button>
    </form>
  );
}
