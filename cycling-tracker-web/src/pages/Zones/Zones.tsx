import { useNavigate } from 'react-router';

import { useZones } from '../../hooks/useZones';

import './Zones.css';

import type { ZoneSet } from '../../api/schemas';

interface ZoneSetCardProps {
    title: string;
    unit: string;
    zones: ZoneSet;
}

function ZoneSetCard({ title, unit, zones }: ZoneSetCardProps) {
    const { zone1Max, zone2Max } = zones;

    return (
        <section className="zone-card" aria-labelledby={`${unit}-zones-heading`}>
            <h2 id={`${unit}-zones-heading`}>{title}</h2>
            <dl className="zone-list">
                <div className="zone-row">
                    <dt>Zone 1</dt>
                    <dd>Up to {zone1Max} {unit}</dd>
                </div>
                <div className="zone-row">
                    <dt>Zone 2</dt>
                    <dd>{zone1Max + 1} to {zone2Max} {unit}</dd>
                </div>
                <div className="zone-row">
                    <dt>Zone 3</dt>
                    <dd>Above {zone2Max} {unit}</dd>
                </div>
            </dl>
        </section>
    );
}

export function Zones() {
    const navigate = useNavigate();
    const { data, loading, error } = useZones();

    if (loading) {
        return (
            <div className="zones-loading" role="status" aria-live="polite">
                <p>Loading zones…</p>
            </div>
        );
    }

    if (error) {
        return (
            <div className="zones-error-container" role="alert">
                <p className="error-title">Error loading zones:</p>
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

    if (!data) {
        return (
            <div className="zones-error-container">
                <p>No zones found.</p>
                <button className="btn-secondary" onClick={() => navigate('/dashboard')}>
                    Return to Dashboard
                </button>
            </div>
        );
    }

    return (
        <div className="zones-detail-container">
            <header className="zones-header">
                <h1>Training Zones</h1>
                <button className="btn-primary" onClick={() => { /* TODO: enter edit mode */ }}>
                    Edit
                </button>
            </header>

            <div className="zone-cards">
                <ZoneSetCard title="Heart rate" unit="bpm" zones={data.heartRate} />
                <ZoneSetCard title="Power" unit="W" zones={data.power} />
            </div>
        </div>
    );
}

