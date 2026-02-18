import { Link, Outlet, useNavigate } from "react-router-dom";
import { useAuthStore } from "../store/authStore";

export default function Layout() {
  const user = useAuthStore((s) => s.user);
  const logout = useAuthStore((s) => s.logout);
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate("/");
  };

  return (
    <div className="min-h-screen bg-slate-50 text-slate-800">
      <nav className="bg-gray-900 px-6 py-4 flex items-center justify-between">
        <Link to="/quiz" className="text-xl font-bold text-white">
          QuoteQuiz
        </Link>
        <div className="flex items-center gap-6">
          <Link to="/quiz" className="text-gray-300 hover:text-white transition-colors text-sm font-medium">
            Play
          </Link>
          <Link to="/history" className="text-gray-300 hover:text-white transition-colors text-sm font-medium">
            History
          </Link>
          <Link to="/settings" className="text-gray-300 hover:text-white transition-colors text-sm font-medium">
            Settings
          </Link>
          <span className="text-gray-600">|</span>
          <span className="text-sm text-gray-400">{user?.username}</span>
          <button
            onClick={handleLogout}
            className="text-sm text-red-400 hover:text-red-300 transition-colors cursor-pointer font-medium"
          >
            Logout
          </button>
        </div>
      </nav>

      <main className="max-w-4xl mx-auto px-6 py-10">
        <Outlet />
      </main>
    </div>
  );
}
