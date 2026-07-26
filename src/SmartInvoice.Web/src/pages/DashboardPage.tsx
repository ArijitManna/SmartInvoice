import { Link } from 'react-router-dom';
import { IndianRupee, FileText, AlertTriangle, CheckCircle, Clock, TrendingUp } from 'lucide-react';
import { AreaChart, Area, XAxis, YAxis, Tooltip, ResponsiveContainer } from 'recharts';
import { Card, CardContent, CardHeader, Badge } from '../components/ui';
import { useGetDashboardQuery } from '../store/dashboardApi';
import { useAppSelector } from '../store/hooks';

export default function DashboardPage() {
  const { data, isLoading } = useGetDashboardQuery();
  const { fullName } = useAppSelector((state) => state.auth);

  if (isLoading || !data) {
    return <div className="text-gray-500 dark:text-slate-400">Loading dashboard...</div>;
  }

  const stats = [
    { label: "Today's Sales", value: `₹${data.todaySales.toFixed(0)}`, icon: IndianRupee, color: 'text-green-600 bg-green-50 dark:bg-green-500/20 dark:text-green-400' },
    { label: 'Outstanding', value: `₹${data.outstandingAmount.toFixed(0)}`, icon: AlertTriangle, color: 'text-orange-600 bg-orange-50 dark:bg-orange-500/20 dark:text-orange-400' },
    { label: 'Invoices (Month)', value: data.invoicesCreatedThisMonth, icon: FileText, color: 'text-blue-600 bg-blue-50 dark:bg-blue-500/20 dark:text-blue-400' },
    { label: 'Paid', value: data.paidInvoices, icon: CheckCircle, color: 'text-green-600 bg-green-50 dark:bg-green-500/20 dark:text-green-400' },
    { label: 'Pending', value: data.pendingInvoices, icon: Clock, color: 'text-yellow-600 bg-yellow-50 dark:bg-yellow-500/20 dark:text-yellow-400' },
    { label: 'Overdue', value: data.overdueInvoices, icon: AlertTriangle, color: 'text-red-600 bg-red-50 dark:bg-red-500/20 dark:text-red-400' },
    { label: 'GST Collected', value: `₹${data.gstCollected.toFixed(0)}`, icon: TrendingUp, color: 'text-purple-600 bg-purple-50 dark:bg-purple-500/20 dark:text-purple-400' },
  ];

  return (
    <div>
      <div className="mb-6">
        <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Dashboard</h1>
        <p className="text-gray-500 dark:text-slate-400 mt-1">Welcome back, {fullName?.split(' ')[0]}! Here&apos;s what&apos;s happening with your business.</p>
      </div>

      {/* Stat Cards */}
      <div className="grid grid-cols-2 md:grid-cols-4 lg:grid-cols-7 gap-4 mb-8">
        {stats.map((stat) => (
          <Card key={stat.label}>
            <CardContent className="p-4">
              <div className={`inline-flex items-center justify-center rounded-lg p-2 mb-2 ${stat.color}`}>
                <stat.icon className="h-4 w-4" />
              </div>
              <p className="text-xs text-gray-500 dark:text-slate-500">{stat.label}</p>
              <p className="text-lg font-bold text-gray-900 dark:text-white">{stat.value}</p>
            </CardContent>
          </Card>
        ))}
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Monthly Revenue */}
        <Card>
          <CardHeader><h2 className="font-semibold text-gray-900 dark:text-white">Monthly Revenue</h2></CardHeader>
          <CardContent>
            {data.monthlyRevenue.length > 0 ? (
              <ResponsiveContainer width="100%" height={220}>
                <AreaChart data={data.monthlyRevenue} margin={{ top: 10, right: 10, left: 0, bottom: 0 }}>
                  <defs>
                    <linearGradient id="revenueGradient" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="5%" stopColor="#3b82f6" stopOpacity={0.3} />
                      <stop offset="95%" stopColor="#3b82f6" stopOpacity={0} />
                    </linearGradient>
                  </defs>
                  <XAxis dataKey="month" axisLine={false} tickLine={false} tick={{ fill: '#64748b', fontSize: 11 }} />
                  <YAxis axisLine={false} tickLine={false} tick={{ fill: '#64748b', fontSize: 11 }} tickFormatter={(v) => `₹${(v / 1000).toFixed(0)}K`} />
                  <Tooltip
                    contentStyle={{ backgroundColor: '#1e293b', border: 'none', borderRadius: '8px', color: '#fff' }}
                    labelStyle={{ color: '#94a3b8' }}
                    formatter={(value: number | string) => [`₹${Number(value).toLocaleString()}`, 'Revenue']}
                  />
                  <Area type="monotone" dataKey="amount" stroke="#3b82f6" strokeWidth={2} fill="url(#revenueGradient)" dot={{ fill: '#3b82f6', r: 4 }} activeDot={{ r: 6, fill: '#3b82f6' }} />
                </AreaChart>
              </ResponsiveContainer>
            ) : (
              <p className="text-sm text-gray-400 dark:text-slate-500">No revenue data yet</p>
            )}
          </CardContent>
        </Card>

        {/* Top Customers */}
        <Card>
          <CardHeader><h2 className="font-semibold text-gray-900 dark:text-white">Top Customers</h2></CardHeader>
          <CardContent className="p-0">
            <table className="w-full text-sm">
              <tbody className="divide-y divide-gray-100 dark:divide-slate-700/50">
                {data.topCustomers.map((c, i) => (
                  <tr key={c.customerId} className="hover:bg-gray-50 dark:hover:bg-slate-800/50">
                    <td className="px-4 py-3">
                      <span className="inline-flex items-center justify-center h-6 w-6 rounded-full bg-blue-100 dark:bg-blue-600/20 text-blue-700 dark:text-blue-400 text-xs font-medium mr-2">{i + 1}</span>
                      <span className="text-gray-900 dark:text-white">{c.name}</span>
                    </td>
                    <td className="px-4 py-3 text-right font-medium text-gray-900 dark:text-white">₹{c.totalAmount.toFixed(0)}</td>
                  </tr>
                ))}
                {data.topCustomers.length === 0 && <tr><td className="px-4 py-6 text-center text-gray-400 dark:text-slate-500">No data</td></tr>}
              </tbody>
            </table>
          </CardContent>
        </Card>

        {/* Recent Payments */}
        <Card>
          <CardHeader><h2 className="font-semibold text-gray-900 dark:text-white">Recent Payments</h2></CardHeader>
          <CardContent className="p-0">
            <table className="w-full text-sm">
              <tbody className="divide-y divide-gray-100 dark:divide-slate-700/50">
                {data.recentPayments.map((p) => (
                  <tr key={`${p.invoiceId}-${p.paymentDate}`} className="hover:bg-gray-50 dark:hover:bg-slate-800/50">
                    <td className="px-4 py-2">
                      <Link to={`/invoices/${p.invoiceId}`} className="text-blue-600 dark:text-blue-400 hover:underline">{p.invoiceNumber}</Link>
                      <p className="text-xs text-gray-500 dark:text-slate-500">{p.customerName}</p>
                    </td>
                    <td className="px-4 py-2 text-right">
                      <p className="font-medium text-gray-900 dark:text-white">₹{p.amount.toFixed(0)}</p>
                      <p className="text-xs text-gray-500 dark:text-slate-500">{new Date(p.paymentDate).toLocaleDateString()}</p>
                    </td>
                  </tr>
                ))}
                {data.recentPayments.length === 0 && <tr><td className="px-4 py-6 text-center text-gray-400 dark:text-slate-500">No payments yet</td></tr>}
              </tbody>
            </table>
          </CardContent>
        </Card>

        {/* Upcoming Due */}
        <Card>
          <CardHeader><h2 className="font-semibold text-gray-900 dark:text-white">Upcoming Due</h2></CardHeader>
          <CardContent className="p-0">
            <table className="w-full text-sm">
              <tbody className="divide-y divide-gray-100 dark:divide-slate-700/50">
                {data.upcomingDueInvoices.map((inv) => (
                  <tr key={inv.invoiceId} className="hover:bg-gray-50 dark:hover:bg-slate-800/50">
                    <td className="px-4 py-2">
                      <Link to={`/invoices/${inv.invoiceId}`} className="text-blue-600 dark:text-blue-400 hover:underline">{inv.invoiceNumber}</Link>
                      <p className="text-xs text-gray-500 dark:text-slate-500">{inv.customerName}</p>
                    </td>
                    <td className="px-4 py-2 text-right">
                      <p className="font-medium text-gray-900 dark:text-white">₹{inv.balanceDue.toFixed(0)}</p>
                      <Badge variant="warning">{new Date(inv.dueDate).toLocaleDateString()}</Badge>
                    </td>
                  </tr>
                ))}
                {data.upcomingDueInvoices.length === 0 && (
                  <tr><td className="px-4 py-6 text-center text-gray-400 dark:text-slate-500">No upcoming dues</td></tr>
                )}
              </tbody>
            </table>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
