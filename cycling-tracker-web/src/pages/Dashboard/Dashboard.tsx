import "./Dashboard.css";
import { useTrainingData } from "../../hooks/useTrainingData.ts";
import { SummaryChart } from "../../components/SummaryChart/SummaryChart.tsx";
import { useNavigate } from "react-router";

export function Dashboard() {
  const { sessions, loading, error } = useTrainingData();
  const navigate = useNavigate();

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

  const activeSessionsOnly = sessions.filter(day => day.hasSession).reverse();

  return (
    <div className="dashboard-card">
      <h1 className="dashboard-title">Dashboard</h1>
      <SummaryChart data={sessions} />

      {/* 2. Top-Level Summary Table */}
      <div className="table-container">
        <h2 className="table-title">Activity Breakdown</h2>
        <table className="summary-table">
          <thead>
            <tr>
              <th>Date</th>
              <th>Sport</th>
              <th>Distance</th>
              <th>Duration</th>
              <th>Avg Power</th>
              <th>Avg HR</th>
            </tr>
          </thead>
          <tbody>
            {activeSessionsOnly.length === 0 ? (
              <tr>
                <td colSpan={6} className="empty-row">No riding activities logged in this window.</td>
              </tr>
            ) : (
              activeSessionsOnly.map((day) => (
                <tr key={day.id} onClick={() => navigate(`sessions/${day.id}`)} // 👈 3. Make row clickable
                  className="clickable-row">                  
                  {/* Format the UTC string to a readable UK date format */}
                  <td>{new Date(day.date).toLocaleDateString('en-GB')}</td>
                  <td className="sport-tag">{day.sport}</td>
                  <td>{((day.distanceMetres || 0) / 1000).toFixed(1)} km</td>
                  <td>{Math.floor((day.durationSeconds || 0) / 60)} mins</td>
                  <td>{day.avgPowerWatts ? `${day.avgPowerWatts} W` : "—"}</td>
                  <td>{day.avgHeartRate ? `${day.avgHeartRate} bpm` : "—"}</td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}

