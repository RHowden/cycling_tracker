import { useEffect, useState } from "react";

import { apiRequest } from "../api/client";
import { SessionSchema, SessionsResponseSchema } from  "../api/schemas";
import { ApiError } from "../api/apiErrors";
import z from "zod";

export function useTrainingData() {
  const { data , loading, error } = useApiResource("/sessions", SessionsResponseSchema);
  return { sessions: data ?? [], loading, error };
}

export function useTrainingSession(id: string | undefined) {
  const { data: session, loading, error } = useApiResource(`/sessions/${id}`, SessionSchema);
  return { session, loading, error };
}

function useApiResource<T extends z.ZodType>(url: string | null, schema: T ) {
  const [data, setData] = useState<z.output<T> | null>(null);
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
        const result = await apiRequest(nullCheckedUrl, schema);
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

