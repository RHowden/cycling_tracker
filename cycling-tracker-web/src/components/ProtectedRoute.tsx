import { Navigate, Outlet } from "react-router";

import { useAuth } from "./AuthContext" 

export function ProtectedRoute() {
  const { status } = useAuth();

  if (status === "loading") return <div>Loading...</div>;
  if (status === "unauthenticated") return <Navigate to="/login" replace />;

  return <Outlet />;
}