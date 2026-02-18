import { useState, useEffect } from 'react';
import { parseApiError } from '../../../shared/api/parseApiError';
import type { FieldErrors } from '../../../shared/api/parseApiError';
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
  const [fieldErrors, setFieldErrors] = useState<FieldErrors>({});

  useEffect(() => {
    if (quote) setForm({ quoteText: quote.quoteText, authorName: quote.authorName });
  }, [quote]);

  const handleChange = (field: string, value: string) => {
    setForm((f) => ({ ...f, [field]: value }));
    setError('');
    setFieldErrors({});
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    setError('');
    setFieldErrors({});
    try {
      await onSave({ quoteText: form.quoteText, authorName: form.authorName });
      onClose();
    } catch (err: unknown) {
      const { generalError, fieldErrors: fe } = parseApiError(err, 'Failed to save quote');
      setError(generalError);
      setFieldErrors(fe);
    } finally {
      setSaving(false);
    }
  };

  const inputClass = (field: string) =>
    `w-full px-4 py-3 rounded-lg bg-white border text-slate-800 placeholder-slate-400 focus:outline-none focus:ring-1 transition-colors ${
      fieldErrors[field]
        ? 'border-red-400 focus:border-red-400 focus:ring-red-400'
        : 'border-slate-300 focus:border-indigo-500 focus:ring-indigo-500'
    }`;

  return (
    <div className="fixed inset-0 bg-black/30 flex items-center justify-center z-50 px-4">
      <div className="bg-white rounded-2xl p-8 w-full max-w-lg border border-slate-200 shadow-xl">
        <h2 className="text-xl font-bold text-slate-800 mb-6">{isEdit ? 'Edit Quote' : 'Add Quote'}</h2>

        {error && (
          <div className="mb-4 px-4 py-3 rounded-lg bg-red-50 border border-red-300 text-red-600 text-sm">{error}</div>
        )}

        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-1">
            <textarea placeholder="Quote text" value={form.quoteText}
              onChange={(e) => handleChange('quoteText', e.target.value)}
              rows={4} className={`${inputClass('quotetext')} resize-none`} />
            {fieldErrors['quotetext'] && <p className="text-red-500 text-xs px-1">{fieldErrors['quotetext']}</p>}
          </div>

          <div className="space-y-1">
            <input type="text" placeholder="Author name" value={form.authorName}
              onChange={(e) => handleChange('authorName', e.target.value)}
              className={inputClass('authorname')} />
            {fieldErrors['authorname'] && <p className="text-red-500 text-xs px-1">{fieldErrors['authorname']}</p>}
          </div>

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
