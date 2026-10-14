import { Outlet, Link, useNavigate } from "react-router";

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
    <div className="app-layout-container">
      {/* ─── LEFT SIDEBAR ─── */}
      <aside className="app-sidebar">
        <div className="sidebar-logo">
          <h2> Cycling Tracker</h2>
        </div>

        <nav className="sidebar-links">
          <Link to="/dashboard" className="sidebar-link">Dashboard</Link>
        </nav>

        <nav className="sidebar-links">
          <Link to="/upload" className="sidebar-link">Upload</Link>
        </nav>

        <nav className="sidebar-links">
          <Link to="/zones" className="sidebar-link">Zones</Link>
        </nav>

        <button onClick={handleSignOut} className="sign-out-button">
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
