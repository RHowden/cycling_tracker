import { useState } from 'react';
import { useNavigate } from 'react-router';
import './Session.css';

interface SessionData {
    date: string;
    type: string;
    durationMin: number;
    avgHr: number;
    avgPower: number;
    distance: number;
}

export function Session() {
    // 1. Grab the dynamic :id parameter from the URL path
    //const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();

    // Mock initial data based on your specific fields
    const [session, _] = useState<SessionData>({
        date: '2026-09-23',
        type: 'Road Cycling',
        durationMin: 90,
        avgHr: 145,
        avgPower: 210,
        distance: 42.5,
    });


    return (
        <div className="session-detail-container">
            {/* Back to Dashboard bar */}
            <header className="session-header">
                <button className="btn-secondary" onClick={() => navigate('/dashboard')}>
                    ← Back to Dashboard
                </button>
                <h1>Session Details </h1>
            </header>

            <div className="session-card view-stats">
                <div className="stats-grid">
                    <div className="stat-box">
                        <span className="stat-label">Date</span>
                        <span className="stat-value">{session.date}</span>
                    </div>
                    <div className="stat-box">
                        <span className="stat-label">Type</span>
                        <span className="stat-value">{session.type}</span>
                    </div>
                    <div className="stat-box">
                        <span className="stat-label">Duration</span>
                        <span className="stat-value">{session.durationMin} mins</span>
                    </div>
                    <div className="stat-box">
                        <span className="stat-label">Distance</span>
                        <span className="stat-value">{session.distance} km</span>
                    </div>
                    <div className="stat-box">
                        <span className="stat-label">Avg Heart Rate</span>
                        <span className="stat-value">{session.avgHr} bpm</span>
                    </div>
                    <div className="stat-box">
                        <span className="stat-label">Avg Power</span>
                        <span className="stat-value">{session.avgPower} W</span>
                    </div>
                </div>
            </div>
        </div>
    );
}
