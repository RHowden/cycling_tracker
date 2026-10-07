import { useNavigate, useParams } from 'react-router';
import { useTrainingSession } from '../../hooks/useTrainingData';
import './Session.css';

// Helper function to format duration (seconds -> MM:SS or HH:MM:SS)
function formatDuration(seconds: number): string {
    const hrs = Math.floor(seconds / 3600);
    const mins = Math.floor((seconds % 3600) / 60);
    const secs = seconds % 60;

    return hrs > 0 
        ? `${hrs}h ${mins}m ${secs}s` 
        : `${mins}m ${secs}s`;
}

export function Session() {
    const navigate = useNavigate();
    const { id } = useParams<{ id: string }>();
    
    // Pass an empty string fallback to prevent type runtime issues if id is undefined
    const { session, loading, error } = useTrainingSession(id ?? '');

    if (loading) {
        return (
            <div className="session-loading" role="status" aria-live="polite">
                <p>Loading session details…</p>
            </div>
        );
    }

    if (error) {
        return (
            <div className="session-error-container" role="alert">
                <p className="error-title">Error loading session:</p>
                <ul className="error-list">
                    {error.map((msg, i) => (
                        <li key={i}>{msg}</li>
                    ))}
                </ul>
                <button className="btn-secondary" onClick={() => navigate('/dashboard')}>
                    Return to Dashboard
                </button>
            </div>
        );
    }

    if (!session) {
        return (
            <div className="session-error-container">
                <p>No session data found.</p>
                <button className="btn-secondary" onClick={() => navigate('/dashboard')}>
                    Return to Dashboard
                </button>
            </div>
        );
    }

    // Safely parse date object
    const sessionDate = new Date(session.startTimeUtc);

    return (
        <div className="session-detail-container">
            <header className="session-header">
                <button 
                    className="btn-secondary" 
                    onClick={() => navigate('/dashboard')}
                    aria-label="Back to Dashboard"
                >
                    ← Back to Dashboard
                </button>
                <h1>Session Details</h1>
            </header>

            <main className="session-card view-stats">
                <div className="stats-grid">
                    <div className="stat-box">
                        <span className="stat-label">Date & Time</span>
                        <span className="stat-value">
                            {sessionDate.toLocaleDateString()} {sessionDate.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                        </span>
                    </div>
                    
                    <div className="stat-box">
                        <span className="stat-label">Activity Type</span>
                        <span className="stat-value">{session.sport}</span>
                    </div>
                    
                    <div className="stat-box">
                        <span className="stat-label">Duration</span>
                        <span className="stat-value">{formatDuration(session.durationSeconds)}</span>
                    </div>
                    
                    <div className="stat-box">
                        <span className="stat-label">Distance</span>
                        <span className="stat-value">{session.distanceMetres ? (session.distanceMetres / 1000).toFixed(2) : '--'} km</span>
                    </div>
                    
                    <div className="stat-box">
                        <span className="stat-label">Avg Heart Rate</span>
                        <span className="stat-value">{session.avgHeartRate ? `${session.avgHeartRate} bpm` : '--'}</span>
                    </div>
                    
                    <div className="stat-box">
                        <span className="stat-label">Avg Power</span>
                        <span className="stat-value">{session.avgPowerWatts ? `${session.avgPowerWatts} W` : '--'}</span>
                    </div>
                </div>
            </main>
        </div>
    );
}
