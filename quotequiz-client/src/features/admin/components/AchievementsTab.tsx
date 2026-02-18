import { useState, useMemo } from 'react';
import { useAchievements } from '../hooks/useAchievements';

type SortKey = 'userName' | 'authorName' | 'quizMode' | 'isCorrect' | 'answeredAt';
type SortDir = 'asc' | 'desc';

export default function AchievementsTab() {
  const { history, loading, error } = useAchievements();
  const [search, setSearch] = useState('');
  const [modeFilter, setModeFilter] = useState('All');
  const [resultFilter, setResultFilter] = useState('All');
  const [sortKey, setSortKey] = useState<SortKey>('answeredAt');
  const [sortDir, setSortDir] = useState<SortDir>('desc');

  const filtered = useMemo(() => {
    let list = [...history];
    if (modeFilter !== 'All') list = list.filter((h) => h.quizMode === modeFilter);
    if (resultFilter === 'Correct') list = list.filter((h) => h.isCorrect);
    if (resultFilter === 'Wrong') list = list.filter((h) => !h.isCorrect);
    if (search) {
      const q = search.toLowerCase();
      list = list.filter((h) => h.userName.toLowerCase().includes(q) || h.authorName.toLowerCase().includes(q));
    }
    list.sort((a, b) => {
      if (sortKey === 'isCorrect') {
        const av = a.isCorrect ? 1 : 0;
        const bv = b.isCorrect ? 1 : 0;
        return sortDir === 'asc' ? av - bv : bv - av;
      }
      const av = a[sortKey] as string;
      const bv = b[sortKey] as string;
      return sortDir === 'asc' ? av.localeCompare(bv) : bv.localeCompare(av);
    });
    return list;
  }, [history, search, modeFilter, resultFilter, sortKey, sortDir]);

  const toggleSort = (key: SortKey) => {
    if (sortKey === key) setSortDir((d) => (d === 'asc' ? 'desc' : 'asc'));
    else { setSortKey(key); setSortDir('asc'); }
  };

  const SortIcon = ({ k }: { k: SortKey }) => (
    <span className="ml-1 text-slate-400">{sortKey === k ? (sortDir === 'asc' ? '↑' : '↓') : '↕'}</span>
  );

  const correctCount = filtered.filter((h) => h.isCorrect).length;
  const accuracy = filtered.length > 0 ? Math.round((correctCount / filtered.length) * 100) : 0;

  const selectClass = "px-3 py-2 rounded-lg bg-white border border-slate-300 text-slate-700 text-sm focus:outline-none focus:border-indigo-500";

  if (loading) return <div className="text-center py-10 text-slate-400">Loading achievements...</div>;
  if (error) return <div className="px-4 py-3 rounded-lg bg-red-50 border border-red-300 text-red-600 text-sm">{error}</div>;

  return (
    <div className="space-y-4">
      <div className="grid grid-cols-3 gap-3">
        <div className="bg-white rounded-xl p-4 text-center border border-slate-200 shadow-sm">
          <p className="text-2xl font-bold text-slate-800">{history.length}</p>
          <p className="text-xs text-slate-400 mt-1">Total Answers</p>
        </div>
        <div className="bg-white rounded-xl p-4 text-center border border-slate-200 shadow-sm">
          <p className="text-2xl font-bold text-green-600">{history.filter((h) => h.isCorrect).length}</p>
          <p className="text-xs text-slate-400 mt-1">Correct</p>
        </div>
        <div className="bg-white rounded-xl p-4 text-center border border-slate-200 shadow-sm">
          <p className="text-2xl font-bold text-indigo-600">{accuracy}%</p>
          <p className="text-xs text-slate-400 mt-1">Accuracy (filtered)</p>
        </div>
      </div>

      <div className="flex flex-wrap gap-3 items-center">
        <input type="text" placeholder="Search user or author..." value={search}
          onChange={(e) => setSearch(e.target.value)}
          className="px-3 py-2 rounded-lg bg-white border border-slate-300 text-slate-700 text-sm placeholder-slate-400 focus:outline-none focus:border-indigo-500" />
        <select value={modeFilter} onChange={(e) => setModeFilter(e.target.value)} className={selectClass}>
          <option value="All">All modes</option>
          <option value="Binary">Binary</option>
          <option value="MultipleChoice">Multiple Choice</option>
        </select>
        <select value={resultFilter} onChange={(e) => setResultFilter(e.target.value)} className={selectClass}>
          <option value="All">All results</option>
          <option value="Correct">Correct</option>
          <option value="Wrong">Wrong</option>
        </select>
      </div>

      <div className="overflow-x-auto rounded-xl border border-slate-200 shadow-sm">
        <table className="w-full text-sm">
          <thead className="bg-slate-50 text-slate-500 border-b border-slate-200">
            <tr>
              <th className="px-4 py-3 text-left cursor-pointer hover:text-slate-800" onClick={() => toggleSort('userName')}>User <SortIcon k="userName" /></th>
              <th className="px-4 py-3 text-left">Quote</th>
              <th className="px-4 py-3 text-left cursor-pointer hover:text-slate-800" onClick={() => toggleSort('authorName')}>Author <SortIcon k="authorName" /></th>
              <th className="px-4 py-3 text-left cursor-pointer hover:text-slate-800" onClick={() => toggleSort('quizMode')}>Mode <SortIcon k="quizMode" /></th>
              <th className="px-4 py-3 text-left">Answer</th>
              <th className="px-4 py-3 text-left cursor-pointer hover:text-slate-800" onClick={() => toggleSort('isCorrect')}>Result <SortIcon k="isCorrect" /></th>
              <th className="px-4 py-3 text-left cursor-pointer hover:text-slate-800" onClick={() => toggleSort('answeredAt')}>Date <SortIcon k="answeredAt" /></th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {filtered.length === 0 && (
              <tr><td colSpan={7} className="px-4 py-8 text-center text-slate-400">No records found</td></tr>
            )}
            {filtered.map((h) => (
              <tr key={h.id} className="bg-white hover:bg-slate-50 transition-colors">
                <td className="px-4 py-3 font-medium text-slate-800">{h.userName}</td>
                <td className="px-4 py-3 max-w-xs">
                  <span className="line-clamp-1 text-slate-500 italic">"{h.quoteText}"</span>
                </td>
                <td className="px-4 py-3 text-slate-700">{h.authorName}</td>
                <td className="px-4 py-3">
                  <span className="px-2 py-0.5 rounded-full text-xs font-medium bg-slate-100 text-slate-600">
                    {h.quizMode === 'MultipleChoice' ? 'Multi' : h.quizMode}
                  </span>
                </td>
                <td className="px-4 py-3 text-slate-500 max-w-[120px]">
                  <span className="truncate block">{h.selectedAnswer}</span>
                </td>
                <td className="px-4 py-3">
                  <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${h.isCorrect ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-600'}`}>
                    {h.isCorrect ? 'Correct' : 'Wrong'}
                  </span>
                </td>
                <td className="px-4 py-3 text-slate-400 whitespace-nowrap">{new Date(h.answeredAt).toLocaleDateString()}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      <p className="text-xs text-slate-400">{filtered.length} of {history.length} records — {correctCount} correct ({accuracy}%)</p>
    </div>
  );
}
