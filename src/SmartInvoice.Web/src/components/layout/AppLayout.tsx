import { Outlet } from 'react-router-dom';
import { Sidebar } from './Sidebar';
import { Header } from './Header';

export function AppLayout() {
  return (
    <div className="min-h-screen bg-gray-50 dark:bg-[#0f1320]">
      <Sidebar />
      <Header />
      <main className="ml-64 pt-20 p-6">
        <Outlet />
      </main>
    </div>
  );
}
