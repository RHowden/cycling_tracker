import { Session, TrainingDay } from "../types/session.ts"
export function fillDateGaps(sessions: Session[]): TrainingDay[] {
  const byDate = new Map(sessions.map((s) => [s.date, s]));
  const result: TrainingDay[] = [];

  const startDate: string = sessions[0].date;
  const endDate: string = sessions[sessions.length - 1].date;

  const cursor = new Date(startDate);
  const end = new Date(endDate);

  while (cursor <= end) {
    const dateStr = cursor.toISOString().split('T')[0];
    const session = byDate.get(dateStr);

    result.push(
      session
        ? { date: dateStr, hasSession: true, id: session.id, type: session.type, duration: session.duration,
             avgHr: session.avgHr, avgPower: session.avgPower, distance: session.distance }
        : { date: dateStr, hasSession: false, id: null, type: null, duration: null, avgHr: null, 
            avgPower: null, distance: null }
    );

    cursor.setDate(cursor.getDate() + 1);
  }

  return result;
}