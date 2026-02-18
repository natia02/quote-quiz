export interface UserDto {
  id: number;
  userName: string;
  email: string;
  role: string;
  isActive: boolean;
  createdAt: string;
}

export interface CreateUserRequest {
  userName: string;
  email: string;
  password: string;
  role: string;
}

export interface QuoteDto {
  id: number;
  quoteText: string;
  authorName: string;
  createdByUserName: string;
  createdAt: string;
}

export interface CreateQuoteRequest {
  quoteText: string;
  authorName: string;
}

export interface AchievementItem {
  id: number;
  userName: string;
  quoteText: string;
  authorName: string;
  quizMode: string;
  selectedAnswer: string;
  isCorrect: boolean;
  answeredAt: string;
}
