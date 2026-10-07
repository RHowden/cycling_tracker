import { describe, it, expect } from 'vitest';
import { fillDateGaps } from './fillDataGaps'; // Update with your actual path
import { Session } from '../api/schemas';

describe('fillDateGaps', () => {
    it('should fill date gaps with empty days and preserve sessions', () => {

        const latestSession = {
            id: 'session-2',
            sport: 'Cycling',
            durationSeconds: 3600,
            avgHeartRate: 140,
            avgPowerWatts: 200,
            distanceMetres: 25000,
            startTimeUtc: new Date('2026-10-03T08:00:00.000Z')
        }

        const earliestSession = {
            id: 'session-1',
            sport: 'Cycling',
            durationSeconds: 7200,
            avgHeartRate: 150,
            avgPowerWatts: 220,
            distanceMetres: 50000,
            startTimeUtc: new Date('2026-10-01T09:00:00.000Z')
        }
        const mockSessions = [latestSession, earliestSession] as unknown as Session[];


        const result = fillDateGaps(mockSessions);

        // Day 1: October 1st (Should have session data)
        // Day 1: October 1st (Earliest Session)
        const day1 = result[0];
        expect(day1.date).toBe(earliestSession.startTimeUtc.toISOString().slice(0, 10));
        expect(day1.hasSession).toBe(true);
        expect(day1.id).toBe(earliestSession.id);
        expect(day1.sport).toBe(earliestSession.sport);
        expect(day1.durationSeconds).toBe(earliestSession.durationSeconds);
        expect(day1.avgHeartRate).toBe(earliestSession.avgHeartRate);
        expect(day1.avgPowerWatts).toBe(earliestSession.avgPowerWatts);
        expect(day1.distanceMetres).toBe(earliestSession.distanceMetres);

        // Day 2: October 2nd (Cleanly filled gap)
        const day2 = result[1];
        expect(day2.date).toBe('2026-10-02');
        expect(day2.hasSession).toBe(false);
        expect(day2.id).toBeNull();
        expect(day2.sport).toBeNull();
        expect(day2.durationSeconds).toBeNull();
        expect(day2.avgHeartRate).toBeNull();
        expect(day2.avgPowerWatts).toBeNull();
        expect(day2.distanceMetres).toBeNull();

        // Day 3: October 3rd (Latest Session)
        const day3 = result[2]; // 👈 Fixed the index here from your copy! It should be 2, not 0
        expect(day3.date).toBe(latestSession.startTimeUtc.toISOString().slice(0, 10));
        expect(day3.hasSession).toBe(true);
        expect(day3.id).toBe(latestSession.id);
        expect(day3.sport).toBe(latestSession.sport);
        expect(day3.durationSeconds).toBe(latestSession.durationSeconds);
        expect(day3.avgHeartRate).toBe(latestSession.avgHeartRate);
        expect(day3.avgPowerWatts).toBe(latestSession.avgPowerWatts);
        expect(day3.distanceMetres).toBe(latestSession.distanceMetres);


        // Expect 3 days total: Oct 1, Oct 2 (gap), Oct 3
        expect(result).toHaveLength(3);
    });
});
