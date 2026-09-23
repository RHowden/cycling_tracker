import { Outlet, NavLink } from "react-router";
import "./AuthLayout.css";

export default function AuthLayout() {
  return (
    <div className="auth-layout-wrapper">
      {/* ─── HORIZONTAL MENU BAR ─── */}
      <header className="auth-top-navbar">
        <div className="auth-nav-logo">
          <span>Cycling Tracker</span>
        </div>
        <nav className="auth-nav-links">
          {/* If /login is active, we append the "hidden" class */}
          <NavLink
            to="/login"
            className={({ isActive }) =>
              isActive ? "auth-nav-link hidden" : "auth-nav-link"
            }
          >
            Log In
          </NavLink>

          {/* If /register is active, we append the "hidden" class */}
          <NavLink
            to="/register"
            className={({ isActive }) =>
              isActive ? "auth-nav-link hidden" : "auth-nav-link"
            }
          >
            Register
          </NavLink>
        </nav>
      </header>

      {/* ─── CENTERED CONTENT WRAPPER ─── */}
      <main className="auth-page-body">
        <div >
          <Outlet />
        </div>
      </main>
    </div>
  );
}
