import { useState } from 'react';
import { Card, CardContent, Badge } from '../components/ui';
import { useGetSalesReportQuery, useGetGstReportQuery, useGetOutstandingReportQuery } from '../store/dashboardApi';

type ReportTab = 'sales' | 'gst' | 'outstanding';

function getDefaultDates() {
  const now = new Date();
  const year = now.getFullYear();
  const month = String(now.getMonth() + 1).padStart(2, '0');
  const day = String(now.getDate()).padStart(2, '0');
  const from = `${year}-${month}-01`;
  const to = `${year}-${month}-${day}`;
  return { from, to };
}

export default function ReportsPage() {
  const [tab, setTab] = useState<ReportTab>('sales');
  const defaults = getDefaultDates();
  const [from, setFrom] = useState(defaults.from);
  const [to, setTo] = useState(defaults.to);

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-900 dark:text-white mb-6">Reports</h1>

      {/* Tab selector */}
      <div className="flex gap-2 mb-6 border-b border-gray-200 dark:border-slate-700">
        {(['sales', 'gst', 'outstanding'] as ReportTab[]).map((t) => (
          <button key={t} onClick={() => setTab(t)}
            className={`px-4 py-2 text-sm font-medium border-b-2 transition-colors -mb-px ${tab === t ? 'border-blue-600 text-blue-600 dark:border-blue-400 dark:text-blue-400' : 'border-transparent text-gray-500 dark:text-slate-400 hover:text-gray-700 dark:hover:text-white'}`}>
            {t === 'sales' ? 'Sales Report' : t === 'gst' ? 'GST Report' : 'Outstanding'}
          </button>
        ))}
      </div>

      {/* Date filters (not for outstanding) */}
      {tab !== 'outstanding' && (
        <div className="flex gap-4 mb-4 items-end">
          <div>
            <label className="block text-xs font-medium text-gray-600 dark:text-slate-400 mb-1">From</label>
            <input type="date" value={from} onChange={(e) => setFrom(e.target.value)}
              className="rounded-lg border border-gray-300 dark:border-slate-600 dark:bg-[#1a2236] dark:text-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
          </div>
          <div>
            <label className="block text-xs font-medium text-gray-600 dark:text-slate-400 mb-1">To</label>
            <input type="date" value={to} onChange={(e) => setTo(e.target.value)}
              className="rounded-lg border border-gray-300 dark:border-slate-600 dark:bg-[#1a2236] dark:text-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
          </div>
        </div>
      )}

      {tab === 'sales' && <SalesReport from={from} to={to} />}
      {tab === 'gst' && <GstReport from={from} to={to} />}
      {tab === 'outstanding' && <OutstandingReport />}
    </div>
  );
}

function SalesReport({ from, to }: { from: string; to: string }) {
  const { data, isLoading } = useGetSalesReportQuery({ from, to });

  if (isLoading) return <p className="text-gray-400">Loading...</p>;
  if (!data) return null;

  return (
    <>
      <div className="grid grid-cols-3 gap-4 mb-6">
        <Card><CardContent className="p-4"><p className="text-xs text-gray-500 dark:text-slate-500">Total Sales</p><p className="text-xl font-bold dark:text-white">₹{data.totalSales.toFixed(2)}</p></CardContent></Card>
        <Card><CardContent className="p-4"><p className="text-xs text-gray-500 dark:text-slate-500">Invoices</p><p className="text-xl font-bold dark:text-white">{data.totalInvoices}</p></CardContent></Card>
        <Card><CardContent className="p-4"><p className="text-xs text-gray-500 dark:text-slate-500">Total Tax</p><p className="text-xl font-bold dark:text-white">₹{data.totalTax.toFixed(2)}</p></CardContent></Card>
      </div>
      <Card>
        <CardContent className="p-0">
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead className="bg-gray-50 dark:bg-slate-800/50 border-b dark:border-slate-700">
                <tr>
                  <th className="px-4 py-3 text-left font-medium text-gray-600 dark:text-slate-400">Invoice #</th>
                  <th className="px-4 py-3 text-left font-medium text-gray-600 dark:text-slate-400">Date</th>
                  <th className="px-4 py-3 text-left font-medium text-gray-600 dark:text-slate-400">Customer</th>
                  <th className="px-4 py-3 text-right font-medium text-gray-600 dark:text-slate-400">Sub Total</th>
                  <th className="px-4 py-3 text-right font-medium text-gray-600 dark:text-slate-400">Tax</th>
                  <th className="px-4 py-3 text-right font-medium text-gray-600 dark:text-slate-400">Total</th>
                  <th className="px-4 py-3 text-left font-medium text-gray-600 dark:text-slate-400">Status</th>
                </tr>
              </thead>
              <tbody className="divide-y dark:divide-slate-700/50">
                {data.items.map((i) => (
                  <tr key={i.invoiceId} className="hover:bg-gray-50 dark:hover:bg-slate-800/50">
                    <td className="px-4 py-2 font-medium dark:text-white">{i.invoiceNumber}</td>
                    <td className="px-4 py-2 text-gray-600 dark:text-slate-300">{new Date(i.invoiceDate).toLocaleDateString()}</td>
                    <td className="px-4 py-2 dark:text-slate-300">{i.customerName}</td>
                    <td className="px-4 py-2 text-right dark:text-slate-300">₹{i.subTotal.toFixed(2)}</td>
                    <td className="px-4 py-2 text-right dark:text-slate-300">₹{i.taxAmount.toFixed(2)}</td>
                    <td className="px-4 py-2 text-right font-medium dark:text-white">₹{i.totalAmount.toFixed(2)}</td>
                    <td className="px-4 py-2"><Badge variant="info">{i.status}</Badge></td>
                  </tr>
                ))}
                {data.items.length === 0 && <tr><td colSpan={7} className="px-4 py-8 text-center text-gray-400 dark:text-slate-500">No data for this period</td></tr>}
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>
    </>
  );
}

function GstReport({ from, to }: { from: string; to: string }) {
  const { data, isLoading } = useGetGstReportQuery({ from, to });

  if (isLoading) return <p className="text-gray-400">Loading...</p>;
  if (!data) return null;

  return (
    <>
      <div className="grid grid-cols-4 gap-4 mb-6">
        <Card><CardContent className="p-4"><p className="text-xs text-gray-500 dark:text-slate-500">Total Tax</p><p className="text-xl font-bold dark:text-white">₹{data.totalTaxCollected.toFixed(2)}</p></CardContent></Card>
        <Card><CardContent className="p-4"><p className="text-xs text-gray-500 dark:text-slate-500">CGST</p><p className="text-xl font-bold dark:text-white">₹{data.totalCgst.toFixed(2)}</p></CardContent></Card>
        <Card><CardContent className="p-4"><p className="text-xs text-gray-500 dark:text-slate-500">SGST</p><p className="text-xl font-bold dark:text-white">₹{data.totalSgst.toFixed(2)}</p></CardContent></Card>
        <Card><CardContent className="p-4"><p className="text-xs text-gray-500 dark:text-slate-500">IGST</p><p className="text-xl font-bold dark:text-white">₹{data.totalIgst.toFixed(2)}</p></CardContent></Card>
      </div>
      <Card>
        <CardContent className="p-0">
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead className="bg-gray-50 dark:bg-slate-800/50 border-b dark:border-slate-700">
                <tr>
                  <th className="px-4 py-3 text-left font-medium text-gray-600 dark:text-slate-400">Invoice #</th>
                  <th className="px-4 py-3 text-left font-medium text-gray-600 dark:text-slate-400">Date</th>
                  <th className="px-4 py-3 text-left font-medium text-gray-600 dark:text-slate-400">Customer</th>
                  <th className="px-4 py-3 text-left font-medium text-gray-600 dark:text-slate-400">GSTIN</th>
                  <th className="px-4 py-3 text-right font-medium text-gray-600 dark:text-slate-400">Taxable</th>
                  <th className="px-4 py-3 text-right font-medium text-gray-600 dark:text-slate-400">CGST</th>
                  <th className="px-4 py-3 text-right font-medium text-gray-600 dark:text-slate-400">SGST</th>
                  <th className="px-4 py-3 text-right font-medium text-gray-600 dark:text-slate-400">IGST</th>
                  <th className="px-4 py-3 text-right font-medium text-gray-600 dark:text-slate-400">Total Tax</th>
                </tr>
              </thead>
              <tbody className="divide-y dark:divide-slate-700/50">
                {data.items.map((i) => (
                  <tr key={i.invoiceId} className="hover:bg-gray-50 dark:hover:bg-slate-800/50">
                    <td className="px-4 py-2 font-medium dark:text-white">{i.invoiceNumber}</td>
                    <td className="px-4 py-2 text-gray-600 dark:text-slate-300">{new Date(i.invoiceDate).toLocaleDateString()}</td>
                    <td className="px-4 py-2 dark:text-slate-300">{i.customerName}</td>
                    <td className="px-4 py-2 font-mono text-xs dark:text-slate-400">{i.customerGstin || '-'}</td>
                    <td className="px-4 py-2 text-right dark:text-slate-300">₹{i.taxableAmount.toFixed(2)}</td>
                    <td className="px-4 py-2 text-right dark:text-slate-300">₹{i.cgstAmount.toFixed(2)}</td>
                    <td className="px-4 py-2 text-right dark:text-slate-300">₹{i.sgstAmount.toFixed(2)}</td>
                    <td className="px-4 py-2 text-right dark:text-slate-300">₹{i.igstAmount.toFixed(2)}</td>
                    <td className="px-4 py-2 text-right font-medium dark:text-white">₹{i.totalTax.toFixed(2)}</td>
                  </tr>
                ))}
                {data.items.length === 0 && <tr><td colSpan={9} className="px-4 py-8 text-center text-gray-400 dark:text-slate-500">No data for this period</td></tr>}
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>
    </>
  );
}

function OutstandingReport() {
  const { data, isLoading } = useGetOutstandingReportQuery();

  if (isLoading) return <p className="text-gray-400">Loading...</p>;
  if (!data) return null;

  return (
    <>
      <div className="grid grid-cols-2 gap-4 mb-6">
        <Card><CardContent className="p-4"><p className="text-xs text-gray-500 dark:text-slate-500">Total Outstanding</p><p className="text-xl font-bold text-red-600 dark:text-red-400">₹{data.totalOutstanding.toFixed(2)}</p></CardContent></Card>
        <Card><CardContent className="p-4"><p className="text-xs text-gray-500 dark:text-slate-500">Unpaid Invoices</p><p className="text-xl font-bold dark:text-white">{data.totalInvoices}</p></CardContent></Card>
      </div>
      <Card>
        <CardContent className="p-0">
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead className="bg-gray-50 dark:bg-slate-800/50 border-b dark:border-slate-700">
                <tr>
                  <th className="px-4 py-3 text-left font-medium text-gray-600 dark:text-slate-400">Invoice #</th>
                  <th className="px-4 py-3 text-left font-medium text-gray-600 dark:text-slate-400">Customer</th>
                  <th className="px-4 py-3 text-left font-medium text-gray-600 dark:text-slate-400">Due Date</th>
                  <th className="px-4 py-3 text-right font-medium text-gray-600 dark:text-slate-400">Total</th>
                  <th className="px-4 py-3 text-right font-medium text-gray-600 dark:text-slate-400">Paid</th>
                  <th className="px-4 py-3 text-right font-medium text-gray-600 dark:text-slate-400">Balance</th>
                  <th className="px-4 py-3 text-right font-medium text-gray-600 dark:text-slate-400">Overdue</th>
                </tr>
              </thead>
              <tbody className="divide-y dark:divide-slate-700/50">
                {data.items.map((i) => (
                  <tr key={i.invoiceId} className="hover:bg-gray-50 dark:hover:bg-slate-800/50">
                    <td className="px-4 py-2 font-medium dark:text-white">{i.invoiceNumber}</td>
                    <td className="px-4 py-2 dark:text-slate-300">{i.customerName}</td>
                    <td className="px-4 py-2 text-gray-600 dark:text-slate-300">{i.dueDate ? new Date(i.dueDate).toLocaleDateString() : '-'}</td>
                    <td className="px-4 py-2 text-right dark:text-slate-300">₹{i.totalAmount.toFixed(2)}</td>
                    <td className="px-4 py-2 text-right text-green-600 dark:text-green-400">₹{i.amountPaid.toFixed(2)}</td>
                    <td className="px-4 py-2 text-right font-medium text-red-600 dark:text-red-400">₹{i.balanceDue.toFixed(2)}</td>
                    <td className="px-4 py-2 text-right">
                      {i.daysOverdue > 0 ? <Badge variant="danger">{i.daysOverdue}d</Badge> : <span className="text-gray-400 dark:text-slate-500">-</span>}
                    </td>
                  </tr>
                ))}
                {data.items.length === 0 && <tr><td colSpan={7} className="px-4 py-8 text-center text-gray-400 dark:text-slate-500">No outstanding invoices</td></tr>}
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>
    </>
  );
}
