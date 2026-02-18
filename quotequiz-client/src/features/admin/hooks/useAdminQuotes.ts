import { useState, useEffect } from 'react';
import axios from 'axios';
import { getQuotes, createQuote, updateQuote, deleteQuote } from '../api';
import type { QuoteDto, CreateQuoteRequest } from '../types';

export function useAdminQuotes() {
  const [quotes, setQuotes] = useState<QuoteDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const load = async () => {
    setLoading(true);
    try {
      const res = await getQuotes();
      setQuotes(res.data);
    } catch (err: unknown) {
      if (axios.isAxiosError(err)) {
        setError(err.response?.data?.message || 'Failed to load quotes');
      } else {
        setError('Failed to load quotes');
      }
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, []);

  const create = async (data: CreateQuoteRequest) => {
    await createQuote(data);
    await load();
  };

  const update = async (id: number, data: CreateQuoteRequest) => {
    await updateQuote(id, data);
    await load();
  };

  const remove = async (id: number) => {
    await deleteQuote(id);
    await load();
  };

  return { quotes, loading, error, create, update, remove };
}
