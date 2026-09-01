import { useState } from 'react';
import { useForm } from 'react-hook-form';
import toast from 'react-hot-toast';
import { Plus, Search, Pencil, Trash2, X } from 'lucide-react';
import { Button, Input, Card, CardContent } from '../components/ui';
import {
  useGetProductsQuery,
  useCreateProductMutation,
  useUpdateProductMutation,
  useDeleteProductMutation,
  type ProductRequest,
  type ProductResponse,
} from '../store/productApi';

export default function InventoryPage() {
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');
  const [showForm, setShowForm] = useState(false);
  const [editingProduct, setEditingProduct] = useState<ProductResponse | null>(null);

  const { data, isLoading } = useGetProductsQuery({ page, pageSize: 20, search: search || undefined });
  const [createProduct, { isLoading: creating }] = useCreateProductMutation();
  const [updateProduct, { isLoading: updating }] = useUpdateProductMutation();
  const [deleteProduct] = useDeleteProductMutation();

  const { register, handleSubmit, reset } = useForm<ProductRequest>();

  const openCreate = () => {
    setEditingProduct(null);
    reset({ name: '', price: 0, unit: 'Nos', taxRate: 18, purchasePrice: 0, openingStock: 0, trackInventory: false });
    setShowForm(true);
  };

  const openEdit = (p: ProductResponse) => {
    setEditingProduct(p);
    reset({
      name: p.name,
      description: p.description ?? '',
      sku: p.sku ?? '',
      hsnSacCode: p.hsnSacCode ?? '',
      unit: p.unit ?? 'Nos',
      price: p.price,
      taxRate: p.taxRate,
      purchasePrice: p.purchasePrice ?? 0,
      openingStock: p.openingStock ?? 0,
      lowStockThreshold: p.lowStockThreshold ?? 0,
      barcode: p.barcode ?? '',
      brand: p.brand ?? '',
      trackInventory: p.trackInventory ?? false,
    });
    setShowForm(true);
  };

  const onSubmit = async (data: ProductRequest) => {
    try {
      if (editingProduct) {
        await updateProduct({ id: editingProduct.id, data }).unwrap();
        toast.success('Product updated');
      } else {
        await createProduct(data).unwrap();
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

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Inventory</h1>
        <Button onClick={openCreate}><Plus className="h-4 w-4 mr-2" />Add Product</Button>
      </div>

      {/* Search */}
      <div className="mb-6">
        <Input
          placeholder="Search products..."
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
                {editingProduct ? 'Edit Product' : 'New Product'}
              </h2>
              <button onClick={() => setShowForm(false)} className="text-gray-500 hover:text-gray-700 dark:hover:text-slate-300">
                <X className="h-5 w-5" />
              </button>
            </div>
            <CardContent className="p-6">
              <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <Input label="Name" {...register('name', { required: true })} />
                  <Input label="SKU" {...register('sku')} />
                  <Input label="HSN/SAC Code" {...register('hsnSacCode')} />
                  <Input label="Brand" {...register('brand')} />
                  <Input label="Barcode" {...register('barcode')} />
                  <Input label="Unit" {...register('unit')} />
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <Input label="Selling Price" type="number" step="0.01" {...register('price', { valueAsNumber: true })} />
                  <Input label="Purchase Price" type="number" step="0.01" {...register('purchasePrice', { valueAsNumber: true })} />
                  <Input label="Tax Rate (%)" type="number" step="0.01" {...register('taxRate', { valueAsNumber: true })} />
                  <Input label="Opening Stock" type="number" step="0.01" {...register('openingStock', { valueAsNumber: true })} />
                  <Input label="Low Stock Alert" type="number" {...register('lowStockThreshold', { valueAsNumber: true })} />
                </div>

                <div>
                  <label className="block text-sm font-medium text-gray-700 dark:text-slate-300 mb-1">Description</label>
                  <textarea
                    {...register('description')}
                    className="w-full rounded-lg border border-gray-300 dark:border-slate-600 dark:bg-[#1a2236] dark:text-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 resize-none h-16"
                  />
                </div>

                <label className="flex items-center gap-2 text-gray-700 dark:text-slate-300">
                  <input type="checkbox" {...register('trackInventory')} className="rounded" />
                  <span className="text-sm">Track Inventory</span>
                </label>

                <div className="flex justify-end gap-3 pt-4">
                  <Button type="button" variant="ghost" onClick={() => setShowForm(false)}>Cancel</Button>
                  <Button type="submit" loading={creating || updating}>Save Product</Button>
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
            <div className="p-6 text-center text-gray-500 dark:text-slate-400">Loading products...</div>
          ) : (data?.items?.length ?? 0) === 0 ? (
            <div className="p-6 text-center text-gray-500 dark:text-slate-400">No products found</div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-gray-200 dark:border-slate-700 bg-gray-50 dark:bg-slate-800/50">
                    <th className="px-6 py-3 text-left font-medium text-gray-600 dark:text-slate-300">Name</th>
                    <th className="px-6 py-3 text-left font-medium text-gray-600 dark:text-slate-300">SKU</th>
                    <th className="px-6 py-3 text-right font-medium text-gray-600 dark:text-slate-300">Price</th>
                    <th className="px-6 py-3 text-right font-medium text-gray-600 dark:text-slate-300">Tax</th>
                    <th className="px-6 py-3 text-right font-medium text-gray-600 dark:text-slate-300">Stock</th>
                    <th className="px-6 py-3 text-center font-medium text-gray-600 dark:text-slate-300">Track</th>
                    <th className="px-6 py-3 text-center font-medium text-gray-600 dark:text-slate-300">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {data?.items?.map((product) => (
                    <tr key={product.id} className="border-b border-gray-200 dark:border-slate-700 hover:bg-gray-50 dark:hover:bg-slate-800/50 transition">
                      <td className="px-6 py-4 text-gray-900 dark:text-white font-medium">{product.name}</td>
                      <td className="px-6 py-4 text-gray-600 dark:text-slate-400">{product.sku ?? '-'}</td>
                      <td className="px-6 py-4 text-right font-medium text-gray-900 dark:text-white">₹{product.price.toFixed(2)}</td>
                      <td className="px-6 py-4 text-right text-gray-600 dark:text-slate-400">{product.taxRate}%</td>
                      <td className="px-6 py-4 text-right font-medium text-gray-900 dark:text-white">{product.openingStock}</td>
                      <td className="px-6 py-4 text-center">
                        <span className={`text-xs font-medium ${product.trackInventory ? 'text-green-600 dark:text-green-400' : 'text-gray-400'}`}>
                          {product.trackInventory ? '✓' : '✗'}
                        </span>
                      </td>
                      <td className="px-6 py-4 text-center">
                        <div className="flex justify-center gap-2">
                          <button
                            onClick={() => openEdit(product)}
                            className="p-2 text-blue-600 hover:bg-blue-50 dark:hover:bg-blue-600/20 rounded transition"
                          >
                            <Pencil className="h-4 w-4" />
                          </button>
                          <button
                            onClick={() => handleDelete(product.id)}
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
