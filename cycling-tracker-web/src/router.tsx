// src/router.tsx
import { createBrowserRouter, Navigate } from "react-router";

import AuthLayout from "./layouts/AuthLayout/AuthLayout";
import { LoginForm } from "./pages/LoginForm/LoginForm";
import { RegisterForm } from "./pages/RegisterForm/RegisterForm";

import { AppLayout } from "./layouts/AppLayout/AppLayout";
import { Dashboard } from "./pages/Dashboard/Dashboard";
import { Session } from "./pages/Session/Session";

export const router = createBrowserRouter([
  // Automatically redirect a bare "/" URL straight to the login page
  {
    path: "/",
    element: <Navigate to="/login" replace />,
  },

  // ─── 1. PUBLIC / AUTH ROUTES (Nested under AuthLayout) ───
  {
    element: <AuthLayout />, // <-- No 'path' property means it wraps both routes cleanly
    children: [
      {
        path: "login", // Matches "/login" exactly
        element: <LoginForm />,
      },
      {
        path: "register", // Matches "/register" exactly
        element: <RegisterForm />,
      },
    ],
  },

  // ─── 2. PROTECTED APPLICATION ROUTES (Nested under AppLayout) ───
  {
    path: "/dashboard",
    element: <AppLayout />, 
    children: [
      {
        index: true, // Matches "/dashboard" exactly
        element: <Dashboard />, 
      },
      {
        path: "sessions/:id", // Matches "/dashboard/sessions/123"
        element: <Session />, 
      },
    ],
  },
]);
