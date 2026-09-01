import { useState } from 'react';
import { useForm } from 'react-hook-form';
import toast from 'react-hot-toast';
import { Plus, Search, Pencil, Trash2, X } from 'lucide-react';
import { Button, Input, Card, CardContent } from '../components/ui';
import {
  useGetExpensesQuery,
  useGetExpenseCategoriesQuery,
  useCreateExpenseMutation,
  useUpdateExpenseMutation,
  useDeleteExpenseMutation,
  type ExpenseRequest,
  type ExpenseResponse,
} from '../store/expenseApi';

export default function ExpensesPage() {
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');
  const [showForm, setShowForm] = useState(false);
  const [editingExpense, setEditingExpense] = useState<ExpenseResponse | null>(null);

  const { data: categories } = useGetExpenseCategoriesQuery();
  const { data, isLoading } = useGetExpensesQuery({ page, pageSize: 20, from: undefined, to: undefined });
  const [createExpense, { isLoading: creating }] = useCreateExpenseMutation();
  const [updateExpense, { isLoading: updating }] = useUpdateExpenseMutation();
  const [deleteExpense] = useDeleteExpenseMutation();

  const { register, handleSubmit, reset, watch } = useForm<ExpenseRequest>();
  const categoryId = watch('categoryId');

  const openCreate = () => {
    setEditingExpense(null);
    reset({ categoryId: '', amount: 0, date: new Date().toISOString().split('T')[0] });
    setShowForm(true);
  };

  const openEdit = (e: ExpenseResponse) => {
    setEditingExpense(e);
    reset({
      categoryId: e.categoryId,
      amount: e.amount,
      description: e.description ?? '',
      date: e.date,
      paymentMethod: e.paymentMethod ?? '',
    });
    setShowForm(true);
  };

  const onSubmit = async (data: ExpenseRequest) => {
    try {
      if (editingExpense) {
        await updateExpense({ id: editingExpense.id, data }).unwrap();
        toast.success('Expense updated');
      } else {
        await createExpense(data).unwrap();
        toast.success('Expense created');
      }
      setShowForm(false);
    } catch {
      toast.error('Operation failed');
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm('Delete this expense?')) return;
    try {
      await deleteExpense(id).unwrap();
      toast.success('Expense deleted');
    } catch {
      toast.error('Delete failed');
    }
  };

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Expenses</h1>
        <Button onClick={openCreate}><Plus className="h-4 w-4 mr-2" />Add Expense</Button>
      </div>

      {/* Form Modal */}
      {showForm && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <Card className="w-full max-w-md max-h-[90vh] overflow-y-auto">
            <div className="flex items-center justify-between p-6 border-b border-gray-200 dark:border-slate-700">
              <h2 className="text-lg font-semibold text-gray-900 dark:text-white">
                {editingExpense ? 'Edit Expense' : 'New Expense'}
              </h2>
              <button onClick={() => setShowForm(false)} className="text-gray-500 hover:text-gray-700 dark:hover:text-slate-300">
                <X className="h-5 w-5" />
              </button>
            </div>
            <CardContent className="p-6">
              <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-slate-300 mb-1">Category</label>
                  <select
                    {...register('categoryId', { required: true })}
                    className="w-full rounded-lg border border-gray-300 dark:border-slate-600 dark:bg-[#1a2236] dark:text-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                  >
                    <option value="">Select Category</option>
                    {categories?.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
                  </select>
                </div>

                <Input
                  label="Amount"
                  type="number"
                  step="0.01"
                  {...register('amount', { required: true, valueAsNumber: true })}
                />

                <Input
                  label="Date"
                  type="date"
                  {...register('date', { required: true })}
                />

                <Input
                  label="Payment Method"
                  {...register('paymentMethod')}
                />

                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-slate-300 mb-1">Description</label>
                  <textarea
                    {...register('description')}
                    className="w-full rounded-lg border border-gray-300 dark:border-slate-600 dark:bg-[#1a2236] dark:text-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 resize-none h-16"
                  />
                </div>

                <div className="flex justify-end gap-3 pt-4">
                  <Button type="button" variant="ghost" onClick={() => setShowForm(false)}>Cancel</Button>
                  <Button type="submit" loading={creating || updating}>Save Expense</Button>
                </div>
              </form>
            </CardContent>
          </Card>
        </div>
      )}

      {/* Table */}
      <Card>
        <CardContent className="p-0">
          {isLoading ? (
            <div className="p-6 text-center text-gray-500 dark:text-slate-400">Loading expenses...</div>
          ) : (data?.items?.length ?? 0) === 0 ? (
            <div className="p-6 text-center text-gray-500 dark:text-slate-400">No expenses found</div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-gray-200 dark:border-slate-700 bg-gray-50 dark:bg-slate-800/50">
                    <th className="px-6 py-3 text-left font-medium text-gray-600 dark:text-slate-300">Date</th>
                    <th className="px-6 py-3 text-left font-medium text-gray-600 dark:text-slate-300">Category</th>
                    <th className="px-6 py-3 text-left font-medium text-gray-600 dark:text-slate-300">Description</th>
                    <th className="px-6 py-3 text-right font-medium text-gray-600 dark:text-slate-300">Amount</th>
                    <th className="px-6 py-3 text-left font-medium text-gray-600 dark:text-slate-300">Payment</th>
                    <th className="px-6 py-3 text-center font-medium text-gray-600 dark:text-slate-300">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {data?.items?.map((expense) => (
                    <tr key={expense.id} className="border-b border-gray-200 dark:border-slate-700 hover:bg-gray-50 dark:hover:bg-slate-800/50 transition">
                      <td className="px-6 py-4 text-gray-600 dark:text-slate-400">{new Date(expense.date).toLocaleDateString()}</td>
                      <td className="px-6 py-4 text-gray-900 dark:text-white font-medium">{expense.categoryName}</td>
                      <td className="px-6 py-4 text-gray-600 dark:text-slate-400">{expense.description ?? '-'}</td>
                      <td className="px-6 py-4 text-right font-medium text-gray-900 dark:text-white">₹{expense.amount?.toFixed(2)}</td>
                      <td className="px-6 py-4 text-gray-600 dark:text-slate-400">{expense.paymentMethod ?? '-'}</td>
                      <td className="px-6 py-4 text-center">
                        <div className="flex justify-center gap-2">
                          <button
                            onClick={() => openEdit(expense)}
                            className="p-2 text-blue-600 hover:bg-blue-50 dark:hover:bg-blue-600/20 rounded transition"
                          >
                            <Pencil className="h-4 w-4" />
                          </button>
                          <button
                            onClick={() => handleDelete(expense.id)}
                            className="p-2 text-red-600 hover:bg-red-50 dark:hover:bg-red-600/20 rounded transition"
                          >
                            <Trash2 className="h-4 w-4" />
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Pagination */}
      {(data?.total ?? 0) > 20 && (
        <div className="mt-6 flex justify-center gap-2">
          <Button
            variant="ghost"
            onClick={() => setPage((p) => Math.max(p - 1, 1))}
            disabled={page === 1}
          >
            Previous
          </Button>
          <span className="px-4 py-2 text-gray-600 dark:text-slate-400">
            Page {page} of {Math.ceil((data?.total ?? 1) / 20)}
          </span>
          <Button
            variant="ghost"
            onClick={() => setPage((p) => p + 1)}
            disabled={page * 20 >= (data?.total ?? 0)}
          >
            Next
          </Button>
        </div>
      )}
    </div>
  );
}
