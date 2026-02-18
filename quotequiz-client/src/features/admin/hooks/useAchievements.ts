import { useState, useEffect } from 'react';
import axios from 'axios';
import { getAllHistory } from '../api';
import type { AchievementItem } from '../types';

export function useAchievements() {
  const [history, setHistory] = useState<AchievementItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    getAllHistory()
      .then((res) => setHistory(res.data))
      .catch((err: unknown) => {
        if (axios.isAxiosError(err)) {
          setError(err.response?.data?.message || 'Failed to load achievements');
        } else {
          setError('Failed to load achievements');
        }
      })
      .finally(() => setLoading(false));
  }, []);

  return { history, loading, error };
}
