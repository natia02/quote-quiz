import { useState, useEffect } from 'react';
import type { QuoteDto, CreateQuoteRequest } from '../types';

interface Props {
  quote?: QuoteDto | null;
  onSave: (data: CreateQuoteRequest) => Promise<void>;
  onClose: () => void;
}

export default function QuoteForm({ quote, onSave, onClose }: Props) {
  const isEdit = !!quote;
  const [form, setForm] = useState({ quoteText: quote?.quoteText || '', authorName: quote?.authorName || '' });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    if (quote) setForm({ quoteText: quote.quoteText, authorName: quote.authorName });
  }, [quote]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    setError('');
    try {
      await onSave({ quoteText: form.quoteText, authorName: form.authorName });
      onClose();
    } catch {
      setError('Failed to save quote');
    } finally {
      setSaving(false);
    }
  };

  const inputClass = "w-full px-4 py-3 rounded-lg bg-white border border-slate-300 text-slate-800 placeholder-slate-400 focus:outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500";

  return (
    <div className="fixed inset-0 bg-black/30 flex items-center justify-center z-50 px-4">
      <div className="bg-white rounded-2xl p-8 w-full max-w-lg border border-slate-200 shadow-xl">
        <h2 className="text-xl font-bold text-slate-800 mb-6">{isEdit ? 'Edit Quote' : 'Add Quote'}</h2>

        {error && (
          <div className="mb-4 px-4 py-3 rounded-lg bg-red-50 border border-red-300 text-red-600 text-sm">{error}</div>
        )}

        <form onSubmit={handleSubmit} className="space-y-4">
          <textarea placeholder="Quote text" value={form.quoteText}
            onChange={(e) => setForm((f) => ({ ...f, quoteText: e.target.value }))}
            rows={4} className={`${inputClass} resize-none`} required />
          <input type="text" placeholder="Author name" value={form.authorName}
            onChange={(e) => setForm((f) => ({ ...f, authorName: e.target.value }))}
            className={inputClass} required />
          <div className="flex gap-3 pt-2">
            <button type="button" onClick={onClose}
              className="flex-1 py-3 rounded-lg bg-slate-100 hover:bg-slate-200 text-slate-600 font-medium transition-colors cursor-pointer">
              Cancel
            </button>
            <button type="submit" disabled={saving}
              className="flex-1 py-3 rounded-lg bg-indigo-600 hover:bg-indigo-500 disabled:opacity-50 text-white font-medium transition-colors cursor-pointer">
              {saving ? 'Saving...' : 'Save'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
