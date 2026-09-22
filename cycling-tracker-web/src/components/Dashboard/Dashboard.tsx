import "./Dashboard.css";
import { useTrainingData } from "../../hooks/useTrainingData.ts";
import SummaryChart from "./SummaryChart.tsx";

interface DashboardProps {
  onLogout: () => void;
}

export function Dashboard({ onLogout }: DashboardProps) {
  const { sessions, loading, error } = useTrainingData();

  if (loading) return <p>Loading…</p>;
  if (error) return <p>Error loading sessions: {error}</p>;

  return (
    <div className="dashboard-card">
      <h1 className="dashboard-title">Dashboard</h1>
      <SummaryChart data={sessions} loading={loading} error={error} />
      <button type="button" onClick={onLogout}>
        Log out
      </button>
    </div>
  );
}

export default Dashboard;