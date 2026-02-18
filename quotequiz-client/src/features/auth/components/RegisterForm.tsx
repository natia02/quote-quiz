import { useState } from "react";
import { useAuth } from "../hooks/useAuth";
import type { RegisterRequest } from "../types";

export default function RegisterForm() {
  const { register, loading, error } = useAuth();
  const [form, setForm] = useState<RegisterRequest>({
    userName: "",
    email: "",
    password: "",
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    register(form);
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      {error && (
        <div className="px-4 py-3 rounded-lg bg-red-50 border border-red-300 text-red-600 text-sm">
          {error}
        </div>
      )}

      <input
        type="text"
        placeholder="Username"
        value={form.userName}
        onChange={(e) => setForm((f) => ({ ...f, userName: e.target.value }))}
        className="w-full px-4 py-3 rounded-lg bg-white border border-slate-300 text-slate-800 placeholder-slate-400 focus:outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 transition-colors"
        required
      />

      <input
        type="email"
        placeholder="Email"
        value={form.email}
        onChange={(e) => setForm((f) => ({ ...f, email: e.target.value }))}
        className="w-full px-4 py-3 rounded-lg bg-white border border-slate-300 text-slate-800 placeholder-slate-400 focus:outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 transition-colors"
        required
      />

      <input
        type="password"
        placeholder="Password (min 8 characters)"
        value={form.password}
        onChange={(e) => setForm((f) => ({ ...f, password: e.target.value }))}
        className="w-full px-4 py-3 rounded-lg bg-white border border-slate-300 text-slate-800 placeholder-slate-400 focus:outline-none focus:border-indigo-500 focus:ring-1 focus:ring-indigo-500 transition-colors"
        required
      />

      <button
        type="submit"
        disabled={loading}
        className="w-full py-3 rounded-lg bg-indigo-600 hover:bg-indigo-500 disabled:opacity-50 text-white font-medium transition-colors cursor-pointer"
      >
        {loading ? "Creating account..." : "Create Account"}
      </button>
    </form>
  );
}
