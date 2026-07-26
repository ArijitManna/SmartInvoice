import { NavLink } from 'react-router-dom';
import {
  LayoutDashboard,
  Users,
  Package,
  FileText,
  CreditCard,
  BarChart3,
  Settings,
  Building2,
} from 'lucide-react';

const navItems = [
  { to: '/dashboard', label: 'Dashboard', icon: LayoutDashboard },
  { to: '/customers', label: 'Customers', icon: Users },
  { to: '/products', label: 'Products', icon: Package },
  { to: '/invoices', label: 'Invoices', icon: FileText },
  { to: '/payments', label: 'Payments', icon: CreditCard },
  { to: '/reports', label: 'Reports', icon: BarChart3 },
  { to: '/settings', label: 'Settings', icon: Settings },
];

export function Sidebar() {
  return (
    <aside className="fixed left-0 top-0 z-40 h-screen w-64 border-r border-gray-200 bg-white dark:bg-[#0a0e1a] dark:border-slate-800">
      <div className="flex h-16 items-center gap-2 border-b border-gray-200 dark:border-slate-800 px-6">
        <Building2 className="h-7 w-7 text-blue-600 dark:text-blue-400" />
        <span className="text-lg font-bold text-gray-900 dark:text-white">Invo<span className="text-blue-600 dark:text-blue-400">xa</span></span>
      </div>

      <nav className="mt-4 px-3">
        <ul className="space-y-1">
          {navItems.map((item) => (
            <li key={item.to}>
              <NavLink
                to={item.to}
                className={({ isActive }) =>
                  `flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium transition-colors ${
                    isActive
                      ? 'bg-blue-50 text-blue-700 dark:bg-blue-600/20 dark:text-blue-400'
                      : 'text-gray-600 hover:bg-gray-50 hover:text-gray-900 dark:text-slate-400 dark:hover:bg-slate-800 dark:hover:text-white'
                  }`
                }
              >
                <item.icon className="h-5 w-5" />
                {item.label}
              </NavLink>
            </li>
          ))}
        </ul>
      </nav>
    </aside>
  );
}
