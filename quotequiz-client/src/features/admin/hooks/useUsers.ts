import { useState, useEffect } from 'react';
import axios from 'axios';
import { getUsers, createUser, updateUser, disableUser, deleteUser } from '../api';
import type { UserDto, CreateUserRequest } from '../types';

export function useUsers() {
  const [users, setUsers] = useState<UserDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const load = async () => {
    setLoading(true);
    try {
      const res = await getUsers();
      setUsers(res.data);
    } catch (err: unknown) {
      if (axios.isAxiosError(err)) {
        setError(err.response?.data?.error || 'Failed to load users');
      } else {
        setError('Failed to load users');
      }
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => { load(); }, []);

  const create = async (data: CreateUserRequest) => {
    await createUser(data);
    await load();
  };

  const update = async (id: number, data: UserDto) => {
    await updateUser(id, data);
    await load();
  };

  const disable = async (id: number) => {
    await disableUser(id);
    await load();
  };

  const remove = async (id: number) => {
    await deleteUser(id);
    await load();
  };

  return { users, loading, error, create, update, disable, remove };
}
