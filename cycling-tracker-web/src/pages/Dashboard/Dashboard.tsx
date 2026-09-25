import "./Dashboard.css";
import { useTrainingData } from "../../hooks/useTrainingData.ts";
import { SummaryChart } from "../../components/SummaryChart/SummaryChart.tsx";

export function Dashboard() {
  const { sessions, loading, error } = useTrainingData();

  if (loading) return <p>Loading…</p>;
  if (error) {
  return (
    <div>
      <p>Error loading sessions:</p>
      <ul>
        {error.map((msg, i) => (
          <li key={i}>{msg}</li>
        ))}
      </ul>
    </div>
  );
}

  return (
    <div className="dashboard-card">
      <h1 className="dashboard-title">Dashboard</h1>
      <SummaryChart data={sessions} />      
    </div>
  );
}

