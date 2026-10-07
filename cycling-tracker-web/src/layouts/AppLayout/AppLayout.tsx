import { Outlet, Link, useNavigate } from "react-router";

import "./AppLayout.css";
import { apiRequestVoid } from "../../api/client";
import { useState } from "react";
import { UserInfo } from "../../api/schemas";

export function AppLayout() {
  const navigate = useNavigate();
  const [_, setUser] = useState<UserInfo | null>(null);

  const handleSignOut = async () => {
    try {
      await apiRequestVoid('/auth/logout', { method: 'Post' });

      setUser(null); // Clear your local React state
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

      </aside>

      {/* ─── RIGHT MAIN WRAPPER ─── */}
      <div className="app-main-wrapper">
        <header className="app-top-header">
          <button onClick={handleSignOut} className="sign-out-button">
            Sign Out
          </button>
        </header>

        {/* ─── DYNAMIC CONTENT AREA ─── */}
        <main className="app-content-slot">
          {/* This is where DashboardHome, ResourceDetails, etc. will inject */}
          <Outlet />
        </main>
      </div>
    </div>
  );
}
