export interface Session {
  id: number;
  date: string;
  type: string;
  duration: number;
  avgHr: number;
  avgPower: number;
  distance: number;
}

export type TrainingDay =
  | { date: string; hasSession: true; id: number; type: string; duration: number; avgHr: number;
      avgPower: number; distance: number;}
  | { date: string; hasSession: false; id: null; type: null; duration: null; avgHr: null;
      avgPower: null, distance: null;};