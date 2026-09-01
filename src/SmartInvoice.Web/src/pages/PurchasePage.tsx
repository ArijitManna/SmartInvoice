import { useState } from 'react';
import { useForm } from 'react-hook-form';
import toast from 'react-hot-toast';
import { Plus, Pencil, Trash2, X } from 'lucide-react';
import { Button, Input, Card, CardContent } from '../components/ui';
import {
  useGetPurchaseOrdersQuery,
  useGetPurchaseBillsQuery,
  useCreatePurchaseOrderMutation,
  useUpdatePurchaseOrderMutation,
  useDeletePurchaseOrderMutation,
  useCreatePurchaseBillMutation,
  useUpdatePurchaseBillMutation,
  useDeletePurchaseBillMutation,
  type PurchaseOrderRequest,
  type PurchaseBillRequest,
} from '../store/purchaseApi';
import { useGetVendorsQuery } from '../store/vendorApi';

export default function PurchasePage() {
  const [tab, setTab] = useState<'orders' | 'bills'>('orders');
  const [page, setPage] = useState(1);
  const [showForm, setShowForm] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);

  const { data: vendors } = useGetVendorsQuery({});
  const { data: orders, isLoading: ordersLoading } = useGetPurchaseOrdersQuery({ page, pageSize: 15 });
  const { data: bills, isLoading: billsLoading } = useGetPurchaseBillsQuery({ page, pageSize: 15 });

  const [createOrder, { isLoading: creatingOrder }] = useCreatePurchaseOrderMutation();
  const [updateOrder, { isLoading: updatingOrder }] = useUpdatePurchaseOrderMutation();
  const [deleteOrder] = useDeletePurchaseOrderMutation();
  const [createBill, { isLoading: creatingBill }] = useCreatePurchaseBillMutation();
  const [updateBill, { isLoading: updatingBill }] = useUpdatePurchaseBillMutation();
  const [deleteBill] = useDeletePurchaseBillMutation();

  const { register, handleSubmit, reset } = useForm();

  const handleCreate = async (data: any) => {
    try {
      if (tab === 'orders') {
        await createOrder(data as PurchaseOrderRequest).unwrap();
        toast.success('Purchase Order created');
      } else {
        await createBill(data as PurchaseBillRequest).unwrap();
        toast.success('Purchase Bill created');
      }
      setShowForm(false);
      reset();
    } catch {
      toast.error('Operation failed');
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm('Delete this record?')) return;
    try {
      if (tab === 'orders') {
        await deleteOrder(id).unwrap();
      } else {
        await deleteBill(id).unwrap();
      }
      toast.success('Deleted');
    } catch {
      toast.error('Delete failed');
    }
  };

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Purchase Management</h1>
        <Button onClick={() => { setEditingId(null); reset(); setShowForm(true); }}>
          <Plus className="h-4 w-4 mr-2" />Add {tab === 'orders' ? 'Order' : 'Bill'}
        </Button>
      </div>

      {/* Tabs */}
      <div className="flex gap-4 mb-6 border-b border-gray-200 dark:border-slate-700">
        {(['orders', 'bills'] as const).map((t) => (
          <button
            key={t}
            onClick={() => { setTab(t); setPage(1); }}
            className={`px-4 py-2 font-medium border-b-2 transition ${
              tab === t
                ? 'text-blue-600 border-blue-600 dark:text-blue-400 dark:border-blue-400'
                : 'text-gray-600 border-transparent dark:text-slate-400 hover:text-gray-900 dark:hover:text-white'
            }`}
          >
            {t === 'orders' ? 'Purchase Orders' : 'Bills'}
          </button>
        ))}
      </div>

      {/* Form Modal */}
      {showForm && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <Card className="w-full max-w-md max-h-[90vh] overflow-y-auto">
            <div className="flex items-center justify-between p-6 border-b border-gray-200 dark:border-slate-700">
              <h2 className="text-lg font-semibold text-gray-900 dark:text-white">
                {tab === 'orders' ? 'Purchase Order' : 'Purchase Bill'}
              </h2>
              <button onClick={() => setShowForm(false)} className="text-gray-500 hover:text-gray-700 dark:hover:text-slate-300">
                <X className="h-5 w-5" />
              </button>
            </div>
            <CardContent className="p-6">
              <form onSubmit={handleSubmit(handleCreate)} className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-slate-300 mb-1">Vendor</label>
                  <select
                    {...register('vendorId', { required: true })}
                    className="w-full rounded-lg border border-gray-300 dark:border-slate-600 dark:bg-[#1a2236] dark:text-white px-3 py-2 text-sm"
                  >
                    <option value="">Select Vendor</option>
                    {vendors?.items?.map((v) => <option key={v.id} value={v.id}>{v.name}</option>)}
                  </select>
                </div>

                {tab === 'orders' ? (
                  <>
                    <Input label="Order Date" type="date" {...register('orderDate', { required: true })} />
                    <Input label="Expected Delivery Date" type="date" {...register('expectedDeliveryDate')} />
                  </>
                ) : (
                  <>
                    <Input label="Bill Number" {...register('billNumber', { required: true })} />
                    <Input label="Bill Date" type="date" {...register('billDate', { required: true })} />
                    <Input label="Due Date" type="date" {...register('dueDate')} />
                    <Input label="Amount" type="number" step="0.01" {...register('amount', { valueAsNumber: true })} />
                  </>
                )}

                <textarea
                  {...register('notes')}
                  placeholder="Notes"
                  className="w-full rounded-lg border border-gray-300 dark:border-slate-600 dark:bg-[#1a2236] dark:text-white px-3 py-2 text-sm resize-none h-16"
                />

                <div className="flex justify-end gap-3">
                  <Button type="button" variant="ghost" onClick={() => setShowForm(false)}>Cancel</Button>
                  <Button type="submit" loading={creatingOrder || creatingBill || updatingOrder || updatingBill}>Save</Button>
                </div>
              </form>
            </CardContent>
          </Card>
        </div>
      )}

      {/* Table */}
      <Card>
        <CardContent className="p-0">
          {(tab === 'orders' ? ordersLoading : billsLoading) ? (
            <div className="p-6 text-center text-gray-500 dark:text-slate-400">Loading...</div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-gray-200 dark:border-slate-700 bg-gray-50 dark:bg-slate-800/50">
                    {tab === 'orders' ? (
                      <>
                        <th className="px-6 py-3 text-left font-medium text-gray-600 dark:text-slate-300">Order #</th>
                        <th className="px-6 py-3 text-left font-medium text-gray-600 dark:text-slate-300">Vendor</th>
                        <th className="px-6 py-3 text-left font-medium text-gray-600 dark:text-slate-300">Date</th>
                        <th className="px-6 py-3 text-right font-medium text-gray-600 dark:text-slate-300">Amount</th>
                        <th className="px-6 py-3 text-left font-medium text-gray-600 dark:text-slate-300">Status</th>
                        <th className="px-6 py-3 text-center font-medium text-gray-600 dark:text-slate-300">Actions</th>
                      </>
                    ) : (
                      <>
                        <th className="px-6 py-3 text-left font-medium text-gray-600 dark:text-slate-300">Bill #</th>
                        <th className="px-6 py-3 text-left font-medium text-gray-600 dark:text-slate-300">Vendor</th>
                        <th className="px-6 py-3 text-left font-medium text-gray-600 dark:text-slate-300">Date</th>
                        <th className="px-6 py-3 text-right font-medium text-gray-600 dark:text-slate-300">Amount</th>
                        <th className="px-6 py-3 text-left font-medium text-gray-600 dark:text-slate-300">Status</th>
                        <th className="px-6 py-3 text-center font-medium text-gray-600 dark:text-slate-300">Actions</th>
                      </>
                    )}
                  </tr>
                </thead>
                <tbody>
                  {tab === 'orders'
                    ? orders?.items?.map((o) => (
                        <tr key={o.id} className="border-b border-gray-200 dark:border-slate-700">
                          <td className="px-6 py-4 text-gray-900 dark:text-white font-medium">{o.orderNumber}</td>
                          <td className="px-6 py-4 text-gray-600 dark:text-slate-400">{o.vendorName}</td>
                          <td className="px-6 py-4 text-gray-600 dark:text-slate-400">{new Date(o.orderDate).toLocaleDateString()}</td>
                          <td className="px-6 py-4 text-right font-medium text-gray-900 dark:text-white">₹{o.totalAmount?.toFixed(2) ?? '0.00'}</td>
                          <td className="px-6 py-4 text-gray-600 dark:text-slate-400">{o.status}</td>
                          <td className="px-6 py-4 text-center">
                            <button
                              onClick={() => handleDelete(o.id)}
                              className="p-2 text-red-600 hover:bg-red-50 dark:hover:bg-red-600/20 rounded"
                            >
                              <Trash2 className="h-4 w-4" />
                            </button>
                          </td>
                        </tr>
                      ))
                    : bills?.items?.map((b) => (
                        <tr key={b.id} className="border-b border-gray-200 dark:border-slate-700">
                          <td className="px-6 py-4 text-gray-900 dark:text-white font-medium">{b.billNumber}</td>
                          <td className="px-6 py-4 text-gray-600 dark:text-slate-400">{b.vendorName}</td>
                          <td className="px-6 py-4 text-gray-600 dark:text-slate-400">{new Date(b.billDate).toLocaleDateString()}</td>
                          <td className="px-6 py-4 text-right font-medium text-gray-900 dark:text-white">₹{b.amount?.toFixed(2)}</td>
                          <td className="px-6 py-4 text-gray-600 dark:text-slate-400">{b.status}</td>
                          <td className="px-6 py-4 text-center">
                            <button
                              onClick={() => handleDelete(b.id)}
                              className="p-2 text-red-600 hover:bg-red-50 dark:hover:bg-red-600/20 rounded"
                            >
                              <Trash2 className="h-4 w-4" />
                            </button>
                          </td>
                        </tr>
                      ))}
                </tbody>
              </table>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
