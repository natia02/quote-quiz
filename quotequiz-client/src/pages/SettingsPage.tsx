import { useState } from 'react';
import { useAuthStore } from '../shared/store/authStore';
import { useSettingsStore } from '../shared/store/settingsStore';
import type { QuizMode } from '../shared/store/settingsStore';
import { useHistory } from '../features/history/hooks/useHistory';
import StatsGrid from '../features/history/components/StatsGrid';
import UsersTab from '../features/admin/components/UsersTab';
import QuotesTab from '../features/admin/components/QuotesTab';
import AchievementsTab from '../features/admin/components/AchievementsTab';

type AdminTab = 'users' | 'quotes' | 'achievements';

export default function SettingsPage() {
  const user = useAuthStore((s) => s.user);
  const { mode, setMode } = useSettingsStore();
  const { stats, loading: statsLoading } = useHistory();
  const [adminTab, setAdminTab] = useState<AdminTab>('users');

  const isAdmin = user?.role === 'Admin';

  const modes: { value: QuizMode; label: string; description: string }[] = [
    { value: 'Binary', label: 'Yes / No', description: 'A quote is shown with an author name — you decide if it matches.' },
    { value: 'MultipleChoice', label: 'Multiple Choice', description: 'Pick the correct author from four options.' },
  ];

  return (
    <div className="space-y-10">
      <h1 className="text-2xl font-bold text-slate-800">Settings</h1>

      {/* Quiz Mode */}
      <section className="space-y-4">
        <h2 className="text-lg font-semibold text-slate-700">Quiz Mode</h2>
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          {modes.map((m) => (
            <button
              key={m.value}
              onClick={() => setMode(m.value)}
              className={`text-left p-5 rounded-xl border-2 transition-all cursor-pointer ${
                mode === m.value
                  ? 'border-indigo-500 bg-indigo-50'
                  : 'border-slate-200 bg-white hover:border-indigo-300'
              }`}
            >
              <p className="font-semibold text-slate-800 mb-1">{m.label}</p>
              <p className="text-sm text-slate-500">{m.description}</p>
              {mode === m.value && (
                <span className="mt-2 inline-block text-xs text-indigo-600 font-semibold">Active</span>
              )}
            </button>
          ))}
        </div>
      </section>

      {/* My Stats */}
      <section className="space-y-4">
        <h2 className="text-lg font-semibold text-slate-700">My Statistics</h2>
        {statsLoading ? (
          <p className="text-slate-400 text-sm">Loading stats...</p>
        ) : stats ? (
          <StatsGrid stats={stats} />
        ) : (
          <p className="text-slate-400 text-sm">No statistics yet — play some rounds first!</p>
        )}
      </section>

      {/* Admin Section */}
      {isAdmin && (
        <section className="space-y-4">
          <h2 className="text-lg font-semibold text-slate-700">Administration</h2>

          <div className="flex gap-1 bg-slate-100 rounded-xl p-1 w-fit">
            {([
              { key: 'users', label: 'Users' },
              { key: 'quotes', label: 'Quotes' },
              { key: 'achievements', label: 'Achievements' },
            ] as { key: AdminTab; label: string }[]).map((t) => (
              <button
                key={t.key}
                onClick={() => setAdminTab(t.key)}
                className={`px-4 py-2 rounded-lg text-sm font-medium transition-colors cursor-pointer ${
                  adminTab === t.key
                    ? 'bg-indigo-600 text-white shadow-sm'
                    : 'text-slate-500 hover:text-slate-700'
                }`}
              >
                {t.label}
              </button>
            ))}
          </div>

          {adminTab === 'users' && <UsersTab />}
          {adminTab === 'quotes' && <QuotesTab />}
          {adminTab === 'achievements' && <AchievementsTab />}
        </section>
      )}
    </div>
  );
}
