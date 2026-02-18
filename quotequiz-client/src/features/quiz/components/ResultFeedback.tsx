import type { AnswerResult } from "../types";

interface Props {
  result: AnswerResult;
  onNext: () => void;
}

export default function ResultFeedback({ result, onNext }: Props) {
  return (
    <div className="space-y-4">
      <div
        className={`p-5 rounded-xl border ${
          result.isCorrect
            ? "bg-green-50 border-green-300 text-green-700"
            : "bg-red-50 border-red-300 text-red-700"
        }`}
      >
        <p className="font-semibold text-lg mb-1">
          {result.isCorrect ? "Correct!" : "Wrong!"}
        </p>
        <p className="text-sm opacity-80">{result.message}</p>
      </div>

      <button
        onClick={onNext}
        className="w-full py-3 rounded-xl bg-indigo-600 hover:bg-indigo-500 text-white font-semibold transition-colors cursor-pointer shadow-sm"
      >
        Next Question
      </button>
    </div>
  );
}
