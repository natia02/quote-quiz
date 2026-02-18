import { useQuiz } from "../features/quiz/hooks/useQuiz";
import QuizCard from "../features/quiz/components/QuizCard";

export default function QuizPage() {
  const { mode, question, result, loading, error, score, fetchQuestion, answer } = useQuiz();

  return (
    <div className="space-y-8">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold text-slate-800">Who Said It?</h1>
        <div className="flex items-center gap-4">
          <span className="text-xs px-3 py-1 rounded-full bg-indigo-100 text-indigo-600 border border-indigo-200 font-medium">
            {mode === "Binary" ? "Yes / No" : "Multiple Choice"}
          </span>
          <div className="text-sm text-slate-500">
            Score:{" "}
            <span className="text-slate-800 font-semibold">
              {score.correct}/{score.total}
            </span>
          </div>
        </div>
      </div>

      {error && (
        <div className="px-4 py-3 rounded-lg bg-red-50 border border-red-300 text-red-600 text-sm">
          {error}
        </div>
      )}

      {!question && !loading && (
        <div className="text-center py-20">
          <p className="text-slate-500 mb-6">Ready to test your quote knowledge?</p>
          <button
            onClick={fetchQuestion}
            className="px-8 py-3 bg-indigo-600 hover:bg-indigo-500 rounded-xl text-white font-semibold text-lg transition-colors cursor-pointer shadow-sm"
          >
            Start
          </button>
        </div>
      )}

      {loading && (
        <div className="text-center py-20 text-slate-400">Loading question...</div>
      )}

      {question && !loading && (
        <QuizCard
          question={question}
          result={result}
          onAnswer={answer}
          onNext={fetchQuestion}
        />
      )}
    </div>
  );
}
