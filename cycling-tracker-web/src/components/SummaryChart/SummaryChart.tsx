import { useMemo } from 'react';
import {
    BarChart,
    Bar,
    XAxis,
    YAxis,
    CartesianGrid,
    Tooltip,
    ResponsiveContainer
} from 'recharts';

import type { SessionIndex } from '../../api/schemas';
import { toDailyDurations } from '../../utils/toDailyDurations';

import './SummaryChart.css'

// Chart colours (Recharts takes these as props, so they can't come from CSS)
const GRID = '#262c33';
const AXIS_TEXT = '#7d8994';
const BAR = '#9fe870';
const TOOLTIP_STYLE = {
    background: '#14181d',
    border: '1px solid #262c33',
    borderRadius: 6,
    color: '#e7ebee',
};

interface SummaryChartProps {
    sessions: SessionIndex[];
}

export function SummaryChart({ sessions }: SummaryChartProps) {
    // Hooks must run before any early return
    const days = useMemo(() => toDailyDurations(sessions), [sessions]);

    if (sessions.length === 0) {
        return (
            <div className="summary-chart__status">
                No sessions in this period yet.
            </div>
        );
    }

    return (
        <div className="training-chart">
            {/* Header and Toggle Controls */}
            <div className="training-chart-header">
                <div>
                    <h3 className="training-chart-title">Training Events Log</h3>
                </div>
            </div>

            {/* --- GRAPH 1: ANCHOR TIME DATA (STAYS FIXED) --- */}
            <div className="training-chart-graph-section">
                <div className="training-chart-graph-header">
                    <span className="training-chart-label-duration">Workout Duration</span>
                    <span className="training-chart-label-unit">Second</span>
                </div>
                <div className="training-chart-wrapper-top">
                    <ResponsiveContainer>
                        <BarChart data={days} syncId="trainingTimeline" margin={{ top: 5, right: 5, left: -20, bottom: 5 }}>
                            <CartesianGrid strokeDasharray="3 3" vertical={false} stroke={GRID} />
                            <XAxis dataKey="date" hide />
                            <YAxis
                                tick={{ fill: AXIS_TEXT, fontSize: 11 }}
                                axisLine={false}
                                tickLine={false}
                                tickFormatter={(val) => `${Math.round(val / 60)}m`}
                            />
                            <Tooltip
                                cursor={{ fill: BAR, opacity: 0.08 }}
                                contentStyle={TOOLTIP_STYLE}
                                formatter={(value: any) => [`${(value / 60).toFixed(1)} Min`, 'Duration']}
                            />
                            <Bar
                                dataKey="durationSeconds"
                                fill={BAR}
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