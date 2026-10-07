import {
    BarChart,
    Bar,
    XAxis,
    YAxis,
    CartesianGrid,
    Tooltip,
    ResponsiveContainer
} from 'recharts';

import { TrainingDay } from '../../api/schemas';

import './SummaryChart.css'
import { useNavigate } from 'react-router';

interface SummaryChartProps {
    data: TrainingDay[] | undefined;
}

export function SummaryChart({ data }: SummaryChartProps) {
    if (!data || data.length === 0) {
        return (
            <div className="summary-chart__status">
                No sessions in this period yet.
            </div>
        );
    }

    const navigate = useNavigate();

    // Recharts passes the clicked item data into the handler callback
    const handleBarClick = (data: any) => {
        if (data && data.id) {
            // Navigates directly to /dashboard/sessions/123
            navigate(`sessions/${data.id}`);
        }
    };

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
                        <BarChart data={data} syncId="trainingTimeline" margin={{ top: 5, right: 5, left: -20, bottom: 5 }}>
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
                                onClick={handleBarClick}
                                style={{ cursor: "pointer" }} />
                        </BarChart>
                    </ResponsiveContainer>
                </div>
            </div>  
        </div>
    );
}

