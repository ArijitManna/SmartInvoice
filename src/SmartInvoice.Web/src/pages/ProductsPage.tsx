import { useState } from 'react';
import { useForm } from 'react-hook-form';
import toast from 'react-hot-toast';
import { Plus, Search, Pencil, Trash2, X } from 'lucide-react';
import { Button, Input, Card, CardContent, Badge } from '../components/ui';
import {
  useGetProductsQuery,
  useCreateProductMutation,
  useUpdateProductMutation,
  useDeleteProductMutation,
  useGetCategoriesQuery,
  useCreateCategoryMutation,
  type ProductRequest,
  type ProductResponse,
} from '../store/productApi';

export default function ProductsPage() {
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');
  const [showForm, setShowForm] = useState(false);
  const [editingProduct, setEditingProduct] = useState<ProductResponse | null>(null);

  const { data, isLoading } = useGetProductsQuery({ page, pageSize: 15, search: search || undefined });
  const { data: categories } = useGetCategoriesQuery();
  const [createProduct, { isLoading: creating }] = useCreateProductMutation();
  const [updateProduct, { isLoading: updating }] = useUpdateProductMutation();
  const [deleteProduct] = useDeleteProductMutation();
  const [createCategory] = useCreateCategoryMutation();
  const [newCategoryName, setNewCategoryName] = useState('');

  const { register, handleSubmit, reset } = useForm<ProductRequest>();

  const openCreate = () => {
    setEditingProduct(null);
    reset({ name: '', type: 0, unit: 'Nos', price: 0, taxRate: 18 });
    setShowForm(true);
  };

  const openEdit = (p: ProductResponse) => {
    setEditingProduct(p);
    reset({
      name: p.name, description: p.description ?? '', type: p.type,
      sku: p.sku ?? '', hsnSacCode: p.hsnSacCode ?? '', unit: p.unit,
      price: p.price, taxRate: p.taxRate, categoryId: p.categoryId,
    });
    setShowForm(true);
  };

  const onSubmit = async (data: ProductRequest) => {
    const payload = { ...data, price: Number(data.price), taxRate: Number(data.taxRate), type: Number(data.type), categoryId: data.categoryId || null };
    try {
      if (editingProduct) {
        await updateProduct({ id: editingProduct.id, data: payload }).unwrap();
        toast.success('Product updated');
      } else {
        await createProduct(payload).unwrap();
        toast.success('Product created');
      }
      setShowForm(false);
    } catch {
      toast.error('Operation failed');
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm('Delete this product?')) return;
    try {
      await deleteProduct(id).unwrap();
      toast.success('Product deleted');
    } catch {
      toast.error('Delete failed');
    }
  };

  const typeLabel = (t: number) => t === 0 ? 'Product' : 'Service';

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Products & Services</h1>
        <Button onClick={openCreate}><Plus className="h-4 w-4 mr-2" />Add Product</Button>
      </div>

      <div className="mb-4 max-w-sm">
        <div className="relative">
          <Search className="absolute left-3 top-2.5 h-4 w-4 text-gray-400" />
          <input
            type="text"
            placeholder="Search products..."
            value={search}
            onChange={(e) => { setSearch(e.target.value); setPage(1); }}
            className="w-full rounded-lg border border-gray-300 dark:border-slate-600 dark:bg-[#1a2236] dark:text-white dark:placeholder-slate-500 pl-10 pr-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>
      </div>

      <Card>
        <CardContent className="p-0">
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead className="bg-gray-50 dark:bg-slate-800/50 border-b border-gray-200 dark:border-slate-700">
                <tr>
                  <th className="px-4 py-3 text-left font-medium text-gray-600 dark:text-slate-400">Name</th>
                  <th className="px-4 py-3 text-left font-medium text-gray-600 dark:text-slate-400">Type</th>
                  <th className="px-4 py-3 text-left font-medium text-gray-600 dark:text-slate-400">SKU</th>
                  <th className="px-4 py-3 text-right font-medium text-gray-600 dark:text-slate-400">Price</th>
                  <th className="px-4 py-3 text-right font-medium text-gray-600 dark:text-slate-400">Tax %</th>
                  <th className="px-4 py-3 text-left font-medium text-gray-600 dark:text-slate-400">Category</th>
                  <th className="px-4 py-3 text-right font-medium text-gray-600 dark:text-slate-400">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100 dark:divide-slate-700/50">
                {isLoading && <tr><td colSpan={7} className="px-4 py-8 text-center text-gray-400 dark:text-slate-500">Loading...</td></tr>}
                {data?.items.map((p) => (
                  <tr key={p.id} className="hover:bg-gray-50 dark:hover:bg-slate-800/50">
                    <td className="px-4 py-3 font-medium text-gray-900 dark:text-white">{p.name}</td>
                    <td className="px-4 py-3"><Badge variant={p.type === 0 ? 'info' : 'success'}>{typeLabel(p.type)}</Badge></td>
                    <td className="px-4 py-3 text-gray-600 dark:text-slate-400 font-mono text-xs">{p.sku || '-'}</td>
                    <td className="px-4 py-3 text-right text-gray-900 dark:text-white">₹{p.price.toFixed(2)}</td>
                    <td className="px-4 py-3 text-right text-gray-600 dark:text-slate-300">{p.taxRate}%</td>
                    <td className="px-4 py-3 text-gray-600 dark:text-slate-300">{p.categoryName || '-'}</td>
                    <td className="px-4 py-3 text-right">
                      <button onClick={() => openEdit(p)} className="p-1 text-gray-400 hover:text-blue-600"><Pencil className="h-4 w-4" /></button>
                      <button onClick={() => handleDelete(p.id)} className="p-1 text-gray-400 hover:text-red-600 ml-1"><Trash2 className="h-4 w-4" /></button>
                    </td>
                  </tr>
                ))}
                {data && data.items.length === 0 && (
                  <tr><td colSpan={7} className="px-4 py-8 text-center text-gray-400 dark:text-slate-500">No products found</td></tr>
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

      {/* Slide-over Form */}
      {showForm && (
        <div className="fixed inset-0 z-50 flex justify-end">
          <div className="absolute inset-0 bg-black/30" onClick={() => setShowForm(false)} />
          <div className="relative w-full max-w-md bg-white dark:bg-[#131a2e] shadow-xl overflow-y-auto">
            <div className="flex items-center justify-between p-4 border-b dark:border-slate-700">
              <h2 className="text-lg font-semibold dark:text-white">{editingProduct ? 'Edit Product' : 'New Product'}</h2>
              <button onClick={() => setShowForm(false)} className="p-1 hover:bg-gray-100 dark:hover:bg-slate-700 rounded"><X className="h-5 w-5 dark:text-slate-400" /></button>
            </div>
            <form onSubmit={handleSubmit(onSubmit)} className="p-4 space-y-4">
              <Input label="Name" {...register('name', { required: true })} />
              <Input label="Description" {...register('description')} />

              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-slate-300 mb-1">Type</label>
                <select {...register('type')} className="w-full rounded-lg border border-gray-300 dark:border-slate-600 dark:bg-[#1a2236] dark:text-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500">
                  <option value={0}>Product</option>
                  <option value={1}>Service</option>
                </select>
              </div>

              <div className="grid grid-cols-2 gap-3">
                <Input label="SKU" {...register('sku')} />
                <Input label="HSN/SAC Code" {...register('hsnSacCode')} />
              </div>

              <div className="grid grid-cols-3 gap-3">
                <Input label="Unit" {...register('unit')} />
                <Input label="Price (₹)" type="number" step="0.01" {...register('price', { valueAsNumber: true })} />
                <Input label="Tax %" type="number" step="0.01" {...register('taxRate', { valueAsNumber: true })} />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-slate-300 mb-1">Category</label>
                <select {...register('categoryId')} className="w-full rounded-lg border border-gray-300 dark:border-slate-600 dark:bg-[#1a2236] dark:text-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500">
                  <option value="">None</option>
                  {categories?.map((cat) => (
                    <option key={cat.id} value={cat.id}>{cat.name}</option>
                  ))}
                </select>
                <div className="flex gap-2 mt-2">
                  <input
                    type="text"
                    placeholder="New category name"
                    value={newCategoryName}
                    onChange={(e) => setNewCategoryName(e.target.value)}
                    className="flex-1 rounded border border-gray-300 px-2 py-1 text-sm"
                  />
                  <button type="button" onClick={async () => {
                    if (!newCategoryName.trim()) return;
                    try {
                      await createCategory({ name: newCategoryName.trim() }).unwrap();
                      setNewCategoryName('');
                      toast.success('Category added');
                    } catch { toast.error('Failed'); }
                  }} className="px-2 py-1 text-xs bg-blue-100 text-blue-700 rounded hover:bg-blue-200">Add</button>
                </div>
              </div>

              <Button type="submit" loading={creating || updating} className="w-full">
                {editingProduct ? 'Update' : 'Create'}
              </Button>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
