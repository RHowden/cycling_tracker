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

// "2026-10-13" -> "13 Oct" (noon avoids any timezone/DST day shift)
const formatDay = (iso: string) =>
    new Date(`${iso}T12:00:00`).toLocaleDateString('en-GB', { day: 'numeric', month: 'short' });

// 5400 -> "1.5h"
const formatHours = (seconds: number) => `${Number((seconds / 3600).toFixed(1))}h`;

// 5400 -> "1h 30m", 2700 -> "45m"
const formatDuration = (seconds: number) => {
    const totalMinutes = Math.round(seconds / 60);
    const h = Math.floor(totalMinutes / 60);
    const m = totalMinutes % 60;
    return h > 0 ? `${h}h ${m}m` : `${m}m`;
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
            {/* --- GRAPH 1: ANCHOR TIME DATA (STAYS FIXED) --- */}
            <div className="training-chart-graph-section">
                <div className="training-chart-graph-header">
                </div>
                <div className="training-chart-wrapper-top">
                    <ResponsiveContainer>
                        <BarChart data={days} syncId="trainingTimeline" margin={{ top: 5, right: 5, left: -20, bottom: 5 }}>
                            <CartesianGrid strokeDasharray="3 3" vertical={false} stroke={GRID} />
                            <XAxis
                                dataKey="date"
                                tickFormatter={formatDay}
                                tick={{ fill: AXIS_TEXT, fontSize: 11 }}
                                axisLine={{ stroke: GRID }}
                                tickLine={false}
                                minTickGap={24}
                            />
                            <YAxis
                                tick={{ fill: AXIS_TEXT, fontSize: 11 }}
                                axisLine={false}
                                tickLine={false}
                                tickCount={4}
                                tickFormatter={formatHours}
                            />
                            <Tooltip
                                cursor={{ fill: BAR, opacity: 0.08 }}
                                contentStyle={TOOLTIP_STYLE}
                                labelFormatter={(label) => formatDay(label as string)}
                                formatter={(value: any) => [formatDuration(Number(value)), 'Duration']}
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