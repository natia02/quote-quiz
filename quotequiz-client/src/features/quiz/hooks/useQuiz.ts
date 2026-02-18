import { useState } from "react";
import axios from "axios";
import { getQuestion, submitAnswer } from "../api";
import { useSettingsStore } from "../../../shared/store/settingsStore";
import type { QuizQuestion, AnswerResult } from "../types";

export function useQuiz() {
  const mode = useSettingsStore((s) => s.mode);
  const [question, setQuestion] = useState<QuizQuestion | null>(null);
  const [result, setResult] = useState<AnswerResult | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [score, setScore] = useState({ correct: 0, total: 0 });

  const fetchQuestion = async () => {
    setLoading(true);
    setResult(null);
    setError("");
    try {
      const res = await getQuestion(mode);
      setQuestion(res.data);
    } catch (err: unknown) {
      if (axios.isAxiosError(err)) {
        setError(err.response?.data?.message || "Failed to load question");
      } else {
        setError("Failed to load question");
      }
    } finally {
      setLoading(false);
    }
  };

  const answer = async (selectedAnswer: string) => {
    if (!question || result) return;
    try {
      const res = await submitAnswer({
        quoteId: question.quoteId,
        selectedAnswer,
        quizMode: question.quizMode,
        displayedAuthor: question.displayedAuthor,
      });
      setResult(res.data);
      setScore((s) => ({
        correct: s.correct + (res.data.isCorrect ? 1 : 0),
        total: s.total + 1,
      }));
    } catch (err: unknown) {
      if (axios.isAxiosError(err)) {
        setError(err.response?.data?.message || "Failed to submit answer");
      } else {
        setError("Failed to submit answer");
      }
    }
  };

  return { mode, question, result, loading, error, score, fetchQuestion, answer };
}
