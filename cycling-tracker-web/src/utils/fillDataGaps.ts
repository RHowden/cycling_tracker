import { Session, TrainingDay } from "../api/schemas";

export function fillDateGaps(sessions: Session[]): TrainingDay[] {
  if (sessions.length === 0) return [];

  const toDateString = (date: Date) => date.toISOString().slice(0, 10);

  const orderedSessions = [...sessions].sort((a, b) => a.startTimeUtc.getTime() - b.startTimeUtc.getTime());

  const byDate = new Map(
    orderedSessions.map((s) => [toDateString(s.startTimeUtc), s])
  );
  
  const result: TrainingDay[] = [];

  // 3. Extract the clean string boundaries
  const startDateStr = toDateString(orderedSessions[0].startTimeUtc);
  const endDateStr = toDateString(orderedSessions[orderedSessions.length - 1].startTimeUtc);

  // 4. Initialize our loop cursor using pure UTC midnight boundaries
  const cursor = new Date(startDateStr);
  const end = new Date(endDateStr);

  while (cursor <= end) {    
    const currentDateStr = toDateString(cursor);
    const session = byDate.get(currentDateStr);

    result.push(
      session
        ? {
          date: currentDateStr, // Clean string for Recharts to consume effortlessly
          hasSession: true, 
          id: session.id, 
          sport: session.sport, 
          durationSeconds: session.durationSeconds,
          avgHeartRate: session.avgHeartRate, 
          avgPowerWatts: session.avgPowerWatts, 
          distanceMetres: session.distanceMetres
        }
        : {
          date: currentDateStr, // Pure missing gap day
          hasSession: false, 
          id: null, 
          sport: null, 
          durationSeconds: null,   
          avgHeartRate: null,   
          avgPowerWatts: null, 
          distanceMetres: null    
        }
    );

    // Securely move forward by exactly one calendar day
    cursor.setUTCDate(cursor.getUTCDate() + 1);
  }

  return result;
}
