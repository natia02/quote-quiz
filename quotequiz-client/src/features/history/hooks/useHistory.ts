import { useState, useEffect } from "react";
import { getMyHistory, getMyStats } from "../api";
import type { GameHistory, UserStats } from "../types";

export function useHistory() {
  const [history, setHistory] = useState<GameHistory[]>([]);
  const [stats, setStats] = useState<UserStats | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    Promise.all([getMyHistory(), getMyStats()])
      .then(([h, s]) => {
        setHistory(h.data);
        setStats(s.data);
      })
      .catch(() => setError("Failed to load history"))
      .finally(() => setLoading(false));
  }, []);

  return { history, stats, loading, error };
}
