import { useState } from 'react';
import { useForm } from 'react-hook-form';
import toast from 'react-hot-toast';
import { Plus, Search, Pencil, Trash2, X } from 'lucide-react';
import { Button, Input, Card, CardContent } from '../components/ui';
import {
  useGetCustomersQuery,
  useCreateCustomerMutation,
  useUpdateCustomerMutation,
  useDeleteCustomerMutation,
  type CustomerRequest,
  type CustomerResponse,
} from '../store/customerApi';
import { stateNames, indianStates } from '../data/indianLocations';

export default function CustomersPage() {
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');
  const [showForm, setShowForm] = useState(false);
  const [editingCustomer, setEditingCustomer] = useState<CustomerResponse | null>(null);

  const { data, isLoading } = useGetCustomersQuery({ page, pageSize: 15, search: search || undefined });
  const [createCustomer, { isLoading: creating }] = useCreateCustomerMutation();
  const [updateCustomer, { isLoading: updating }] = useUpdateCustomerMutation();
  const [deleteCustomer] = useDeleteCustomerMutation();

  const { register, handleSubmit, reset, watch, setValue } = useForm<CustomerRequest>();
  const billingState = watch('billingState');

  const openCreate = () => {
    setEditingCustomer(null);
    reset({ name: '', email: '', phone: '', billingCountry: 'India' });
    setShowForm(true);
  };

  const openEdit = (c: CustomerResponse) => {
    setEditingCustomer(c);
    reset({
      name: c.name, email: c.email ?? '', phone: c.phone ?? '',
      contactPerson: c.contactPerson ?? '', notes: c.notes ?? '',
      gstin: c.gstin ?? '', pan: c.pan ?? '', gstStateCode: c.gstStateCode ?? '',
      billingStreet: c.billingStreet ?? '', billingCity: c.billingCity ?? '',
      billingState: c.billingState ?? '', billingPostalCode: c.billingPostalCode ?? '',
      billingCountry: c.billingCountry ?? 'India',
    });
    setShowForm(true);
  };

  const onSubmit = async (data: CustomerRequest) => {
    try {
      if (editingCustomer) {
        await updateCustomer({ id: editingCustomer.id, data }).unwrap();
        toast.success('Customer updated');
      } else {
        await createCustomer(data).unwrap();
        toast.success('Customer created');
      }
      setShowForm(false);
    } catch {
      toast.error('Operation failed');
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm('Delete this customer?')) return;
    try {
      await deleteCustomer(id).unwrap();
      toast.success('Customer deleted');
    } catch {
      toast.error('Delete failed');
    }
  };

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Customers</h1>
        <Button onClick={openCreate}><Plus className="h-4 w-4 mr-2" />Add Customer</Button>
      </div>

      {/* Search */}
      <div className="mb-4 max-w-sm">
        <div className="relative">
          <Search className="absolute left-3 top-2.5 h-4 w-4 text-gray-400" />
          <input
            type="text"
            placeholder="Search customers..."
            value={search}
            onChange={(e) => { setSearch(e.target.value); setPage(1); }}
            className="w-full rounded-lg border border-gray-300 dark:border-slate-600 dark:bg-[#1a2236] dark:text-white dark:placeholder-slate-500 pl-10 pr-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>
      </div>

      {/* Table */}
      <Card>
        <CardContent className="p-0">
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead className="bg-gray-50 dark:bg-slate-800/50 border-b border-gray-200 dark:border-slate-700">
                <tr>
                  <th className="px-4 py-3 text-left font-medium text-gray-600 dark:text-slate-400">Name</th>
                  <th className="px-4 py-3 text-left font-medium text-gray-600 dark:text-slate-400">Email</th>
                  <th className="px-4 py-3 text-left font-medium text-gray-600 dark:text-slate-400">Phone</th>
                  <th className="px-4 py-3 text-left font-medium text-gray-600 dark:text-slate-400">GSTIN</th>
                  <th className="px-4 py-3 text-left font-medium text-gray-600 dark:text-slate-400">City</th>
                  <th className="px-4 py-3 text-right font-medium text-gray-600 dark:text-slate-400">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100 dark:divide-slate-700/50">
                {isLoading && <tr><td colSpan={6} className="px-4 py-8 text-center text-gray-400 dark:text-slate-500">Loading...</td></tr>}
                {data?.items.map((c) => (
                  <tr key={c.id} className="hover:bg-gray-50 dark:hover:bg-slate-800/50">
                    <td className="px-4 py-3 font-medium text-gray-900 dark:text-white">{c.name}</td>
                    <td className="px-4 py-3 text-gray-600 dark:text-slate-300">{c.email || '-'}</td>
                    <td className="px-4 py-3 text-gray-600 dark:text-slate-300">{c.phone || '-'}</td>
                    <td className="px-4 py-3 text-gray-600 dark:text-slate-400 font-mono text-xs">{c.gstin || '-'}</td>
                    <td className="px-4 py-3 text-gray-600 dark:text-slate-300">{c.billingCity || '-'}</td>
                    <td className="px-4 py-3 text-right">
                      <button onClick={() => openEdit(c)} className="p-1 text-gray-400 hover:text-blue-600"><Pencil className="h-4 w-4" /></button>
                      <button onClick={() => handleDelete(c.id)} className="p-1 text-gray-400 hover:text-red-600 ml-1"><Trash2 className="h-4 w-4" /></button>
                    </td>
                  </tr>
                ))}
                {data && data.items.length === 0 && (
                  <tr><td colSpan={6} className="px-4 py-8 text-center text-gray-400 dark:text-slate-500">No customers found</td></tr>
                )}
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>

      {/* Pagination */}
      {data && data.totalPages > 1 && (
        <div className="flex items-center justify-between mt-4">
          <p className="text-sm text-gray-600">Showing {data.items.length} of {data.totalCount}</p>
          <div className="flex gap-2">
            <Button size="sm" variant="secondary" disabled={!data.hasPreviousPage} onClick={() => setPage(page - 1)}>Previous</Button>
            <Button size="sm" variant="secondary" disabled={!data.hasNextPage} onClick={() => setPage(page + 1)}>Next</Button>
          </div>
        </div>
      )}

      {/* Slide-over Form */}
      {showForm && (
        <div className="fixed inset-0 z-50 flex justify-end">
          <div className="absolute inset-0 bg-black/30" onClick={() => setShowForm(false)} />
          <div className="relative w-full max-w-md bg-white dark:bg-[#131a2e] shadow-xl overflow-y-auto">
            <div className="flex items-center justify-between p-4 border-b dark:border-slate-700">
              <h2 className="text-lg font-semibold dark:text-white">{editingCustomer ? 'Edit Customer' : 'New Customer'}</h2>
              <button onClick={() => setShowForm(false)} className="p-1 hover:bg-gray-100 dark:hover:bg-slate-700 rounded"><X className="h-5 w-5 dark:text-slate-400" /></button>
            </div>
            <form onSubmit={handleSubmit(onSubmit)} className="p-4 space-y-4">
              <Input label="Name" {...register('name', { required: true })} />
              <Input label="Email" type="email" {...register('email')} />
              <Input label="Phone" {...register('phone')} />
              <Input label="Contact Person" {...register('contactPerson')} />
              <Input label="GSTIN" {...register('gstin')} />
              <Input label="PAN" {...register('pan')} />
              <Input label="GST State Code" {...register('gstStateCode')} />
              <hr className="my-2" />
              <p className="text-sm font-medium text-gray-700 dark:text-slate-300">Billing Address</p>
              <Input label="Street" {...register('billingStreet')} />
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-slate-300 mb-1">Country</label>
                <select {...register('billingCountry')} className="w-full rounded-lg border border-gray-300 dark:border-slate-600 dark:bg-[#1a2236] dark:text-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500">
                  <option value="India">India</option>
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-slate-300 mb-1">State</label>
                <select {...register('billingState')} onChange={(e) => { setValue('billingState', e.target.value); setValue('billingCity', ''); }} className="w-full rounded-lg border border-gray-300 dark:border-slate-600 dark:bg-[#1a2236] dark:text-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500">
                  <option value="">Select State</option>
                  {stateNames.map((s) => <option key={s} value={s}>{s}</option>)}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-slate-300 mb-1">City</label>
                <select {...register('billingCity')} className="w-full rounded-lg border border-gray-300 dark:border-slate-600 dark:bg-[#1a2236] dark:text-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500">
                  <option value="">Select City</option>
                  {(billingState && indianStates[billingState] ? indianStates[billingState] : []).map((c) => <option key={c} value={c}>{c}</option>)}
                </select>
              </div>
              <Input label="Postal Code" {...register('billingPostalCode')} />
              <Input label="Notes" {...register('notes')} />
              <Button type="submit" loading={creating || updating} className="w-full">
                {editingCustomer ? 'Update' : 'Create'}
              </Button>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
