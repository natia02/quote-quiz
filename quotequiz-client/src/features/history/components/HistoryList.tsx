import type { GameHistory } from "../types";

interface Props {
  history: GameHistory[];
}

export default function HistoryList({ history }: Props) {
  if (history.length === 0) {
    return (
      <p className="text-slate-400 text-center py-10">No games played yet.</p>
    );
  }

  return (
    <div className="space-y-3">
      {history.map((h) => (
        <div
          key={h.id}
          className={`flex items-start gap-4 p-4 rounded-xl border ${
            h.isCorrect
              ? "bg-green-50 border-green-200"
              : "bg-red-50 border-red-200"
          }`}
        >
          <span
            className={`text-lg mt-0.5 font-bold ${h.isCorrect ? "text-green-500" : "text-red-500"}`}
          >
            {h.isCorrect ? "✓" : "✗"}
          </span>
          <div className="flex-1 min-w-0">
            <p className="text-sm text-slate-600 italic truncate">
              "{h.quoteText}"
            </p>
            <p className="text-xs text-slate-400 mt-1">
              {h.authorName} · {h.quizMode} · Your answer: {h.selectedAnswer}
            </p>
          </div>
          <span className="text-xs text-slate-400 whitespace-nowrap">
            {new Date(h.answeredAt).toLocaleDateString()}
          </span>
        </div>
      ))}
    </div>
  );
}
