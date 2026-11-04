import { Outlet, useNavigate, NavLink } from "react-router";

import "./AppLayout.css";
import { useAuth } from "../../components/AuthContext";

export function AppLayout() {
  const navigate = useNavigate();
  const { logout } = useAuth();
  
  const handleSignOut = async () => {
    try {
      await logout();

      navigate("/login");
    } catch (error) {
      console.error('Network error during logout:', error);
    }
  };

  return (
    <div className="app-layout-container layout-fullscreen-shell">
      {/* ─── LEFT SIDEBAR ─── */}
      <aside className="app-sidebar">
        <div className="sidebar-logo-wrapper app-brand-logo">
          <h2> Cycling Tracker</h2>
        </div>

        <nav className="sidebar-links">
          <NavLink to="/dashboard" className="nav-item-link">Dashboard</NavLink>
          <NavLink to="/upload" className="nav-item-link">Upload</NavLink>
          <NavLink to="/adhoc_session" className="nav-item-link">New Session</NavLink>
          <NavLink to="/zones" className="nav-item-link">Zones</NavLink>
        </nav>

        <button onClick={handleSignOut} className="btn btn-danger sign-out-button">
          Sign Out
        </button>
      </aside>

      {/* ─── RIGHT MAIN WRAPPER ─── */}
      <div className="app-main-wrapper">
        {/* ─── DYNAMIC CONTENT AREA ─── */}
        <main className="app-content-slot">
          {/* This is where DashboardHome, ResourceDetails, etc. will inject */}
          <Outlet />
        </main>
      </div>
    </div>
  );
}
