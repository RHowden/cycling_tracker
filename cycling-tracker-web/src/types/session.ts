export interface Session {
  id: number;
  date: string;
  type: string;
  durationMin: number;
  avgHr: number;
  avgPower: number;
  distance: number;
}

export type TrainingDay =
  | { date: string; hasSession: true; id: number; type: string; durationMin: number; avgHr: number;
      avgPower: number; distance: number;}
  | { date: string; hasSession: false; id: null; type: null; durationMin: null; avgHr: null;
      avgPower: null, distance: null;};