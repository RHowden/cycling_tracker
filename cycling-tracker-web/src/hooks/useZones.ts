import { useEffect, useState } from "react";

import { apiRequest } from "../api/client";
import { AthleteZones, athleteZonesSchema } from "../api/schemas";
import { ApiError } from "../api/apiErrors";

export function useZones() {
  const [data, setData] = useState<AthleteZones | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string[] | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function fetchData() {
      try {
        setLoading(true);
        setError(null);
        const result = await apiRequest('/athlete/zones', athleteZonesSchema);
        if (!cancelled) setData(result);
      } catch (err) {
        if (!cancelled) setError(err instanceof ApiError ? err.messages : ["Unknown error"]);
      } finally {
        if (!cancelled) setLoading(false);
      }
    }

    fetchData();
    return () => { cancelled = true; };
  }, []);

  return { data, loading, error };
}
