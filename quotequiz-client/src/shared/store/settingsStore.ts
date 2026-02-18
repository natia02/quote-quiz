import { create } from 'zustand';

export type QuizMode = 'Binary' | 'MultipleChoice';

interface SettingsStore {
  mode: QuizMode;
  setMode: (mode: QuizMode) => void;
}

export const useSettingsStore = create<SettingsStore>((set) => ({
  mode: (localStorage.getItem('quizMode') as QuizMode) || 'Binary',
  setMode: (mode) => {
    localStorage.setItem('quizMode', mode);
    set({ mode });
  },
}));
