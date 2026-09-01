import { useState } from 'react';
import { useForm } from 'react-hook-form';
import toast from 'react-hot-toast';
import { Plus, Search, Pencil, Trash2, X, Eye } from 'lucide-react';
import { Button, Input, Card, CardContent } from '../components/ui';
import {
  useGetVendorsQuery,
  useCreateVendorMutation,
  useUpdateVendorMutation,
  useDeleteVendorMutation,
  type VendorRequest,
  type VendorResponse,
} from '../store/vendorApi';
import { stateNames, indianStates } from '../data/indianLocations';
import { useNavigate } from 'react-router-dom';

export default function VendorsPage() {
  const navigate = useNavigate();
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');
  const [showForm, setShowForm] = useState(false);
  const [editingVendor, setEditingVendor] = useState<VendorResponse | null>(null);

  const { data, isLoading } = useGetVendorsQuery({ page, pageSize: 15, search: search || undefined });
  const [createVendor, { isLoading: creating }] = useCreateVendorMutation();
  const [updateVendor, { isLoading: updating }] = useUpdateVendorMutation();
  const [deleteVendor] = useDeleteVendorMutation();

  const { register, handleSubmit, reset, watch, setValue } = useForm<VendorRequest>();
  const selectedState = watch('state');

  const openCreate = () => {
    setEditingVendor(null);
    reset({ name: '', email: '', phone: '', country: 'India' });
    setShowForm(true);
  };

  const openEdit = (v: VendorResponse) => {
    setEditingVendor(v);
    reset({
      name: v.name,
      email: v.email ?? '',
      phone: v.phone ?? '',
      contactPerson: v.contactPerson ?? '',
      notes: v.notes ?? '',
      gstin: v.gstin ?? '',
      pan: v.pan ?? '',
      stateCode: v.stateCode ?? '',
      street: v.street ?? '',
      city: v.city ?? '',
      state: v.state ?? '',
      postalCode: v.postalCode ?? '',
      country: v.country ?? 'India',
      openingBalance: v.openingBalance ?? 0,
    });
    setShowForm(true);
  };

  const onSubmit = async (data: VendorRequest) => {
    try {
      if (editingVendor) {
        await updateVendor({ id: editingVendor.id, data }).unwrap();
        toast.success('Vendor updated');
      } else {
        await createVendor(data).unwrap();
        toast.success('Vendor created');
      }
      setShowForm(false);
    } catch {
      toast.error('Operation failed');
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm('Delete this vendor?')) return;
    try {
      await deleteVendor(id).unwrap();
      toast.success('Vendor deleted');
    } catch {
      toast.error('Delete failed');
    }
  };

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Vendors</h1>
        <Button onClick={openCreate}><Plus className="h-4 w-4 mr-2" />Add Vendor</Button>
      </div>

      {/* Search */}
      <div className="mb-6">
        <Input
          placeholder="Search vendors..."
          value={search}
          onChange={(e) => { setSearch(e.target.value); setPage(1); }}
          icon={<Search className="h-4 w-4" />}
        />
      </div>

      {/* Form Modal */}
      {showForm && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <Card className="w-full max-w-2xl max-h-[90vh] overflow-y-auto">
            <div className="flex items-center justify-between p-6 border-b border-gray-200 dark:border-slate-700">
              <h2 className="text-lg font-semibold text-gray-900 dark:text-white">
                {editingVendor ? 'Edit Vendor' : 'New Vendor'}
              </h2>
              <button onClick={() => setShowForm(false)} className="text-gray-500 hover:text-gray-700 dark:hover:text-slate-300">
                <X className="h-5 w-5" />
              </button>
            </div>
            <CardContent className="p-6">
              <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <Input label="Name" {...register('name', { required: true })} />
                  <Input label="Email" type="email" {...register('email')} />
                  <Input label="Phone" {...register('phone')} />
                  <Input label="Contact Person" {...register('contactPerson')} />
                  <Input label="GSTIN" {...register('gstin')} />
                  <Input label="PAN" {...register('pan')} />
                  <Input label="State Code" {...register('stateCode')} />
                  <Input label="Opening Balance" type="number" step="0.01" {...register('openingBalance')} />
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <Input label="Street" {...register('street')} />
                  <div>
                    <label className="block text-sm font-medium text-gray-700 dark:text-slate-300 mb-1">Country</label>
                    <select
                      {...register('country')}
                      className="w-full rounded-lg border border-gray-300 dark:border-slate-600 dark:bg-[#1a2236] dark:text-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                    >
                      <option value="India">India</option>
                    </select>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 dark:text-slate-300 mb-1">State</label>
                    <select
                      {...register('state')}
                      onChange={(e) => { setValue('state', e.target.value); setValue('city', ''); }}
                      className="w-full rounded-lg border border-gray-300 dark:border-slate-600 dark:bg-[#1a2236] dark:text-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                    >
                      <option value="">Select State</option>
                      {stateNames.map((s) => <option key={s} value={s}>{s}</option>)}
                    </select>
                  </div>
                  <div>
                    <label className="block text-sm font-medium text-gray-700 dark:text-slate-300 mb-1">City</label>
                    <select
                      {...register('city')}
                      className="w-full rounded-lg border border-gray-300 dark:border-slate-600 dark:bg-[#1a2236] dark:text-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                    >
                      <option value="">Select City</option>
                      {(selectedState && indianStates[selectedState] ? indianStates[selectedState] : []).map((c) => (
                        <option key={c} value={c}>{c}</option>
                      ))}
                    </select>
                  </div>
                  <Input label="Postal Code" {...register('postalCode')} />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-slate-300 mb-1">Notes</label>
                  <textarea
                    {...register('notes')}
                    className="w-full rounded-lg border border-gray-300 dark:border-slate-600 dark:bg-[#1a2236] dark:text-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 resize-none h-20"
                  />
                </div>

                <div className="flex justify-end gap-3 pt-4">
                  <Button type="button" variant="ghost" onClick={() => setShowForm(false)}>Cancel</Button>
                  <Button type="submit" loading={creating || updating}>Save Vendor</Button>
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
            <div className="p-6 text-center text-gray-500 dark:text-slate-400">Loading vendors...</div>
          ) : (data?.items?.length ?? 0) === 0 ? (
            <div className="p-6 text-center text-gray-500 dark:text-slate-400">No vendors found</div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-gray-200 dark:border-slate-700 bg-gray-50 dark:bg-slate-800/50">
                    <th className="px-6 py-3 text-left font-medium text-gray-600 dark:text-slate-300">Name</th>
                    <th className="px-6 py-3 text-left font-medium text-gray-600 dark:text-slate-300">Email</th>
                    <th className="px-6 py-3 text-left font-medium text-gray-600 dark:text-slate-300">Phone</th>
                    <th className="px-6 py-3 text-left font-medium text-gray-600 dark:text-slate-300">City</th>
                    <th className="px-6 py-3 text-right font-medium text-gray-600 dark:text-slate-300">Balance</th>
                    <th className="px-6 py-3 text-center font-medium text-gray-600 dark:text-slate-300">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {data?.items?.map((vendor) => (
                    <tr key={vendor.id} className="border-b border-gray-200 dark:border-slate-700 hover:bg-gray-50 dark:hover:bg-slate-800/50 transition">
                      <td className="px-6 py-4 text-gray-900 dark:text-white font-medium">{vendor.name}</td>
                      <td className="px-6 py-4 text-gray-600 dark:text-slate-400">{vendor.email ?? '-'}</td>
                      <td className="px-6 py-4 text-gray-600 dark:text-slate-400">{vendor.phone ?? '-'}</td>
                      <td className="px-6 py-4 text-gray-600 dark:text-slate-400">{vendor.city ?? '-'}</td>
                      <td className="px-6 py-4 text-right font-medium text-gray-900 dark:text-white">₹{vendor.outstandingBalance?.toFixed(2) ?? '0.00'}</td>
                      <td className="px-6 py-4 text-center">
                        <div className="flex justify-center gap-2">
                          <button
                            onClick={() => openEdit(vendor)}
                            className="p-2 text-blue-600 hover:bg-blue-50 dark:hover:bg-blue-600/20 rounded transition"
                            title="Edit"
                          >
                            <Pencil className="h-4 w-4" />
                          </button>
                          <button
                            onClick={() => handleDelete(vendor.id)}
                            className="p-2 text-red-600 hover:bg-red-50 dark:hover:bg-red-600/20 rounded transition"
                            title="Delete"
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
      {(data?.total ?? 0) > 15 && (
        <div className="mt-6 flex justify-center gap-2">
          <Button
            variant="ghost"
            onClick={() => setPage((p) => Math.max(p - 1, 1))}
            disabled={page === 1}
          >
            Previous
          </Button>
          <span className="px-4 py-2 text-gray-600 dark:text-slate-400">
            Page {page} of {Math.ceil((data?.total ?? 1) / 15)}
          </span>
          <Button
            variant="ghost"
            onClick={() => setPage((p) => p + 1)}
            disabled={page * 15 >= (data?.total ?? 0)}
          >
            Next
          </Button>
        </div>
      )}
    </div>
  );
}
