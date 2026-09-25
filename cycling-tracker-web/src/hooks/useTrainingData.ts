import { useEffect, useState, useMemo } from "react";

import { apiRequest } from "../api/client";
import { Session } from "../types/session";
import { fillDateGaps } from "../utils/fillDataGaps";
import { ApiError } from "../api/ApiError";

function useApiResource<T>(url: string | null) {
  const [data, setData] = useState<T | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string[] | null>(null);

  useEffect(() => {
    if (!url) return;
    const nullCheckedUrl = url;
    let cancelled = false;

    async function fetchData() {
      try {
        setLoading(true);
        setError(null);
        const result = await apiRequest<T>(nullCheckedUrl);
        if (!cancelled) setData(result);
      } catch (err) {
        if (!cancelled) setError(err instanceof ApiError ? err.messages : ["Unknown error"]);
      } finally {
        if (!cancelled) setLoading(false);
      }
    }

    fetchData();
    return () => { cancelled = true; };
  }, [url]);

  return { data, loading, error };
}

export function useTrainingData() {
  const { data, loading, error } = useApiResource<Session[]>("/sessions");
  const sessions = useMemo(() => (data ? fillDateGaps(data) : []), [data]);
  return { sessions, loading, error };
}

export function useTrainingSession(id: string | undefined) {
  const { data: session, loading, error } = useApiResource<Session>(`/sessions/${id}`);
  return { session, loading, error };
}