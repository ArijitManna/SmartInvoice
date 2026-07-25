import { LogOut, User, Moon, Sun } from 'lucide-react';
import { useAppDispatch, useAppSelector } from '../../store/hooks';
import { logout } from '../../store/authSlice';
import { toggleTheme } from '../../store/themeSlice';
import { useNavigate } from 'react-router-dom';

export function Header() {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const { fullName, email } = useAppSelector((state) => state.auth);
  const { mode } = useAppSelector((state) => state.theme);

  const handleLogout = () => {
    dispatch(logout());
    navigate('/login');
  };

  return (
    <header className="fixed top-0 left-64 right-0 z-30 flex h-16 items-center justify-between border-b border-gray-200 bg-white px-6 dark:bg-[#0a0e1a] dark:border-slate-800">
      <div />
      <div className="flex items-center gap-4">
        {/* Theme toggle */}
        <button
          onClick={() => dispatch(toggleTheme())}
          className="rounded-lg p-2 text-gray-400 hover:bg-gray-100 hover:text-gray-600 dark:hover:bg-slate-800 dark:hover:text-white transition-colors"
          aria-label="Toggle theme"
        >
          {mode === 'dark' ? <Sun className="h-5 w-5" /> : <Moon className="h-5 w-5" />}
        </button>

        <div className="flex items-center gap-2">
          <div className="flex h-8 w-8 items-center justify-center rounded-full bg-blue-100 dark:bg-blue-600/20">
            <User className="h-4 w-4 text-blue-600 dark:text-blue-400" />
          </div>
          <div className="text-sm">
            <p className="font-medium text-gray-900 dark:text-white">{fullName}</p>
            <p className="text-xs text-gray-500 dark:text-slate-500">{email}</p>
          </div>
        </div>
        <button
          onClick={handleLogout}
          className="rounded-lg p-2 text-gray-400 hover:bg-gray-100 hover:text-gray-600 dark:hover:bg-slate-800 dark:hover:text-white transition-colors"
          aria-label="Logout"
        >
          <LogOut className="h-5 w-5" />
        </button>
      </div>
    </header>
  );
}
