import { useEffect, useState } from "react";

import { apiPost } from "../api/client";
import { Session, TrainingDay } from "../types/session";
import { fillDateGaps } from "../utils/fillDataGaps";

export function useTrainingData() {
  const [sessions, setSessions] = useState<TrainingDay[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function fetchSessions() {
      try {
        setLoading(true);
        const data = await apiPost<Session[]>("/sessions");
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
