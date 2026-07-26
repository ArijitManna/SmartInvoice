import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import toast from 'react-hot-toast';
import { Plus, Eye, Copy, Trash2 } from 'lucide-react';
import { Button, Card, CardContent, Badge } from '../components/ui';
import { useGetInvoicesQuery, useDuplicateInvoiceMutation, useDeleteInvoiceMutation } from '../store/invoiceApi';

const statusLabels: Record<number, { label: string; variant: 'default' | 'success' | 'warning' | 'danger' | 'info' }> = {
  0: { label: 'Draft', variant: 'default' },
  1: { label: 'Sent', variant: 'info' },
  2: { label: 'Paid', variant: 'success' },
  3: { label: 'Partial', variant: 'warning' },
  4: { label: 'Overdue', variant: 'danger' },
  5: { label: 'Cancelled', variant: 'default' },
};

export default function InvoicesPage() {
  const navigate = useNavigate();
  const [page, setPage] = useState(1);
  const [statusFilter, setStatusFilter] = useState<number | undefined>(undefined);

  const { data, isLoading } = useGetInvoicesQuery({ page, pageSize: 15, status: statusFilter });
  const [duplicateInvoice] = useDuplicateInvoiceMutation();
  const [deleteInvoice] = useDeleteInvoiceMutation();

  const handleDuplicate = async (id: string) => {
    try {
      const result = await duplicateInvoice(id).unwrap();
      toast.success(`Duplicated as ${result.invoiceNumber}`);
      navigate(`/invoices/${result.id}`);
    } catch {
      toast.error('Duplicate failed');
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm('Delete this invoice?')) return;
    try {
      await deleteInvoice(id).unwrap();
      toast.success('Invoice deleted');
    } catch (err: unknown) {
      const error = err as { data?: { error?: string } };
      toast.error(error.data?.error || 'Delete failed');
    }
  };

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Invoices</h1>
        <Button onClick={() => navigate('/invoices/new')}>
          <Plus className="h-4 w-4 mr-2" />New Invoice
        </Button>
      </div>

      {/* Status filter */}
      <div className="flex gap-2 mb-4 flex-wrap">
        <button
          onClick={() => { setStatusFilter(undefined); setPage(1); }}
          className={`px-3 py-1.5 rounded-lg text-sm font-medium transition-colors ${statusFilter === undefined ? 'bg-blue-100 text-blue-700 dark:bg-blue-600/20 dark:text-blue-400' : 'bg-gray-100 text-gray-600 hover:bg-gray-200 dark:bg-slate-800 dark:text-slate-400 dark:hover:bg-slate-700'}`}
        >All</button>
        {Object.entries(statusLabels).map(([key, { label }]) => (
          <button
            key={key}
            onClick={() => { setStatusFilter(Number(key)); setPage(1); }}
            className={`px-3 py-1.5 rounded-lg text-sm font-medium transition-colors ${statusFilter === Number(key) ? 'bg-blue-100 text-blue-700 dark:bg-blue-600/20 dark:text-blue-400' : 'bg-gray-100 text-gray-600 hover:bg-gray-200 dark:bg-slate-800 dark:text-slate-400 dark:hover:bg-slate-700'}`}
          >{label}</button>
        ))}
      </div>

      <Card>
        <CardContent className="p-0">
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead className="bg-gray-50 dark:bg-slate-800/50 border-b border-gray-200 dark:border-slate-700">
                <tr>
                  <th className="px-4 py-3 text-left font-medium text-gray-600 dark:text-slate-400">Invoice #</th>
                  <th className="px-4 py-3 text-left font-medium text-gray-600 dark:text-slate-400">Customer</th>
                  <th className="px-4 py-3 text-left font-medium text-gray-600 dark:text-slate-400">Date</th>
                  <th className="px-4 py-3 text-left font-medium text-gray-600 dark:text-slate-400">Due Date</th>
                  <th className="px-4 py-3 text-left font-medium text-gray-600 dark:text-slate-400">Status</th>
                  <th className="px-4 py-3 text-right font-medium text-gray-600 dark:text-slate-400">Amount</th>
                  <th className="px-4 py-3 text-right font-medium text-gray-600 dark:text-slate-400">Balance</th>
                  <th className="px-4 py-3 text-right font-medium text-gray-600 dark:text-slate-400">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100 dark:divide-slate-700/50">
                {isLoading && <tr><td colSpan={8} className="px-4 py-8 text-center text-gray-400 dark:text-slate-500">Loading...</td></tr>}
                {data?.items.map((inv) => {
                  const st = statusLabels[inv.status] ?? { label: 'Unknown', variant: 'default' as const };
                  return (
                    <tr key={inv.id} className="hover:bg-gray-50 dark:hover:bg-slate-800/50">
                      <td className="px-4 py-3">
                        <Link to={`/invoices/${inv.id}`} className="font-medium text-blue-600 dark:text-blue-400 hover:underline">{inv.invoiceNumber}</Link>
                      </td>
                      <td className="px-4 py-3 text-gray-900 dark:text-white">{inv.customerName}</td>
                      <td className="px-4 py-3 text-gray-600 dark:text-slate-300">{new Date(inv.invoiceDate).toLocaleDateString()}</td>
                      <td className="px-4 py-3 text-gray-600 dark:text-slate-300">{inv.dueDate ? new Date(inv.dueDate).toLocaleDateString() : '-'}</td>
                      <td className="px-4 py-3"><Badge variant={st.variant}>{st.label}</Badge></td>
                      <td className="px-4 py-3 text-right font-medium text-gray-900 dark:text-white">₹{inv.totalAmount.toFixed(2)}</td>
                      <td className="px-4 py-3 text-right text-gray-600 dark:text-slate-300">₹{inv.balanceDue.toFixed(2)}</td>
                      <td className="px-4 py-3 text-right whitespace-nowrap">
                        <Link to={`/invoices/${inv.id}`} className="p-1 inline-block text-gray-400 hover:text-blue-600 dark:text-slate-500 dark:hover:text-blue-400"><Eye className="h-4 w-4" /></Link>
                        <button onClick={() => handleDuplicate(inv.id)} className="p-1 text-gray-400 hover:text-green-600 dark:text-slate-500 dark:hover:text-green-400"><Copy className="h-4 w-4" /></button>
                        <button onClick={() => handleDelete(inv.id)} className="p-1 text-gray-400 hover:text-red-600 dark:text-slate-500 dark:hover:text-red-400"><Trash2 className="h-4 w-4" /></button>
                      </td>
                    </tr>
                  );
                })}
                {data && data.items.length === 0 && (
                  <tr><td colSpan={8} className="px-4 py-8 text-center text-gray-400 dark:text-slate-500">No invoices found</td></tr>
                )}
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>

      {data && data.totalPages > 1 && (
        <div className="flex items-center justify-between mt-4">
          <p className="text-sm text-gray-600">Showing {data.items.length} of {data.totalCount}</p>
          <div className="flex gap-2">
            <Button size="sm" variant="secondary" disabled={!data.hasPreviousPage} onClick={() => setPage(page - 1)}>Previous</Button>
            <Button size="sm" variant="secondary" disabled={!data.hasNextPage} onClick={() => setPage(page + 1)}>Next</Button>
          </div>
        </div>
      )}
    </div>
  );
}
