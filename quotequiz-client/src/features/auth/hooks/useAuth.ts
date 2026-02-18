import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { login as apiLogin, register as apiRegister } from '../api';
import { useAuthStore } from '../../../shared/store/authStore';
import { parseApiError } from '../../../shared/api/parseApiError';
import type { FieldErrors } from '../../../shared/api/parseApiError';
import type { LoginRequest, RegisterRequest } from '../types';

export function useAuth() {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [fieldErrors, setFieldErrors] = useState<FieldErrors>({});
  const setUser = useAuthStore((s) => s.setUser);
  const navigate = useNavigate();

  const clearErrors = () => {
    setError('');
    setFieldErrors({});
  };

  const login = async (data: LoginRequest) => {
    setLoading(true);
    clearErrors();
    try {
      const res = await apiLogin(data);
      setUser(res.data);
      navigate('/quiz');
    } catch (err: unknown) {
      const { generalError, fieldErrors: fe } = parseApiError(err, 'Invalid credentials');
      setError(generalError);
      setFieldErrors(fe);
    } finally {
      setLoading(false);
    }
  };

  const register = async (data: RegisterRequest) => {
    setLoading(true);
    clearErrors();
    try {
      const res = await apiRegister(data);
      setUser(res.data);
      navigate('/quiz');
    } catch (err: unknown) {
      const { generalError, fieldErrors: fe } = parseApiError(err, 'Registration failed');
      setError(generalError);
      setFieldErrors(fe);
    } finally {
      setLoading(false);
    }
  };

  return { login, register, loading, error, fieldErrors, clearErrors };
}
