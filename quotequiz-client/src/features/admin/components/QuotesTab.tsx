import { useState, useMemo } from 'react';
import { useAdminQuotes } from '../hooks/useAdminQuotes';
import QuoteForm from './QuoteForm';
import type { QuoteDto, CreateQuoteRequest } from '../types';

type SortKey = 'quoteText' | 'authorName' | 'createdByUserName' | 'createdAt';
type SortDir = 'asc' | 'desc';

export default function QuotesTab() {
  const { quotes, loading, error, create, update, remove } = useAdminQuotes();
  const [showForm, setShowForm] = useState(false);
  const [editQuote, setEditQuote] = useState<QuoteDto | null>(null);
  const [search, setSearch] = useState('');
  const [sortKey, setSortKey] = useState<SortKey>('createdAt');
  const [sortDir, setSortDir] = useState<SortDir>('desc');

  const filtered = useMemo(() => {
    let list = [...quotes];
    if (search) {
      const q = search.toLowerCase();
      list = list.filter((qt) => qt.quoteText.toLowerCase().includes(q) || qt.authorName.toLowerCase().includes(q));
    }
    list.sort((a, b) => {
      const av = a[sortKey] as string;
      const bv = b[sortKey] as string;
      return sortDir === 'asc' ? av.localeCompare(bv) : bv.localeCompare(av);
    });
    return list;
  }, [quotes, search, sortKey, sortDir]);

  const toggleSort = (key: SortKey) => {
    if (sortKey === key) setSortDir((d) => (d === 'asc' ? 'desc' : 'asc'));
    else { setSortKey(key); setSortDir('asc'); }
  };

  const handleSave = async (data: CreateQuoteRequest) => {
    if (editQuote) await update(editQuote.id, data);
    else await create(data);
  };

  const handleDelete = async (qt: QuoteDto) => {
    if (confirm(`Delete quote by "${qt.authorName}"? This cannot be undone.`)) await remove(qt.id);
  };

  const SortIcon = ({ k }: { k: SortKey }) => (
    <span className="ml-1 text-slate-400">{sortKey === k ? (sortDir === 'asc' ? '↑' : '↓') : '↕'}</span>
  );

  if (loading) return <div className="text-center py-10 text-slate-400">Loading quotes...</div>;
  if (error) return <div className="px-4 py-3 rounded-lg bg-red-50 border border-red-300 text-red-600 text-sm">{error}</div>;

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap gap-3 items-center justify-between">
        <input type="text" placeholder="Search quote or author..." value={search}
          onChange={(e) => setSearch(e.target.value)}
          className="px-3 py-2 rounded-lg bg-white border border-slate-300 text-slate-700 text-sm placeholder-slate-400 focus:outline-none focus:border-indigo-500" />
        <button onClick={() => { setEditQuote(null); setShowForm(true); }}
          className="px-4 py-2 bg-indigo-600 hover:bg-indigo-500 text-white rounded-lg text-sm font-medium transition-colors cursor-pointer shadow-sm">
          + Add Quote
        </button>
      </div>

      <div className="overflow-x-auto rounded-xl border border-slate-200 shadow-sm">
        <table className="w-full text-sm">
          <thead className="bg-slate-50 text-slate-500 border-b border-slate-200">
            <tr>
              <th className="px-4 py-3 text-left cursor-pointer hover:text-slate-800" onClick={() => toggleSort('quoteText')}>Quote <SortIcon k="quoteText" /></th>
              <th className="px-4 py-3 text-left cursor-pointer hover:text-slate-800" onClick={() => toggleSort('authorName')}>Author <SortIcon k="authorName" /></th>
              <th className="px-4 py-3 text-left cursor-pointer hover:text-slate-800" onClick={() => toggleSort('createdByUserName')}>Added By <SortIcon k="createdByUserName" /></th>
              <th className="px-4 py-3 text-left cursor-pointer hover:text-slate-800" onClick={() => toggleSort('createdAt')}>Created <SortIcon k="createdAt" /></th>
              <th className="px-4 py-3 text-center">Actions</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {filtered.length === 0 && (
              <tr><td colSpan={5} className="px-4 py-8 text-center text-slate-400">No quotes found</td></tr>
            )}
            {filtered.map((qt) => (
              <tr key={qt.id} className="bg-white hover:bg-slate-50 transition-colors">
                <td className="px-4 py-3 max-w-xs">
                  <span className="line-clamp-2 text-slate-600 italic">"{qt.quoteText}"</span>
                </td>
                <td className="px-4 py-3 font-medium text-slate-800 whitespace-nowrap">{qt.authorName}</td>
                <td className="px-4 py-3 text-slate-500">{qt.createdByUserName}</td>
                <td className="px-4 py-3 text-slate-400 whitespace-nowrap">{new Date(qt.createdAt).toLocaleDateString()}</td>
                <td className="px-4 py-3">
                  <div className="flex items-center justify-center gap-2">
                    <button onClick={() => { setEditQuote(qt); setShowForm(true); }} className="text-xs px-2 py-1 rounded bg-slate-100 hover:bg-indigo-100 hover:text-indigo-700 text-slate-600 transition-colors cursor-pointer">Edit</button>
                    <button onClick={() => handleDelete(qt)} className="text-xs px-2 py-1 rounded bg-slate-100 hover:bg-red-100 hover:text-red-600 text-slate-600 transition-colors cursor-pointer">Delete</button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      <p className="text-xs text-slate-400">{filtered.length} of {quotes.length} quotes</p>

      {showForm && (
        <QuoteForm quote={editQuote} onSave={handleSave} onClose={() => { setShowForm(false); setEditQuote(null); }} />
      )}
    </div>
  );
}
