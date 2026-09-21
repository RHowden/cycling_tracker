import "./Dashboard.css";

interface DashboardProps {
  onLogout: () => void;
}

export function Dashboard({ onLogout }: DashboardProps) {
  return (
    <div className="dashboard-card">
      <h1 className="dashboard-title">Dashboard</h1>
      <p className="dashboard-note">
        You're logged in. Ride list and training charts land here next.
      </p>
      <button type="button" onClick={onLogout}>
        Log out
      </button>
    </div>
  );
}
