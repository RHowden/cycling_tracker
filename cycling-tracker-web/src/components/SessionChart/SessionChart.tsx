import { useMemo } from 'react';
import {
    CartesianGrid,
    Legend,
    Line,
    LineChart,
    ResponsiveContainer,
    Tooltip,
    XAxis,
    YAxis,
} from 'recharts';
import type { SessionSamples } from '../../api/schemas'; // adjust path

function formatElapsed(seconds: number): string {
    const hrs = Math.floor(seconds / 3600);
    const mins = Math.floor((seconds % 3600) / 60);
    const secs = Math.floor(seconds % 60);
    const mm = String(mins).padStart(2, '0');
    const ss = String(secs).padStart(2, '0');
    return hrs > 0 ? `${hrs}:${mm}:${ss}` : `${mm}:${ss}`;
}

type Point = { time: number; power: number | null; heartRate: number | null };

export function SessionChart({ samples }: { samples: SessionSamples }) {
    const data = useMemo<Point[]>(
        () =>
            samples.time.map((t, i) => ({
                time: t,
                power: samples.power[i] ?? null,
                heartRate: samples.heartRate[i] ?? null,
            })),
        [samples]
    );

    const hasPower = data.some((p) => p.power !== null);
    const hasHeartRate = data.some((p) => p.heartRate !== null);

    if (data.length === 0 || (!hasPower && !hasHeartRate)) {
        return null;
    }

    return (
        <section className="session-card session-chart">
            <h2>Power &amp; Heart Rate</h2>
            <ResponsiveContainer width="100%" height={320}>
                <LineChart data={data} margin={{ top: 8, right: 16, left: 0, bottom: 0 }}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis
                        dataKey="time"
                        type="number"
                        domain={['dataMin', 'dataMax']}
                        tickFormatter={formatElapsed}
                    />
                    {hasPower && (
                        <YAxis
                            yAxisId="power"
                            orientation="left"
                            unit=" W"
                            width={60}
                        />
                    )}
                    {hasHeartRate && (
                        <YAxis
                            yAxisId="hr"
                            orientation="right"
                            unit=" bpm"
                            width={70}
                            domain={['dataMin - 5', 'dataMax + 5']}
                        />
                    )}
                    <Tooltip labelFormatter={(t) => formatElapsed(Number(t))} />
                    <Legend />
                    {hasPower && (
                        <Line
                            yAxisId="power"
                            type="monotone"
                            dataKey="power"
                            name="Power"
                            stroke="#2563eb"
                            dot={false}
                            connectNulls={false}
                            isAnimationActive={false}
                        />
                    )}
                    {hasHeartRate && (
                        <Line
                            yAxisId="hr"
                            type="monotone"
                            dataKey="heartRate"
                            name="Heart rate"
                            stroke="#dc2626"
                            dot={false}
                            connectNulls={false}
                            isAnimationActive={false}
                        />
                    )}
                </LineChart>
            </ResponsiveContainer>
        </section>
    );
}