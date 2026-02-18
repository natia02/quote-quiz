export interface GameHistory {
  id: number;
  userName: string;
  quoteText: string;
  authorName: string;
  quizMode: string;
  selectedAnswer: string;
  isCorrect: boolean;
  answeredAt: string;
}

export interface UserStats {
  totalGames: number;
  correctAnswers: number;
  wrongAnswers: number;
  successRate: number;
  binaryGames: number;
  multipleChoiceGames: number;
}
