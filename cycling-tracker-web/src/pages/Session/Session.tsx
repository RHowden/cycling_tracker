import { useNavigate, useParams } from 'react-router';
import { useTrainingSession } from '../../hooks/useTrainingData';
import './Session.css';
import { apiRequestVoid } from '../../api/client';
import { useState } from 'react';
import { ApiError } from '../../api/apiErrors';

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
    const [isDeleting, setIsDeleting] = useState(false);
    const [deleteError, setErrorMessages] = useState<string[] | null>(null);

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

    const handleDelete = async () => {
        // 1. Browser "Are you sure?" confirmation
        const confirmed = window.confirm("Are you sure you want to delete this session? This action cannot be undone.");
        if (!confirmed) return;

        setIsDeleting(true);
        setErrorMessages(null);

        try {
            // 3. Make the API call via your void wrapper
            // If .NET returns a 204, this succeeds and returns nothing
            await apiRequestVoid(`/sessions/${id}`, { method: 'DELETE' });

            // 4. On success, instantly jump back to the dashboard
            navigate('/dashboard');

        } catch (err) {
            // 1. Clean, type-safe check against your custom API Error class
            if (err instanceof ApiError) {
                setErrorMessages(err.messages);
            }
            // 2. Fallback for native browser errors (like networking failures)
            else if (err instanceof Error) {
                setErrorMessages([err.message]);
            }
            // 3. Fallback for the absolute unexpected
            else {
                setErrorMessages(['An unknown error occurred.']);
            }

            // Unlock the button so they can try again if they want
            setIsDeleting(false);
        };
    }

    return (
        <div className="session-detail-container">
            <header className="session-header">
                {deleteError && <p className="error-text" style={{ color: 'red' }}>{deleteError}</p>}

                <h1>Session Details</h1>
                <button
                    onClick={handleDelete}
                    disabled={isDeleting}
                    className="delete-session-button"
                >
                    {isDeleting ? 'Deleting...' : 'Delete Session'}
                </button>
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

