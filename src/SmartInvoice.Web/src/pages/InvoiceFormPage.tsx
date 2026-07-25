import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import toast from 'react-hot-toast';
import { Plus, Trash2 } from 'lucide-react';
import { Button, Input, Card, CardContent, CardHeader } from '../components/ui';
import { useGetCustomersQuery } from '../store/customerApi';
import { useGetProductsQuery } from '../store/productApi';
import { useCreateInvoiceMutation, type InvoiceItemRequest } from '../store/invoiceApi';

interface LineItem extends InvoiceItemRequest {
  amount: number;
  taxAmount: number;
}

export default function InvoiceFormPage() {
  const navigate = useNavigate();
  const { data: customers } = useGetCustomersQuery({ pageSize: 100 });
  const { data: products } = useGetProductsQuery({ pageSize: 100 });
  const [createInvoice, { isLoading }] = useCreateInvoiceMutation();

  const [customerId, setCustomerId] = useState('');
  const [invoiceType, setInvoiceType] = useState(0);
  const [dueDate, setDueDate] = useState('');
  const [discountPercentage, setDiscountPercentage] = useState(0);
  const [notes, setNotes] = useState('');
  const [terms, setTerms] = useState('');
  const [items, setItems] = useState<LineItem[]>([createEmptyItem()]);

  function createEmptyItem(): LineItem {
    return { description: '', quantity: 1, unit: 'Nos', rate: 0, discountPercentage: 0, taxRate: 18, amount: 0, taxAmount: 0 };
  }

  function calculateItem(item: LineItem): LineItem {
    const lineTotal = item.quantity * item.rate;
    const discAmt = lineTotal * item.discountPercentage / 100;
    const taxable = lineTotal - discAmt;
    const taxAmt = taxable * item.taxRate / 100;
    return { ...item, amount: taxable, taxAmount: taxAmt };
  }

  function updateItem(index: number, field: keyof LineItem, value: string | number) {
    setItems((prev) => {
      const updated = [...prev];
      updated[index] = { ...updated[index]!, [field]: value };
      updated[index] = calculateItem(updated[index]!);
      return updated;
    });
  }

  function addItem() {
    setItems((prev) => [...prev, createEmptyItem()]);
  }

  function removeItem(index: number) {
    if (items.length <= 1) return;
    setItems((prev) => prev.filter((_, i) => i !== index));
  }

  function selectProduct(index: number, productId: string) {
    const product = products?.items.find((p) => p.id === productId);
    if (!product) return;
    setItems((prev) => {
      const updated = [...prev];
      updated[index] = calculateItem({
        ...updated[index]!,
        productId,
        description: product.name,
        hsnSacCode: product.hsnSacCode ?? '',
        unit: product.unit,
        rate: product.price,
        taxRate: product.taxRate,
      });
      return updated;
    });
  }

  // Totals
  const subTotal = items.reduce((s, i) => s + i.amount, 0);
  const totalTax = items.reduce((s, i) => s + i.taxAmount, 0);
  const discountAmount = subTotal * discountPercentage / 100;
  const grandTotal = subTotal - discountAmount + totalTax;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!customerId) { toast.error('Select a customer'); return; }
    if (items.some((i) => !i.description || i.rate <= 0)) { toast.error('Fill all line items'); return; }

    try {
      const result = await createInvoice({
        customerId,
        type: invoiceType,
        dueDate: dueDate || undefined,
        discountPercentage,
        notes: notes || undefined,
        termsAndConditions: terms || undefined,
        items: items.map(({ amount, taxAmount, ...rest }) => rest),
      }).unwrap();
      toast.success(`Invoice ${result.invoiceNumber} created`);
      navigate(`/invoices/${result.id}`);
    } catch {
      toast.error('Failed to create invoice');
    }
  };

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-900 dark:text-white mb-6">New Invoice</h1>

      <form onSubmit={handleSubmit} className="space-y-6 max-w-5xl">
        {/* Header */}
        <Card>
          <CardContent>
            <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-slate-300 mb-1">Customer</label>
                <select value={customerId} onChange={(e) => setCustomerId(e.target.value)}
                  className="w-full rounded-lg border border-gray-300 dark:border-slate-600 dark:bg-[#1a2236] dark:text-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" required>
                  <option value="">Select customer</option>
                  {customers?.items.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-slate-300 mb-1">Type</label>
                <select value={invoiceType} onChange={(e) => setInvoiceType(Number(e.target.value))}
                  className="w-full rounded-lg border border-gray-300 dark:border-slate-600 dark:bg-[#1a2236] dark:text-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500">
                  <option value={0}>Regular</option>
                  <option value={1}>GST Invoice</option>
                  <option value={2}>Proforma</option>
                  <option value={6}>Quotation</option>
                </select>
              </div>
              <Input label="Due Date" type="date" value={dueDate} onChange={(e) => setDueDate(e.target.value)} />
              <Input label="Discount %" type="number" step="0.01" value={discountPercentage}
                onChange={(e) => setDiscountPercentage(Number(e.target.value))} />
            </div>
          </CardContent>
        </Card>

        {/* Line Items */}
        <Card>
          <CardHeader>
            <div className="flex items-center justify-between">
              <h2 className="text-lg font-semibold">Line Items</h2>
              <Button type="button" size="sm" variant="secondary" onClick={addItem}>
                <Plus className="h-4 w-4 mr-1" />Add Item
              </Button>
            </div>
          </CardHeader>
          <CardContent className="p-0">
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead className="bg-gray-50 dark:bg-slate-800/50 border-b dark:border-slate-700">
                  <tr>
                    <th className="px-3 py-2 text-left text-xs font-medium text-gray-500 dark:text-slate-400">Product</th>
                    <th className="px-3 py-2 text-left text-xs font-medium text-gray-500 dark:text-slate-400">Description</th>
                    <th className="px-3 py-2 text-right text-xs font-medium text-gray-500 dark:text-slate-400 w-16">Qty</th>
                    <th className="px-3 py-2 text-right text-xs font-medium text-gray-500 dark:text-slate-400 w-24">Rate</th>
                    <th className="px-3 py-2 text-right text-xs font-medium text-gray-500 dark:text-slate-400 w-16">Tax%</th>
                    <th className="px-3 py-2 text-right text-xs font-medium text-gray-500 dark:text-slate-400 w-24">Amount</th>
                    <th className="px-3 py-2 w-10"></th>
                  </tr>
                </thead>
                <tbody className="divide-y dark:divide-slate-700/50">
                  {items.map((item, idx) => (
                    <tr key={idx}>
                      <td className="px-3 py-2">
                        <select value={item.productId ?? ''} onChange={(e) => selectProduct(idx, e.target.value)}
                          className="w-full rounded border border-gray-300 dark:border-slate-600 dark:bg-[#1a2236] dark:text-white px-2 py-1 text-sm">
                          <option value="">Custom</option>
                          {products?.items.map((p) => <option key={p.id} value={p.id}>{p.name}</option>)}
                        </select>
                      </td>
                      <td className="px-3 py-2">
                        <input value={item.description} onChange={(e) => updateItem(idx, 'description', e.target.value)}
                          className="w-full rounded border border-gray-300 dark:border-slate-600 dark:bg-[#1a2236] dark:text-white px-2 py-1 text-sm" required />
                      </td>
                      <td className="px-3 py-2">
                        <input type="number" min="0.01" step="0.01" value={item.quantity}
                          onChange={(e) => updateItem(idx, 'quantity', Number(e.target.value))}
                          className="w-full rounded border border-gray-300 dark:border-slate-600 dark:bg-[#1a2236] dark:text-white px-2 py-1 text-sm text-right" />
                      </td>
                      <td className="px-3 py-2">
                        <input type="number" min="0" step="0.01" value={item.rate}
                          onChange={(e) => updateItem(idx, 'rate', Number(e.target.value))}
                          className="w-full rounded border border-gray-300 dark:border-slate-600 dark:bg-[#1a2236] dark:text-white px-2 py-1 text-sm text-right" />
                      </td>
                      <td className="px-3 py-2">
                        <input type="number" min="0" step="0.01" value={item.taxRate}
                          onChange={(e) => updateItem(idx, 'taxRate', Number(e.target.value))}
                          className="w-full rounded border border-gray-300 dark:border-slate-600 dark:bg-[#1a2236] dark:text-white px-2 py-1 text-sm text-right" />
                      </td>
                      <td className="px-3 py-2 text-right font-medium dark:text-white">₹{item.amount.toFixed(2)}</td>
                      <td className="px-3 py-2">
                        <button type="button" onClick={() => removeItem(idx)} className="p-1 text-gray-400 hover:text-red-600">
                          <Trash2 className="h-4 w-4" />
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </CardContent>
        </Card>

        {/* Totals + Notes */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-slate-300 mb-1">Notes</label>
              <textarea value={notes} onChange={(e) => setNotes(e.target.value)} rows={3}
                className="w-full rounded-lg border border-gray-300 dark:border-slate-600 dark:bg-[#1a2236] dark:text-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-slate-300 mb-1">Terms & Conditions</label>
              <textarea value={terms} onChange={(e) => setTerms(e.target.value)} rows={3}
                className="w-full rounded-lg border border-gray-300 dark:border-slate-600 dark:bg-[#1a2236] dark:text-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
            </div>
          </div>

          <Card>
            <CardContent>
              <div className="space-y-2 text-sm">
                <div className="flex justify-between"><span className="text-gray-600 dark:text-slate-400">Sub Total</span><span className="dark:text-white">₹{subTotal.toFixed(2)}</span></div>
                {discountPercentage > 0 && (
                  <div className="flex justify-between"><span className="text-gray-600 dark:text-slate-400">Discount ({discountPercentage}%)</span><span className="dark:text-white">-₹{discountAmount.toFixed(2)}</span></div>
                )}
                <div className="flex justify-between"><span className="text-gray-600 dark:text-slate-400">Tax</span><span className="dark:text-white">₹{totalTax.toFixed(2)}</span></div>
                <hr className="dark:border-slate-700" />
                <div className="flex justify-between text-base font-bold"><span className="dark:text-white">Total</span><span className="dark:text-white">₹{grandTotal.toFixed(2)}</span></div>
              </div>
            </CardContent>
          </Card>
        </div>

        <div className="flex justify-end gap-3">
          <Button type="button" variant="secondary" onClick={() => navigate('/invoices')}>Cancel</Button>
          <Button type="submit" loading={isLoading}>Create Invoice</Button>
        </div>
      </form>
    </div>
  );
}
