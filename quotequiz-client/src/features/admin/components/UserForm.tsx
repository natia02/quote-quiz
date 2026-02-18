import { useState, useEffect } from 'react';
import type { UserDto, CreateUserRequest } from '../types';

interface Props {
  user?: UserDto | null;
  onSave: (data: CreateUserRequest | UserDto) => Promise<void>;
  onClose: () => void;
}

export default function UserForm({ user, onSave, onClose }: Props) {
  const isEdit = !!user;
  const [form, setForm] = useState({
    userName: user?.userName || '',
    email: user?.email || '',
    password: '',
    role: user?.role || 'User',
    isActive: user?.isActive ?? true,
  });
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    if (user) {
      setForm({ userName: user.userName, email: user.email, password: '', role: user.role, isActive: user.isActive });
    }
  }, [user]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    setError('');
    try {
      if (isEdit && user) {
        await onSave({ ...user, userName: form.userName, email: form.email, role: form.role, isActive: form.isActive });
      } else {
        await onSave({ userName: form.userName, email: form.email, password: form.password, role: form.role });
      }
      onClose();
    } catch {
      setError('Failed to save user');
    } finally {
      setSaving(false);
    }
  };

  const inputClass = "w-full px-4 py-3 rounded-lg bg-white border border-slate-300 text-slate-800 placeholder-slate-400 focus:outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500";

  return (
    <div className="fixed inset-0 bg-black/30 flex items-center justify-center z-50 px-4">
      <div className="bg-white rounded-2xl p-8 w-full max-w-md border border-slate-200 shadow-xl">
        <h2 className="text-xl font-bold text-slate-800 mb-6">{isEdit ? 'Edit User' : 'Create User'}</h2>

        {error && (
          <div className="mb-4 px-4 py-3 rounded-lg bg-red-50 border border-red-300 text-red-600 text-sm">{error}</div>
        )}

        <form onSubmit={handleSubmit} className="space-y-4">
          <input type="text" placeholder="Username" value={form.userName}
            onChange={(e) => setForm((f) => ({ ...f, userName: e.target.value }))}
            className={inputClass} required />
          <input type="email" placeholder="Email" value={form.email}
            onChange={(e) => setForm((f) => ({ ...f, email: e.target.value }))}
            className={inputClass} required />
          {!isEdit && (
            <input type="password" placeholder="Password (min 8 characters)" value={form.password}
              onChange={(e) => setForm((f) => ({ ...f, password: e.target.value }))}
              className={inputClass} required />
          )}
          <select value={form.role} onChange={(e) => setForm((f) => ({ ...f, role: e.target.value }))}
            className={inputClass}>
            <option value="User">User</option>
            <option value="Admin">Admin</option>
          </select>
          {isEdit && (
            <label className="flex items-center gap-3 cursor-pointer">
              <input type="checkbox" checked={form.isActive}
                onChange={(e) => setForm((f) => ({ ...f, isActive: e.target.checked }))}
                className="w-4 h-4 rounded accent-indigo-600" />
              <span className="text-slate-600 text-sm">Active</span>
            </label>
          )}
          <div className="flex gap-3 pt-2">
            <button type="button" onClick={onClose}
              className="flex-1 py-3 rounded-lg bg-slate-100 hover:bg-slate-200 text-slate-600 font-medium transition-colors cursor-pointer">
              Cancel
            </button>
            <button type="submit" disabled={saving}
              className="flex-1 py-3 rounded-lg bg-indigo-600 hover:bg-indigo-500 disabled:opacity-50 text-white font-medium transition-colors cursor-pointer">
              {saving ? 'Saving...' : 'Save'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
