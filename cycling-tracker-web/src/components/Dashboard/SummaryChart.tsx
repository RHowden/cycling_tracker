import {
    BarChart,
    Bar,
    XAxis,
    YAxis,
    CartesianGrid,
    Tooltip,
    Legend,
    ResponsiveContainer
} from 'recharts';

import React, { useState } from 'react';
import { Session } from "../../types/session";

interface SummaryChartProps {
    data: Session[] | undefined;
    loading: boolean;
    error: Error | null;
}

interface MetricConfig {
    label: string;
    unit: string;
    color: string;
    key: string;
}

const SECONDARY_METRICS = {
    hr: { label: 'Avg HR', unit: 'BPM', color: '#dc2626', key: 'avgHr' },
    power: { label: 'Avg Power', unit: 'W', color: '#2563eb', key: 'avgPower' },
    distance: { label: 'Distance', unit: 'km', color: '#10b981', key: 'distance' },
} as const satisfies Record<string, MetricConfig>;

type MetricKey = keyof typeof SECONDARY_METRICS;

/**
 * SummaryChart
 *
 * Purely presentational — takes shaped session data and renders it.
 * Owns no fetching, no filter state, no period logic. Dashboard (or
 * whoever calls the data hook) is responsible for passing in `data`
 * already filtered/shaped the way it should be displayed.
 *
 * Expected shape of `data`, one object per session:
 *   {
 *     date: string,        // e.g. "2026-09-15" — used as the X axis label
 *     distanceKm: number,
 *     avgHr: number,
 *   }
 */
function SummaryChart({ data, loading, error }: SummaryChartProps) {
    if (loading) {
        return <div className="Summary-chart__status">Loading sessions…</div>;
    }

    if (error) {
        return (
            <div className="summary-chart__status summary-chart__status--error">
                Couldn't load your training data. {error.message}
            </div>
        );
    }

    if (!data || data.length === 0) {
        return (
            <div className="summary-chart__status">
                No sessions in this period yet.
            </div>
        );
    }


    //const [activeKey, setActiveKey] = useState('power');
    const [activeKey, setActiveKey] = useState<MetricKey>('hr');
    const activeMetric = SECONDARY_METRICS[activeKey];

    return (
        <div className="summary-chart">
            {/* Header and Toggle Controls */}
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
                <div>
                    <h3 style={{ margin: 0 }}>Training Events Log</h3>
                </div>

                {/* Toggle Menu */}
                <div style={{ display: 'flex', gap: '8px', background: '#f3f4f6', padding: '4px', borderRadius: '8px' }}>
                    {Object.keys(SECONDARY_METRICS).map((key) => {
                        const metricKey = key as MetricKey;
                        const isSelected = activeKey === metricKey;
                        return (
                            <button
                                key={key}
                                onClick={() => setActiveKey(metricKey)}
                                style={{
                                    padding: '6px 12px', border: 'none', borderRadius: '6px', fontSize: '14px', cursor: 'pointer',
                                    backgroundColor: isSelected ? '#fff' : 'transparent',
                                    color: isSelected ? '#1f2937' : '#4b5563',
                                    boxShadow: isSelected ? '0 1px 3px rgba(0,0,0,0.1)' : 'none',
                                }}
                            >
                                {SECONDARY_METRICS[metricKey].label}
                            </button>
                        );
                    })}
                </div>
            </div>

            {/* --- GRAPH 1: ANCHOR TIME DATA (STAYS FIXED) --- */}
            <div style={{ marginBottom: '16px' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', padding: '0 10px', marginBottom: '6px' }}>
                    <span style={{ fontSize: '13px', fontWeight: 600, color: '#4b5563' }}>Workout Duration</span>
                    <span style={{ fontSize: '12px', color: '#9ca3af' }}>Min</span>
                </div>
                <div style={{ width: '100%', height: 130 }}>
                    <ResponsiveContainer>
                        <BarChart data={data} syncId="trainingTimeline" margin={{ top: 5, right: 5, left: -20, bottom: 5 }}>
                            <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#f3f4f6" />
                            <XAxis dataKey="date" hide />
                            <YAxis tick={{ fill: '#9ca3af', fontSize: 11 }} axisLine={false} tickLine={false} />
                            <Tooltip
                                cursor={{ fill: '#f3f4f6', opacity: 0.6 }}
                                formatter={(value) => [`${value} Min`, 'Duration']}
                            />
                            <Bar dataKey="durationMin" fill="#9ca3af" radius={[4, 4, 0, 0]} barSize={16} />
                        </BarChart>
                    </ResponsiveContainer>
                </div>
            </div>

            {/* --- GRAPH 2: PERFORMANCE METRIC (CHANGES DYNAMICALLY) --- */}
            <div>
                <div style={{ display: 'flex', justifyContent: 'space-between', padding: '0 10px', marginBottom: '6px' }}>
                    <span style={{ fontSize: '13px', fontWeight: 600, color: activeMetric.color }}>{activeMetric.label}</span>
                    <span style={{ fontSize: '12px', color: '#9ca3af' }}>{activeMetric.unit}</span>
                </div>
                <div style={{ width: '100%', height: 155 }}>
                    <ResponsiveContainer>
                        <BarChart data={data} syncId="trainingTimeline" margin={{ top: 5, right: 5, left: -20, bottom: 20 }}>
                            <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#f3f4f6" />
                            <XAxis dataKey="date" hide />

                            {/* 🌟 The YAxis scale and color auto-update to match the selected button */}
                            <YAxis tick={{ fill: '#9ca3af', fontSize: 11 }} axisLine={false} tickLine={false} />

                            <Tooltip
                                cursor={{ fill: '#f3f4f6', opacity: 0.6 }}
                                formatter={(value) => [`${value} ${activeMetric.unit}`, activeMetric.label]}
                            />

                            {/* 🌟 The Bar targets the dynamic metric configuration properties */}
                            <Bar
                                key={activeMetric.key} // Forces fresh mount animation on selection change
                                dataKey={activeMetric.key}
                                fill={activeMetric.color}
                                radius={[4, 4, 0, 0]}
                                barSize={16}
                            />
                        </BarChart>
                    </ResponsiveContainer>
                </div>
            </div>
        </div>
    );
}

export default SummaryChart;