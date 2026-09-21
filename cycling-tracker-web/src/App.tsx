import { useState } from "react";
import { RegisterForm } from "./components/RegisterForm/RegisterForm";
import { LoginForm } from "./components/LoginForm/LoginForm";
import { Dashboard } from "./components/Dashboard/Dashboard";
import type { LoginResponse } from "./types/auth";

type Page = "register" | "login" | "dashboard";

export default function App() {
  const [page, setPage] = useState<Page>("register");
  // Kept in memory only for now - lost on refresh. Persisting this (and
  // attaching it to future API calls) is a follow-up step, not needed
  // just to prove the login round-trip works.
  const [session, setSession] = useState<LoginResponse | null>(null);

  function handleLoginSuccess(result: LoginResponse) {
    setSession(result);
    setPage("dashboard");
  }

  function handleLogout() {
    setSession(null);
    setPage("login");
  }

  return (
    <main className="page">
      {page === "register" && (
        <RegisterForm onNavigateToLogin={() => setPage("login")} />
      )}
      {page === "login" && (
        <LoginForm
          onSuccess={handleLoginSuccess}
          onNavigateToRegister={() => setPage("register")}
        />
      )}
      {page === "dashboard" && session && <Dashboard onLogout={handleLogout} />}
    </main>
  );
}
