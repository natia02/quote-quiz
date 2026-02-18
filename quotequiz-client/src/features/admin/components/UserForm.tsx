import { useState, useEffect } from 'react';
import { parseApiError } from '../../../shared/api/parseApiError';
import type { FieldErrors } from '../../../shared/api/parseApiError';
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
  const [fieldErrors, setFieldErrors] = useState<FieldErrors>({});

  useEffect(() => {
    if (user) {
      setForm({ userName: user.userName, email: user.email, password: '', role: user.role, isActive: user.isActive });
    }
  }, [user]);

  const handleChange = (field: string, value: string | boolean) => {
    setForm((f) => ({ ...f, [field]: value }));
    setError('');
    setFieldErrors({});
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    setError('');
    setFieldErrors({});
    try {
      if (isEdit && user) {
        await onSave({ ...user, userName: form.userName, email: form.email, role: form.role, isActive: form.isActive });
      } else {
        await onSave({ userName: form.userName, email: form.email, password: form.password, role: form.role });
      }
      onClose();
    } catch (err: unknown) {
      const { generalError, fieldErrors: fe } = parseApiError(err, 'Failed to save user');
      setError(generalError);
      setFieldErrors(fe);
    } finally {
      setSaving(false);
    }
  };

  const inputClass = (field: string) =>
    `w-full px-4 py-3 rounded-lg bg-white border text-slate-800 placeholder-slate-400 focus:outline-none focus:ring-1 transition-colors ${
      fieldErrors[field]
        ? 'border-red-400 focus:border-red-400 focus:ring-red-400'
        : 'border-slate-300 focus:border-indigo-500 focus:ring-indigo-500'
    }`;

  return (
    <div className="fixed inset-0 bg-black/30 flex items-center justify-center z-50 px-4">
      <div className="bg-white rounded-2xl p-8 w-full max-w-md border border-slate-200 shadow-xl">
        <h2 className="text-xl font-bold text-slate-800 mb-6">{isEdit ? 'Edit User' : 'Create User'}</h2>

        {error && (
          <div className="mb-4 px-4 py-3 rounded-lg bg-red-50 border border-red-300 text-red-600 text-sm">{error}</div>
        )}

        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-1">
            <input type="text" placeholder="Username" value={form.userName}
              onChange={(e) => handleChange('userName', e.target.value)}
              className={inputClass('username')} />
            {fieldErrors['username'] && <p className="text-red-500 text-xs px-1">{fieldErrors['username']}</p>}
          </div>

          <div className="space-y-1">
            <input type="text" placeholder="Email" value={form.email}
              onChange={(e) => handleChange('email', e.target.value)}
              className={inputClass('email')} />
            {fieldErrors['email'] && <p className="text-red-500 text-xs px-1">{fieldErrors['email']}</p>}
          </div>

          {!isEdit && (
            <div className="space-y-1">
              <input type="password" placeholder="Password (min 8 characters)" value={form.password}
                onChange={(e) => handleChange('password', e.target.value)}
                className={inputClass('password')} />
              {fieldErrors['password'] && <p className="text-red-500 text-xs px-1">{fieldErrors['password']}</p>}
            </div>
          )}

          <select value={form.role} onChange={(e) => handleChange('role', e.target.value)}
            className={inputClass('role')}>
            <option value="User">User</option>
            <option value="Admin">Admin</option>
          </select>

          {isEdit && (
            <label className="flex items-center gap-3 cursor-pointer">
              <input type="checkbox" checked={form.isActive}
                onChange={(e) => handleChange('isActive', e.target.checked)}
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
