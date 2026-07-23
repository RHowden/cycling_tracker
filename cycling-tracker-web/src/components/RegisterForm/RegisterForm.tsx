import { useState, FormEvent } from "react";
import { register } from "../../api/auth";
import { ApiError } from "../../api/client";
import "./RegisterForm.css";

type Status = "idle" | "submitting" | "success" | "error";

export function RegisterForm() {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [status, setStatus] = useState<Status>("idle");
  const [errorMessage, setErrorMessage] = useState("");

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setStatus("submitting");
    setErrorMessage("");

    try {
      await register({ email, password });
      setStatus("success");
    } catch (err) {
      setStatus("error");
      setErrorMessage(
        err instanceof ApiError ? err.message : "Something went wrong. Try again.",
      );
    }
  }

  if (status === "success") {
    return (
      <div className="register-card">
        <p className="register-success">Account created. You can log in now.</p>
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

      {status === "error" && <p className="register-error">{errorMessage}</p>}

      <button type="submit" disabled={status === "submitting"}>
        {status === "submitting" ? "Creating account…" : "Register"}
      </button>
    </form>
  );
}
