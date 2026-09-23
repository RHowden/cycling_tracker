import { useState, FormEvent } from "react";
import { Outlet, Link, useNavigate } from "react-router";
import type { LoginResponse } from "../../types/auth";
import "./AppLayout.css";

export function AppLayout() {
  const navigate = useNavigate();
  const [session, setSession] = useState<LoginResponse | null>(null);

  const handleSignOut = () => {
    setSession(null);
    
    // 2. Kick them back to the login page
    navigate("/login");
  };


  return (
    <div className="app-layout-container">
      {/* ─── LEFT SIDEBAR ─── */}
      <aside className="app-sidebar">
        <div className="sidebar-logo">
          <h2> Cycling Tracker</h2>
        </div>
        
        <nav className="sidebar-links">
          <Link to="/dashboard" className="sidebar-link">Dashboard Home</Link>
          <Link to="/dashboard/upload" className="sidebar-link">Upload File</Link>
          <Link to="/dashboard/resources/create" className="sidebar-link">New Resource</Link>
        </nav>

        <div className="sidebar-footer">
          <button onClick={handleSignOut} className="sign-out-button">
            Sign Out
          </button>
        </div>
      </aside>

      {/* ─── RIGHT MAIN WRAPPER ─── */}
      <div className="app-main-wrapper">
        <header className="app-top-header">
          <span>Welcome Back, Athlete!</span>
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
