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
                            <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#f3f4f6" />
                            <XAxis dataKey="date" hide />
                            <YAxis
                                tick={{ fill: '#9ca3af', fontSize: 11 }}
                                axisLine={false}
                                tickLine={false}
                                tickFormatter={(val) => `${Math.round(val / 60)}m`}
                            />
                            <Tooltip
                                cursor={{ fill: '#f3f4f6', opacity: 0.6 }}
                                formatter={(value: any) => [`${(value / 60).toFixed(1)} Min`, 'Duration']}
                            />
                            <Bar
                                dataKey="durationSeconds"
                                fill="#9ca3af"
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