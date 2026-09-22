// hooks/useTrainingData.ts
import { useEffect, useState } from "react";
import { Session, TrainingDay } from "../types/session";
import { fillDateGaps } from "../utils/fillDataGaps";

const API_BASE = "https://localhost:7285/api";

export function useTrainingData() {
  const [sessions, setSessions] = useState<TrainingDay[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function fetchSessions() {
      try {
        setLoading(true);
        const res = await fetch(`${API_BASE}/sessions`);
        if (!res.ok) throw new Error(`Request failed: ${res.status}`);
        const data: Session[] = await res.json();
        if (!cancelled) setSessions(fillDateGaps(data));
      } catch (err) {
        if (!cancelled) setError(err instanceof Error ? err.message : "Unknown error");
      } finally {
        if (!cancelled) setLoading(false);
      }
    }

    fetchSessions();
    return () => { cancelled = true; };
  }, []);

  return { sessions, loading, error };
}
