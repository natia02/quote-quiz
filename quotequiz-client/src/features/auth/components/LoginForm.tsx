import { useState } from "react";
import { useAuth } from "../hooks/useAuth";
import type { LoginRequest } from "../types";

export default function LoginForm() {
  const { login, loading, error, fieldErrors, clearErrors } = useAuth();
  const [form, setForm] = useState<LoginRequest>({
    emailOrUsername: "",
    password: "",
  });

  const handleChange = (field: keyof LoginRequest, value: string) => {
    setForm((f) => ({ ...f, [field]: value }));
    clearErrors();
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    login(form);
  };

  const inputClass = (field: string) =>
    `w-full px-4 py-3 rounded-lg bg-white border text-slate-800 placeholder-slate-400 focus:outline-none focus:ring-1 transition-colors ${
      fieldErrors[field]
        ? "border-red-400 focus:border-red-400 focus:ring-red-400"
        : "border-slate-300 focus:border-indigo-500 focus:ring-indigo-500"
    }`;

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      {error && (
        <div className="px-4 py-3 rounded-lg bg-red-50 border border-red-300 text-red-600 text-sm">
          {error}
        </div>
      )}

      <div className="space-y-1">
        <input
          type="text"
          placeholder="Email or username"
          value={form.emailOrUsername}
          onChange={(e) => handleChange("emailOrUsername", e.target.value)}
          className={inputClass("emailorusername")}
        />
        {fieldErrors["emailorusername"] && (
          <p className="text-red-500 text-xs px-1">{fieldErrors["emailorusername"]}</p>
        )}
      </div>

      <div className="space-y-1">
        <input
          type="password"
          placeholder="Password"
          value={form.password}
          onChange={(e) => handleChange("password", e.target.value)}
          className={inputClass("password")}
        />
        {fieldErrors["password"] && (
          <p className="text-red-500 text-xs px-1">{fieldErrors["password"]}</p>
        )}
      </div>

      <button
        type="submit"
        disabled={loading}
        className="w-full py-3 rounded-lg bg-indigo-600 hover:bg-indigo-500 disabled:opacity-50 text-white font-medium transition-colors cursor-pointer"
      >
        {loading ? "Signing in..." : "Sign In"}
      </button>
    </form>
  );
}
