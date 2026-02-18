export interface QuizQuestion {
  quoteId: number;
  quoteText: string;
  displayedAuthor: string;
  options: string[];
  quizMode: "Binary" | "MultipleChoice";
}

export interface AnswerResult {
  isCorrect: boolean;
  correctAnswer: string;
  message: string;
}

export type QuizMode = "Binary" | "MultipleChoice";
