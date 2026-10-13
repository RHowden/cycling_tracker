import type { SessionIndex } from "../api/schemas";

export type DailyDuration = {
  date: string;            // YYYY-MM-DD, local time
  durationSeconds: number; // summed across that day's sessions; 0 on rest days
};

const dateKey = (d: Date) =>
  `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}-${String(d.getDate()).padStart(2, "0")}`;

export function toDailyDurations(sessions: SessionIndex[]): DailyDuration[] {
  if (sessions.length === 0) return [];

  // Sum durations per local calendar day
  const totals = new Map<string, number>();
  for (const s of sessions) {
    const key = dateKey(s.startTimeUtc);
    totals.set(key, (totals.get(key) ?? 0) + s.durationSeconds);
  }

  // YYYY-MM-DD strings sort correctly as plain text
  const keys = [...totals.keys()].sort();
  const last = keys[keys.length - 1];

  const [y, m, d] = keys[0].split("-").map(Number);
  const cursor = new Date(y, m - 1, d, 12); // noon, so a DST change can't shift the day

  const result: DailyDuration[] = [];
  for (let key = dateKey(cursor); key <= last; key = dateKey(cursor)) {
    result.push({ date: key, durationSeconds: totals.get(key) ?? 0 });
    cursor.setDate(cursor.getDate() + 1);
  }
  return result;
}