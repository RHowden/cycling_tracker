import { useNavigate, useParams } from 'react-router';

import { useTrainingSession } from '../../hooks/useTrainingData';
import './Session.css';


export function Session() {
    const navigate = useNavigate();
    const { id } = useParams<{ id: string }>();
    const { session, loading, error } = useTrainingSession(id);

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
        <div className="session-detail-container">
            {/* Back to Dashboard bar */}
            <header className="session-header">
                <button className="btn-secondary" onClick={() => navigate('/dashboard')}>
                    ← Back to Dashboard
                </button>
                <h1>Session Details </h1>
            </header>

            {session && <div className="session-card view-stats">
                <div className="stats-grid">
                    <div className="stat-box">
                        <span className="stat-label">Date</span>
                        <span className="stat-value">{session.startTimeUtc.toLocaleTimeString()}</span>
                    </div>
                    <div className="stat-box">
                        <span className="stat-label">Type</span>
                        <span className="stat-value">{session.sport}</span>
                    </div>
                    <div className="stat-box">
                        <span className="stat-label">Duration</span>
                        <span className="stat-value">{session.durationSeconds} seconds</span>
                    </div>
                    <div className="stat-box">
                        <span className="stat-label">Distance</span>
                        <span className="stat-value">{session.distanceMetres} km</span>
                    </div>
                    <div className="stat-box">
                        <span className="stat-label">Avg Heart Rate</span>
                        <span className="stat-value">{session.avgHeartRate  } bpm</span>
                    </div>
                    <div className="stat-box">
                        <span className="stat-label">Avg Power</span>
                        <span className="stat-value">{session.avgPowerWatts} W</span>
                    </div>
                </div>
            </div>
            }
        </div>
    );
}
