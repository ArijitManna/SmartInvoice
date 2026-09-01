import { useState } from 'react';
import { Button, Card, CardContent } from '../components/ui';
import { BarChart3, TrendingUp } from 'lucide-react';

export default function ExtendedReportsPage() {
  const [report, setReport] = useState<'hsn' | 'gstr1' | 'profit' | 'cashflow' | 'products' | 'tax'>('hsn');
  const [from, setFrom] = useState(new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0]);
  const [to, setTo] = useState(new Date().toISOString().split('T')[0]);
  const [loading, setLoading] = useState(false);
  const [data, setData] = useState<any>(null);

  const fetchReport = async () => {
    setLoading(true);
    try {
      const token = localStorage.getItem('token');
      const endpoint = {
        hsn: `/api/reports/hsn-summary?from=${from}&to=${to}`,
        gstr1: `/api/reports/gstr1?from=${from}&to=${to}`,
        profit: `/api/reports/profit-loss?from=${from}&to=${to}`,
        cashflow: `/api/reports/cash-flow?from=${from}&to=${to}`,
        products: `/api/reports/products?from=${from}&to=${to}`,
        tax: `/api/reports/tax-collected?from=${from}&to=${to}`,
      }[report];

      const response = await fetch(endpoint, {
        headers: { 'Authorization': `Bearer ${token}` },
      });

      if (response.ok) {
        const result = await response.json();
        setData(result);
      }
    } catch (error) {
      console.error('Report fetch failed:', error);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-900 dark:text-white mb-6">Extended Reports</h1>

      {/* Report Selection */}
      <Card className="mb-6">
        <CardContent className="p-6">
          <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-2 mb-6">
            {[
              { id: 'hsn', label: 'HSN Summary' },
              { id: 'gstr1', label: 'GSTR-1' },
              { id: 'profit', label: 'P&L' },
              { id: 'cashflow', label: 'Cash Flow' },
              { id: 'products', label: 'Products' },
              { id: 'tax', label: 'Tax Collected' },
            ].map((r) => (
              <button
                key={r.id}
                onClick={() => setReport(r.id as any)}
                className={`px-3 py-2 rounded-lg text-sm font-medium transition ${
                  report === r.id
                    ? 'bg-blue-600 text-white'
                    : 'bg-gray-100 text-gray-700 hover:bg-gray-200 dark:bg-slate-800 dark:text-slate-300 dark:hover:bg-slate-700'
                }`}
              >
                {r.label}
              </button>
            ))}
          </div>

          {/* Date Range */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-slate-300 mb-1">From</label>
              <input
                type="date"
                value={from}
                onChange={(e) => setFrom(e.target.value)}
                className="w-full rounded-lg border border-gray-300 dark:border-slate-600 dark:bg-[#1a2236] dark:text-white px-3 py-2 text-sm"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-slate-300 mb-1">To</label>
              <input
                type="date"
                value={to}
                onChange={(e) => setTo(e.target.value)}
                className="w-full rounded-lg border border-gray-300 dark:border-slate-600 dark:bg-[#1a2236] dark:text-white px-3 py-2 text-sm"
              />
            </div>
            <div className="flex items-end">
              <Button onClick={fetchReport} loading={loading} className="w-full">
                Generate Report
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Report Display */}
      {data && (
        <Card>
          <CardContent className="p-6">
            {report === 'hsn' && data.items && (
              <div>
                <h2 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">HSN Summary</h2>
                <div className="grid grid-cols-3 gap-4 mb-6">
                  <div className="bg-blue-50 dark:bg-blue-600/10 p-4 rounded-lg">
                    <p className="text-sm text-gray-600 dark:text-slate-400">Total Taxable Value</p>
                    <p className="text-2xl font-bold text-gray-900 dark:text-white">₹{data.totalTaxableValue?.toFixed(2)}</p>
                  </div>
                  <div className="bg-green-50 dark:bg-green-600/10 p-4 rounded-lg">
                    <p className="text-sm text-gray-600 dark:text-slate-400">Total Tax</p>
                    <p className="text-2xl font-bold text-gray-900 dark:text-white">₹{data.totalTax?.toFixed(2)}</p>
                  </div>
                  <div className="bg-purple-50 dark:bg-purple-600/10 p-4 rounded-lg">
                    <p className="text-sm text-gray-600 dark:text-slate-400">Items</p>
                    <p className="text-2xl font-bold text-gray-900 dark:text-white">{data.items?.length}</p>
                  </div>
                </div>
                <div className="overflow-x-auto">
                  <table className="w-full text-sm">
                    <thead>
                      <tr className="border-b border-gray-200 dark:border-slate-700 bg-gray-50 dark:bg-slate-800/50">
                        <th className="px-4 py-2 text-left">HSN</th>
                        <th className="px-4 py-2 text-right">Qty</th>
                        <th className="px-4 py-2 text-right">Taxable</th>
                        <th className="px-4 py-2 text-right">CGST</th>
                        <th className="px-4 py-2 text-right">SGST</th>
                        <th className="px-4 py-2 text-right">IGST</th>
                        <th className="px-4 py-2 text-right">Total Tax</th>
                      </tr>
                    </thead>
                    <tbody>
                      {data.items?.map((item: any, i: number) => (
                        <tr key={i} className="border-b border-gray-200 dark:border-slate-700">
                          <td className="px-4 py-2">{item.hsnCode}</td>
                          <td className="px-4 py-2 text-right">{item.quantity}</td>
                          <td className="px-4 py-2 text-right">₹{item.taxableValue?.toFixed(2)}</td>
                          <td className="px-4 py-2 text-right">₹{item.cgstAmount?.toFixed(2)}</td>
                          <td className="px-4 py-2 text-right">₹{item.sgstAmount?.toFixed(2)}</td>
                          <td className="px-4 py-2 text-right">₹{item.igstAmount?.toFixed(2)}</td>
                          <td className="px-4 py-2 text-right font-medium">₹{item.totalTax?.toFixed(2)}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>
            )}

            {report === 'profit' && data && (
              <div>
                <h2 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">Profit & Loss</h2>
                <div className="grid grid-cols-2 gap-4 mb-6">
                  <div className="bg-blue-50 dark:bg-blue-600/10 p-4 rounded-lg">
                    <p className="text-sm text-gray-600 dark:text-slate-400">Revenue</p>
                    <p className="text-2xl font-bold text-blue-600 dark:text-blue-400">₹{data.totalRevenue?.toFixed(2)}</p>
                  </div>
                  <div className="bg-red-50 dark:bg-red-600/10 p-4 rounded-lg">
                    <p className="text-sm text-gray-600 dark:text-slate-400">Expenses</p>
                    <p className="text-2xl font-bold text-red-600 dark:text-red-400">₹{data.totalExpenses?.toFixed(2)}</p>
                  </div>
                  <div className="bg-green-50 dark:bg-green-600/10 p-4 rounded-lg col-span-2">
                    <p className="text-sm text-gray-600 dark:text-slate-400">Net Profit</p>
                    <p className={`text-2xl font-bold ${data.netProfit >= 0 ? 'text-green-600 dark:text-green-400' : 'text-red-600 dark:text-red-400'}`}>
                      ₹{data.netProfit?.toFixed(2)}
                    </p>
                  </div>
                </div>
              </div>
            )}

            {report === 'tax' && data?.byPeriod && (
              <div>
                <h2 className="text-lg font-semibold text-gray-900 dark:text-white mb-4">Tax Collected</h2>
                <div className="bg-blue-50 dark:bg-blue-600/10 p-4 rounded-lg mb-6">
                  <p className="text-sm text-gray-600 dark:text-slate-400">Total Tax</p>
                  <p className="text-3xl font-bold text-gray-900 dark:text-white">₹{data.totalTax?.toFixed(2)}</p>
                </div>
                <div className="overflow-x-auto">
                  <table className="w-full text-sm">
                    <thead>
                      <tr className="border-b border-gray-200 dark:border-slate-700 bg-gray-50 dark:bg-slate-800/50">
                        <th className="px-4 py-2 text-left">Period</th>
                        <th className="px-4 py-2 text-right">CGST</th>
                        <th className="px-4 py-2 text-right">SGST</th>
                        <th className="px-4 py-2 text-right">IGST</th>
                        <th className="px-4 py-2 text-right">Total</th>
                      </tr>
                    </thead>
                    <tbody>
                      {data.byPeriod?.map((p: any, i: number) => (
                        <tr key={i} className="border-b border-gray-200 dark:border-slate-700">
                          <td className="px-4 py-2">{p.period}</td>
                          <td className="px-4 py-2 text-right">₹{p.cgst?.toFixed(2)}</td>
                          <td className="px-4 py-2 text-right">₹{p.sgst?.toFixed(2)}</td>
                          <td className="px-4 py-2 text-right">₹{p.igst?.toFixed(2)}</td>
                          <td className="px-4 py-2 text-right font-medium">₹{p.total?.toFixed(2)}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>
            )}

            {!data && !loading && (
              <div className="text-center text-gray-500 dark:text-slate-400 py-8">
                Select report type and date range, then click "Generate Report"
              </div>
            )}
          </CardContent>
        </Card>
      )}
    </div>
  );
}
