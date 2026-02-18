import type { UserStats } from "../types";

interface Props {
  stats: UserStats;
}

export default function StatsGrid({ stats }: Props) {
  const items = [
    { label: "Total Games", value: stats.totalGames },
    { label: "Correct", value: stats.correctAnswers },
    { label: "Wrong", value: stats.wrongAnswers },
    { label: "Success Rate", value: `${stats.successRate}%` },
    { label: "Yes / No", value: stats.binaryGames },
    { label: "Multiple Choice", value: stats.multipleChoiceGames },
  ];

  return (
    <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
      {items.map((item) => (
        <div
          key={item.label}
          className="bg-white rounded-xl border border-slate-200 shadow-sm p-5 text-center"
        >
          <div className="text-2xl font-bold text-indigo-600">{item.value}</div>
          <div className="text-sm text-slate-500 mt-1">{item.label}</div>
        </div>
      ))}
    </div>
  );
}
