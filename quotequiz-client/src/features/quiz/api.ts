import client from "../../shared/api/client";
import type { QuizQuestion, AnswerResult, QuizMode } from "./types";

export const getQuestion = (mode: QuizMode) =>
  client.get<QuizQuestion>("/quiz/question", { params: { mode } });

export const submitAnswer = (data: {
  quoteId: number;
  selectedAnswer: string;
  quizMode: string;
  displayedAuthor: string;
}) => client.post<AnswerResult>("/quiz/answer", data);
