import { useEffect, useState } from "react";

import { apiRequest, apiRequestVoid } from "../api/client";
import { AthleteZones, athleteZonesSchema } from "../api/schemas";
import { ApiError } from "../api/apiErrors";

export function useZones() {
  const [data, setData] = useState<AthleteZones | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string[] | null>(null);

  const [saving, setSaving] = useState(false);
  const [saveError, setSaveError] = useState<string[] | null>(null);

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

  // Returns true on success so the page knows when to leave edit mode.
  async function save(zones: AthleteZones): Promise<boolean> {
    setSaving(true);
    setSaveError(null);
    
    try {
      await apiRequestVoid("/athlete/zones", {
        method: "PUT",
        body: zones,
      });

      setData(zones); // 204: the server stored what we sent
      return true;
    } catch (err) {
      setSaveError(err instanceof ApiError ? err.messages : ["Unknown error"]);
      return false;
    } finally {
      setSaving(false);
    }
  }

  function clearSaveError() {
    setSaveError(null);
  }

  return { data, loading, error, saving, saveError, save, clearSaveError };
}
