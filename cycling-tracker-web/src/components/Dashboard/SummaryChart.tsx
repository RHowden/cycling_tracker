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
import './SummaryChart.css'

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
        <div className="training-chart">
            {/* Header and Toggle Controls */}
            <div className="training-chart-header">
                <div>
                    <h3 className="training-chart-title">Training Events Log</h3>
                </div>

                {/* Toggle Menu */}
                <div className="training-chart-toggle-menu">
                    {Object.keys(SECONDARY_METRICS).map((key) => {
                        const metricKey = key as MetricKey;
                        const isSelected = activeKey === metricKey;
                        return (
                            <button
                                key={key}
                                onClick={() => setActiveKey(metricKey)}
                                className="training-chart-toggle-btn"
                                style={{
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
            <div className="training-chart-graph-section">
                <div className="training-chart-graph-header">
                    <span className="training-chart-label-duration">Workout Duration</span>
                    <span className="training-chart-label-unit">Min</span>
                </div>
                <div className="training-chart-wrapper-top">
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
                <div className="training-chart-graph-header">
                    <span style={{ fontSize: '13px', fontWeight: 600, color: activeMetric.color }}>
                        {activeMetric.label}
                    </span>
                    <span className="training-chart-label-unit">{activeMetric.unit}</span>
                </div>
                <div className="training-chart-wrapper-bottom">
                    <ResponsiveContainer>
                        <BarChart data={data} syncId="trainingTimeline" margin={{ top: 5, right: 5, left: -20, bottom: 20 }}>
                            <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#f3f4f6" />
                            <XAxis dataKey="date" hide />

                            <YAxis tick={{ fill: '#9ca3af', fontSize: 11 }} axisLine={false} tickLine={false} />

                            <Tooltip
                                cursor={{ fill: '#f3f4f6', opacity: 0.6 }}
                                formatter={(value) => [`${value} ${activeMetric.unit}`, activeMetric.label]}
                            />

                            <Bar
                                key={activeMetric.key}
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