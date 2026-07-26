import { Link } from 'react-router-dom';
import { Card, CardContent, CardHeader, Badge } from '../components/ui';
import { useGetInvoicesQuery } from '../store/invoiceApi';

export default function PaymentsPage() {
  // Show invoices with payments (paid or partially paid)
  const { data: paidInvoices, isLoading: loadingPaid } = useGetInvoicesQuery({ pageSize: 10, status: 2 });
  const { data: partialInvoices, isLoading: loadingPartial } = useGetInvoicesQuery({ pageSize: 10, status: 3 });

  const isLoading = loadingPaid || loadingPartial;
  const allInvoices = [
    ...(paidInvoices?.items || []),
    ...(partialInvoices?.items || []),
  ].sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-900 dark:text-white mb-6">Payments</h1>
      <p className="text-sm text-gray-600 dark:text-slate-400 mb-4">
        To record a payment, open an invoice and click "Record Payment". Below are invoices with payment activity.
      </p>

      <Card>
        <CardHeader><h2 className="font-semibold dark:text-white">Invoices with Payments</h2></CardHeader>
        <CardContent className="p-0">
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead className="bg-gray-50 dark:bg-slate-800/50 border-b border-gray-200 dark:border-slate-700">
                <tr>
                  <th className="px-4 py-3 text-left font-medium text-gray-600 dark:text-slate-400">Invoice #</th>
                  <th className="px-4 py-3 text-left font-medium text-gray-600 dark:text-slate-400">Customer</th>
                  <th className="px-4 py-3 text-left font-medium text-gray-600 dark:text-slate-400">Status</th>
                  <th className="px-4 py-3 text-right font-medium text-gray-600 dark:text-slate-400">Total</th>
                  <th className="px-4 py-3 text-right font-medium text-gray-600 dark:text-slate-400">Balance</th>
                  <th className="px-4 py-3 text-left font-medium text-gray-600 dark:text-slate-400">Date</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100 dark:divide-slate-700/50">
                {isLoading && <tr><td colSpan={6} className="px-4 py-8 text-center text-gray-400 dark:text-slate-500">Loading...</td></tr>}
                {allInvoices.map((inv) => (
                  <tr key={inv.id} className="hover:bg-gray-50 dark:hover:bg-slate-800/50">
                    <td className="px-4 py-3">
                      <Link to={`/invoices/${inv.id}`} className="font-medium text-blue-600 dark:text-blue-400 hover:underline">{inv.invoiceNumber}</Link>
                    </td>
                    <td className="px-4 py-3 text-gray-900 dark:text-white">{inv.customerName}</td>
                    <td className="px-4 py-3">
                      <Badge variant={inv.status === 2 ? 'success' : 'warning'}>
                        {inv.status === 2 ? 'Paid' : 'Partial'}
                      </Badge>
                    </td>
                    <td className="px-4 py-3 text-right font-medium dark:text-white">₹{inv.totalAmount.toFixed(2)}</td>
                    <td className="px-4 py-3 text-right text-gray-600 dark:text-slate-300">₹{inv.balanceDue.toFixed(2)}</td>
                    <td className="px-4 py-3 text-gray-600 dark:text-slate-300">{new Date(inv.invoiceDate).toLocaleDateString()}</td>
                  </tr>
                ))}
                {!isLoading && allInvoices.length === 0 && (
                  <tr><td colSpan={6} className="px-4 py-8 text-center text-gray-400 dark:text-slate-500">No payment activity yet. Record payments from invoice detail pages.</td></tr>
                )}
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
