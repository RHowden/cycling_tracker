// src/api/schemas.ts
import { z } from "zod";

export const UserInfoSchema = z.object({
  email: z.string(),
  isEmailConfirmed: z.boolean()
});
export type UserInfo = z.infer<typeof UserInfoSchema>;

export const SessionIndexSchema = z.object({
  id: z.string(),
  startTimeUtc: z.iso.datetime({ offset: true }).transform((s) => new Date(s)),
  sport: z.string(),
  durationSeconds: z.number(),
  distanceMetres: z.number().nullable(),
  avgHeartRate: z.number().nullable(),
  avgPowerWatts: z.number().nullable(),
});

export const SessionSamplesSchema = z.object({
  time: z.array(z.number()),
  heartRate: z.array(z.number().nullable()),
  power: z.array(z.number().nullable()),
  speed: z.array(z.number().nullable()),
});

export const SessionSchema = SessionIndexSchema.extend({
  samples: SessionSamplesSchema.nullish(),
});

export const SessionsResponseSchema = z.array(SessionIndexSchema);

export type SessionIndexResponse = z.input<typeof SessionIndexSchema>;
export type SessionIndex = z.output<typeof SessionIndexSchema>;
export type SessionResponse = z.input<typeof SessionSchema>;
export type Session = z.output<typeof SessionSchema>;
export type SessionSamples = z.output<typeof SessionSamplesSchema>;

export const SportTypeEnum = ['Other', 'Running', 'Cycling', 'Walking', 'Strength'] as const;

export const CreateSessionSchema = z.object({
  sport: z.enum(SportTypeEnum, { 
    error: "Please select a valid sport type" 
  }),
  
  startTime: z.date({
    error: "A valid start date & time is required",
  }),

  durationSeconds: z.number({ error: "Duration is required" })
    .int("Must be a whole number")
    .min(1, "Duration must be at least 1 second")
    .max(86400, "Duration cannot exceed 24 hours"),

  avgHeartRate: z.number().int().min(30).max(250).nullable(),
  avgPowerWatts: z.number().int().min(0).max(2500).nullable(),
  distanceMetres: z.number().min(0).max(1000000).nullable(),
});


const zoneSetSchema = z
  .object({
    zone1Max: z.number().int("Must be a whole number").positive("Must be greater than 0"),
    zone2Max: z.number().int("Must be a whole number").positive("Must be greater than 0"),
  })
  .refine((zones) => zones.zone1Max < zones.zone2Max, {
    message: "Zone 2 max must be higher than zone 1 max",
    path: ["zone2Max"],
  });

export const athleteZonesSchema = z.object({
  heartRate: zoneSetSchema,
  power: zoneSetSchema,
});

export type ZoneSet = z.infer<typeof zoneSetSchema>;
export type AthleteZones = z.infer<typeof athleteZonesSchema>;
