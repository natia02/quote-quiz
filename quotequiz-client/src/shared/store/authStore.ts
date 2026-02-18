import {create} from 'zustand'

interface User{
    token: string;
    username: string;
    email: string;
    role: string;
}

interface AuthStore {
    user: User | null;
    setUser: (user: User) => void;
    logout: () => void;
}

export const useAuthStore = create<AuthStore>((set) => ({
     user: (() => {
      const stored = localStorage.getItem('user');
      return stored ? JSON.parse(stored) : null;
    })(),

    setUser: (user) => {
      localStorage.setItem('token', user.token);
      localStorage.setItem('user', JSON.stringify(user));
      set({ user });
    },

    logout: () => {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      set({ user: null });
    },
}));