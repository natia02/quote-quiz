import BinaryOptions from "./BinaryOptions";
import MultiChoiceOptions from "./MultiChoiceOptions";
import ResultFeedback from "./ResultFeedback";
import type { QuizQuestion, AnswerResult } from "../types";

interface Props {
  question: QuizQuestion;
  result: AnswerResult | null;
  onAnswer: (answer: string) => void;
  onNext: () => void;
}

export default function QuizCard({ question, result, onAnswer, onNext }: Props) {
  return (
    <div className="bg-white rounded-2xl border border-slate-200 shadow-sm p-8 space-y-6">
      <div className="bg-pink-500 rounded-xl p-6">
        <p className="text-white text-lg italic leading-relaxed font-medium">
          "{question.quoteText}"
        </p>
      </div>

      {result && (
        <p className="text-center text-lg text-slate-600 font-semibold">
          — {result.correctAnswer}
        </p>
      )}

      {!result && question.quizMode === "Binary" && (
        <BinaryOptions displayedAuthor={question.displayedAuthor} onAnswer={onAnswer} />
      )}

      {!result && question.quizMode === "MultipleChoice" && (
        <MultiChoiceOptions options={question.options} onAnswer={onAnswer} />
      )}

      {result && <ResultFeedback result={result} onNext={onNext} />}
    </div>
  );
}
