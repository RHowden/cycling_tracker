import { Navigate, Outlet } from "react-router";

export function ProtectedRoute() {
  const token = localStorage.getItem("authToken");

  if (!token) {
    return <Navigate to="/login" replace />;
  }

  return <Outlet />;
}
