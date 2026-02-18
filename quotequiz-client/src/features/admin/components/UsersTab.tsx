import { useState, useMemo } from 'react';
import { useUsers } from '../hooks/useUsers';
import UserForm from './UserForm';
import type { UserDto, CreateUserRequest } from '../types';

type SortKey = 'userName' | 'email' | 'role' | 'createdAt';
type SortDir = 'asc' | 'desc';

export default function UsersTab() {
  const { users, loading, error, create, update, disable, remove } = useUsers();
  const [showForm, setShowForm] = useState(false);
  const [editUser, setEditUser] = useState<UserDto | null>(null);
  const [roleFilter, setRoleFilter] = useState('All');
  const [statusFilter, setStatusFilter] = useState('All');
  const [search, setSearch] = useState('');
  const [sortKey, setSortKey] = useState<SortKey>('userName');
  const [sortDir, setSortDir] = useState<SortDir>('asc');

  const filtered = useMemo(() => {
    let list = [...users];
    if (roleFilter !== 'All') list = list.filter((u) => u.role === roleFilter);
    if (statusFilter === 'Active') list = list.filter((u) => u.isActive);
    if (statusFilter === 'Disabled') list = list.filter((u) => !u.isActive);
    if (search) list = list.filter((u) => u.userName.toLowerCase().includes(search.toLowerCase()) || u.email.toLowerCase().includes(search.toLowerCase()));
    list.sort((a, b) => {
      const av = a[sortKey] as string;
      const bv = b[sortKey] as string;
      return sortDir === 'asc' ? av.localeCompare(bv) : bv.localeCompare(av);
    });
    return list;
  }, [users, roleFilter, statusFilter, search, sortKey, sortDir]);

  const toggleSort = (key: SortKey) => {
    if (sortKey === key) setSortDir((d) => (d === 'asc' ? 'desc' : 'asc'));
    else { setSortKey(key); setSortDir('asc'); }
  };

  const handleSave = async (data: CreateUserRequest | UserDto) => {
    if (editUser) await update(editUser.id, data as UserDto);
    else await create(data as CreateUserRequest);
  };

  const handleDelete = async (user: UserDto) => {
    if (confirm(`Delete user "${user.userName}"? This cannot be undone.`)) await remove(user.id);
  };

  const SortIcon = ({ k }: { k: SortKey }) => (
    <span className="ml-1 text-slate-400">{sortKey === k ? (sortDir === 'asc' ? '↑' : '↓') : '↕'}</span>
  );

  const selectClass = "px-3 py-2 rounded-lg bg-white border border-slate-300 text-slate-700 text-sm focus:outline-none focus:border-indigo-500";

  if (loading) return <div className="text-center py-10 text-slate-400">Loading users...</div>;
  if (error) return <div className="px-4 py-3 rounded-lg bg-red-50 border border-red-300 text-red-600 text-sm">{error}</div>;

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap gap-3 items-center justify-between">
        <div className="flex flex-wrap gap-3">
          <input type="text" placeholder="Search name or email..." value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="px-3 py-2 rounded-lg bg-white border border-slate-300 text-slate-700 text-sm placeholder-slate-400 focus:outline-none focus:border-indigo-500" />
          <select value={roleFilter} onChange={(e) => setRoleFilter(e.target.value)} className={selectClass}>
            <option value="All">All roles</option>
            <option value="Admin">Admin</option>
            <option value="User">User</option>
          </select>
          <select value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)} className={selectClass}>
            <option value="All">All status</option>
            <option value="Active">Active</option>
            <option value="Disabled">Disabled</option>
          </select>
        </div>
        <button onClick={() => { setEditUser(null); setShowForm(true); }}
          className="px-4 py-2 bg-indigo-600 hover:bg-indigo-500 text-white rounded-lg text-sm font-medium transition-colors cursor-pointer shadow-sm">
          + Add User
        </button>
      </div>

      <div className="overflow-x-auto rounded-xl border border-slate-200 shadow-sm">
        <table className="w-full text-sm">
          <thead className="bg-slate-50 text-slate-500 border-b border-slate-200">
            <tr>
              <th className="px-4 py-3 text-left cursor-pointer hover:text-slate-800" onClick={() => toggleSort('userName')}>Username <SortIcon k="userName" /></th>
              <th className="px-4 py-3 text-left cursor-pointer hover:text-slate-800" onClick={() => toggleSort('email')}>Email <SortIcon k="email" /></th>
              <th className="px-4 py-3 text-left cursor-pointer hover:text-slate-800" onClick={() => toggleSort('role')}>Role <SortIcon k="role" /></th>
              <th className="px-4 py-3 text-left">Status</th>
              <th className="px-4 py-3 text-left cursor-pointer hover:text-slate-800" onClick={() => toggleSort('createdAt')}>Created <SortIcon k="createdAt" /></th>
              <th className="px-4 py-3 text-center">Actions</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {filtered.length === 0 && (
              <tr><td colSpan={6} className="px-4 py-8 text-center text-slate-400">No users found</td></tr>
            )}
            {filtered.map((u) => (
              <tr key={u.id} className="bg-white hover:bg-slate-50 transition-colors">
                <td className="px-4 py-3 font-medium text-slate-800">{u.userName}</td>
                <td className="px-4 py-3 text-slate-500">{u.email}</td>
                <td className="px-4 py-3">
                  <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${u.role === 'Admin' ? 'bg-purple-100 text-purple-700' : 'bg-slate-100 text-slate-600'}`}>
                    {u.role}
                  </span>
                </td>
                <td className="px-4 py-3">
                  <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${u.isActive ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-600'}`}>
                    {u.isActive ? 'Active' : 'Disabled'}
                  </span>
                </td>
                <td className="px-4 py-3 text-slate-400">{new Date(u.createdAt).toLocaleDateString()}</td>
                <td className="px-4 py-3">
                  <div className="flex items-center justify-center gap-2">
                    <button onClick={() => { setEditUser(u); setShowForm(true); }} className="text-xs px-2 py-1 rounded bg-slate-100 hover:bg-indigo-100 hover:text-indigo-700 text-slate-600 transition-colors cursor-pointer">Edit</button>
                    {u.isActive && (
                      <button onClick={() => disable(u.id)} className="text-xs px-2 py-1 rounded bg-slate-100 hover:bg-yellow-100 hover:text-yellow-700 text-slate-600 transition-colors cursor-pointer">Disable</button>
                    )}
                    <button onClick={() => handleDelete(u)} className="text-xs px-2 py-1 rounded bg-slate-100 hover:bg-red-100 hover:text-red-600 text-slate-600 transition-colors cursor-pointer">Delete</button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      <p className="text-xs text-slate-400">{filtered.length} of {users.length} users</p>

      {showForm && (
        <UserForm user={editUser} onSave={handleSave} onClose={() => { setShowForm(false); setEditUser(null); }} />
      )}
    </div>
  );
}
