// src/router.tsx
import { createBrowserRouter, Navigate } from "react-router";

import { ProtectedRoute } from "./components/ProtectedRoute";
import { ErrorElement } from "./components/ErrorElement/ErrorElement.tsx";

import AuthLayout from "./layouts/AuthLayout/AuthLayout";
import { LoginForm } from "./pages/LoginForm/LoginForm";
import { RegisterForm } from "./pages/RegisterForm/RegisterForm";

import { AppLayout } from "./layouts/AppLayout/AppLayout";
import { Dashboard } from "./pages/Dashboard/Dashboard";
import { Session } from "./pages/Session/Session";
import { FitUpload } from "./pages/FileUpload/FitUpload.tsx" 

import { NotFound } from './pages/NotFound/NotFound.tsx';

export const router = createBrowserRouter([
  // Automatically redirect a bare "/" URL straight to the login page
  {
    path: "/",
    element: <Navigate to="/login" replace />,
  },

  // ─── 1. PUBLIC / AUTH ROUTES (Nested under AuthLayout) ───
  {
    element: <AuthLayout />, // <-- No 'path' property means it wraps both routes cleanly
    errorElement: <ErrorElement />,
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
  {
    element: <ProtectedRoute />,
    children: [
      {
        path: "/dashboard",
        element: <AppLayout />,
        errorElement: <ErrorElement />,
        children: [
          {
            index: true,
            element: <Dashboard />,
          },
          {
            path: "sessions/:id",
            element: <Session />,
          },
        ],
      },
      {
        path: "/upload",
        element: <AppLayout />,
        errorElement: <ErrorElement />,
        children: [
          {
            index: true,
            element: <FitUpload />,
          },
        ],
      }
    ]
  },
  {
    path: "*",
    element: <NotFound />,
  },
]);
