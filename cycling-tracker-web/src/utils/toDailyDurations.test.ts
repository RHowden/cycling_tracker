import { describe, it, expect } from "vitest";
import type { SessionIndex } from "../api/schemas";
import { toDailyDurations } from "./toDailyDurations";

// Local-time dates (month is 1-based here), so results don't depend on the machine's time zone
const at = (y: number, month: number, d: number, h = 10, min = 0) =>
  new Date(y, month - 1, d, h, min);

const session = (start: Date, durationSeconds: number): SessionIndex => ({
  id: String(start.getTime()),
  startTimeUtc: start,
  sport: "Cycling",
  durationSeconds,
  distanceMetres: null,
  avgHeartRate: null,
  avgPowerWatts: null,
});

describe("toDailyDurations", () => {
  it("returns an empty array when there are no sessions", () => {
    expect(toDailyDurations([])).toEqual([]);
  });

  it("returns one entry for a single session", () => {
    const result = toDailyDurations([session(at(2026, 10, 2), 3600)]);

    expect(result).toEqual([{ date: "2026-10-02", durationSeconds: 3600 }]);
  });

  it("sums multiple sessions on the same day into one entry", () => {
    const result = toDailyDurations([
      session(at(2026, 10, 2, 8), 3600),
      session(at(2026, 10, 2, 18), 1800),
    ]);

    expect(result).toEqual([{ date: "2026-10-02", durationSeconds: 5400 }]);
  });

  it("fills the days between sessions with zero duration", () => {
    const result = toDailyDurations([
      session(at(2026, 10, 1), 7200),
      session(at(2026, 10, 4), 3600),
    ]);

    expect(result).toEqual([
      { date: "2026-10-01", durationSeconds: 7200 },
      { date: "2026-10-02", durationSeconds: 0 },
      { date: "2026-10-03", durationSeconds: 0 },
      { date: "2026-10-04", durationSeconds: 3600 },
    ]);
  });

  it("gives the same result regardless of input order", () => {
    const oldest = session(at(2026, 10, 1), 7200);
    const newest = session(at(2026, 10, 3), 3600);

    // The API returns newest first
    expect(toDailyDurations([newest, oldest])).toEqual(toDailyDurations([oldest, newest]));
  });

  it("groups by local calendar day, so late-night and early-morning rides are separate days", () => {
    const result = toDailyDurations([
      session(at(2026, 10, 2, 23, 30), 1800),
      session(at(2026, 10, 3, 0, 30), 1800),
    ]);

    expect(result.map((d) => d.date)).toEqual(["2026-10-02", "2026-10-03"]);
  });

  it("stays continuous across a month boundary", () => {
    const result = toDailyDurations([
      session(at(2026, 10, 30), 3600),
      session(at(2026, 11, 2), 3600),
    ]);

    expect(result.map((d) => d.date)).toEqual([
      "2026-10-30",
      "2026-10-31",
      "2026-11-01",
      "2026-11-02",
    ]);
  });

  it("stays continuous across a clock change", () => {
    // UK clocks go back on 25 Oct 2026. Only meaningful when run in a time zone that
    // observes DST, but it passes everywhere and guards against a skipped or repeated day.
    const result = toDailyDurations([
      session(at(2026, 10, 24), 3600),
      session(at(2026, 10, 26), 3600),
    ]);

    expect(result.map((d) => d.date)).toEqual(["2026-10-24", "2026-10-25", "2026-10-26"]);
  });
});