import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import axios from 'axios';
import { login as apiLogin, register as apiRegister } from '../api';
import { useAuthStore } from '../../../shared/store/authStore';
import type { LoginRequest, RegisterRequest } from '../types';

export function useAuth() {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const setUser = useAuthStore((s) => s.setUser);
  const navigate = useNavigate();

  const login = async (data: LoginRequest) => {
    setLoading(true);
    setError('');
    try {
      const res = await apiLogin(data);
      setUser(res.data);
      navigate('/quiz');
    } catch (err: unknown) {
      if (axios.isAxiosError(err)) {
        setError(err.response?.data?.message || 'Invalid credentials');
      } else {
        setError('Invalid credentials');
      }
    } finally {
      setLoading(false);
    }
  };

  const register = async (data: RegisterRequest) => {
    setLoading(true);
    setError('');
    try {
      const res = await apiRegister(data);
      setUser(res.data);
      navigate('/quiz');
    } catch (err: unknown) {
      if (axios.isAxiosError(err)) {
        setError(err.response?.data?.message || 'Registration failed');
      } else {
        setError('Registration failed');
      }
    } finally {
      setLoading(false);
    }
  };

  return { login, register, loading, error };
}
