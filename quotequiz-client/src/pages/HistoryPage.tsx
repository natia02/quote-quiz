import { useHistory } from "../features/history/hooks/useHistory";
import StatsGrid from "../features/history/components/StatsGrid";
import HistoryList from "../features/history/components/HistoryList";

export default function HistoryPage() {
  const { history, stats, loading, error } = useHistory();

  if (loading) {
    return <div className="text-center text-slate-400 py-20">Loading...</div>;
  }

  if (error) {
    return (
      <div className="px-4 py-3 rounded-lg bg-red-50 border border-red-300 text-red-600 text-sm">
        {error}
      </div>
    );
  }

  return (
    <div className="space-y-8">
      <h1 className="text-2xl font-bold text-slate-800">My History</h1>
      {stats && <StatsGrid stats={stats} />}
      <HistoryList history={history} />
    </div>
  );
}
