import { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import toast from 'react-hot-toast';
import { Download, Send, CreditCard, X } from 'lucide-react';
import { Button, Input, Card, CardContent, CardHeader, Badge } from '../components/ui';
import { useAppSelector } from '../store/hooks';
import { useGetInvoiceQuery, useSendInvoiceMutation } from '../store/invoiceApi';
import { useGetPaymentsByInvoiceQuery, useRecordPaymentMutation } from '../store/paymentApi';

const statusMap: Record<number, { label: string; variant: 'default' | 'success' | 'warning' | 'danger' | 'info' }> = {
  0: { label: 'Draft', variant: 'default' },
  1: { label: 'Sent', variant: 'info' },
  2: { label: 'Paid', variant: 'success' },
  3: { label: 'Partially Paid', variant: 'warning' },
  4: { label: 'Overdue', variant: 'danger' },
  5: { label: 'Cancelled', variant: 'default' },
};

const paymentModes = ['Cash', 'Card', 'UPI', 'NEFT', 'Cheque', 'Wallet'];

export default function InvoiceDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { data: invoice, isLoading } = useGetInvoiceQuery(id!);
  const { data: payments } = useGetPaymentsByInvoiceQuery(id!);
  const [sendInvoice, { isLoading: sending }] = useSendInvoiceMutation();
  const [recordPayment, { isLoading: recording }] = useRecordPaymentMutation();

  const [showPaymentForm, setShowPaymentForm] = useState(false);
  const [paymentAmount, setPaymentAmount] = useState(0);
  const [paymentMode, setPaymentMode] = useState(0);
  const [paymentRef, setPaymentRef] = useState('');

  const { accessToken } = useAppSelector((state) => state.auth);

  if (isLoading || !invoice) {
    return <div className="text-gray-500">Loading...</div>;
  }

  const st = statusMap[invoice.status] || statusMap[0];

  const handleDownloadPdf = async () => {
    try {
      const response = await fetch(`/api/invoices/${id}/pdf`, {
        headers: { Authorization: `Bearer ${accessToken}` },
      });
      if (!response.ok) {
        toast.error('Failed to download PDF');
        return;
      }
      const blob = await response.blob();
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `${invoice.invoiceNumber}.pdf`;
      link.click();
      window.URL.revokeObjectURL(url);
    } catch {
      toast.error('Failed to download PDF');
    }
  };

  const handleSend = async () => {
    try {
      await sendInvoice({ id: id! }).unwrap();
      toast.success('Invoice email queued');
    } catch {
      toast.error('Failed to send');
    }
  };

  const handleRecordPayment = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await recordPayment({ invoiceId: id!, data: { amount: paymentAmount, paymentMode, referenceNumber: paymentRef || undefined } }).unwrap();
      toast.success('Payment recorded');
      setShowPaymentForm(false);
      setPaymentAmount(0);
      setPaymentRef('');
    } catch (err: unknown) {
      const error = err as { data?: { error?: string } };
      toast.error(error.data?.error || 'Failed to record payment');
    }
  };

  return (
    <div className="max-w-4xl">
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white">{invoice.invoiceNumber}</h1>
          <p className="text-gray-600 dark:text-slate-400">{invoice.customerName}</p>
        </div>
        <div className="flex items-center gap-3">
          <Badge variant={st.variant}>{st.label}</Badge>
          <Button size="sm" variant="secondary" onClick={handleDownloadPdf}>
            <Download className="h-4 w-4 mr-1" />PDF
          </Button>
          <Button size="sm" variant="secondary" onClick={handleSend} loading={sending}>
            <Send className="h-4 w-4 mr-1" />Send
          </Button>
          {invoice.balanceDue > 0 && (
            <Button size="sm" onClick={() => { setPaymentAmount(invoice.balanceDue); setShowPaymentForm(true); }}>
              <CreditCard className="h-4 w-4 mr-1" />Record Payment
            </Button>
          )}
        </div>
      </div>

      {/* Invoice Info */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">
        <Card><CardContent><p className="text-xs text-gray-500 dark:text-slate-500">Date</p><p className="font-medium dark:text-white">{new Date(invoice.invoiceDate).toLocaleDateString()}</p></CardContent></Card>
        <Card><CardContent><p className="text-xs text-gray-500 dark:text-slate-500">Due Date</p><p className="font-medium dark:text-white">{invoice.dueDate ? new Date(invoice.dueDate).toLocaleDateString() : 'On Receipt'}</p></CardContent></Card>
        <Card><CardContent><p className="text-xs text-gray-500 dark:text-slate-500">Total</p><p className="font-medium text-lg dark:text-white">₹{invoice.totalAmount.toFixed(2)}</p></CardContent></Card>
        <Card><CardContent><p className="text-xs text-gray-500 dark:text-slate-500">Balance Due</p><p className="font-medium text-lg text-red-600 dark:text-red-400">₹{invoice.balanceDue.toFixed(2)}</p></CardContent></Card>
      </div>

      {/* Items */}
      <Card className="mb-6">
        <CardHeader><h2 className="font-semibold dark:text-white">Items</h2></CardHeader>
        <CardContent className="p-0">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 dark:bg-slate-800/50 border-b dark:border-slate-700">
              <tr>
                <th className="px-4 py-2 text-left font-medium text-gray-600 dark:text-slate-400">Description</th>
                <th className="px-4 py-2 text-right font-medium text-gray-600 dark:text-slate-400">Qty</th>
                <th className="px-4 py-2 text-right font-medium text-gray-600 dark:text-slate-400">Rate</th>
                <th className="px-4 py-2 text-right font-medium text-gray-600 dark:text-slate-400">Tax</th>
                <th className="px-4 py-2 text-right font-medium text-gray-600 dark:text-slate-400">Amount</th>
              </tr>
            </thead>
            <tbody className="divide-y dark:divide-slate-700/50">
              {invoice.items.map((item) => (
                <tr key={item.id}>
                  <td className="px-4 py-2 dark:text-white">{item.description}</td>
                  <td className="px-4 py-2 text-right dark:text-slate-300">{item.quantity} {item.unit}</td>
                  <td className="px-4 py-2 text-right dark:text-slate-300">₹{item.rate.toFixed(2)}</td>
                  <td className="px-4 py-2 text-right dark:text-slate-300">{item.taxRate}% (₹{item.taxAmount.toFixed(2)})</td>
                  <td className="px-4 py-2 text-right font-medium dark:text-white">₹{item.amount.toFixed(2)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </CardContent>
      </Card>

      {/* Totals */}
      <Card className="mb-6">
        <CardContent>
          <div className="max-w-xs ml-auto space-y-1 text-sm">
            <div className="flex justify-between"><span className="text-gray-600 dark:text-slate-400">Sub Total</span><span className="dark:text-white">₹{invoice.subTotal.toFixed(2)}</span></div>
            {invoice.discountAmount > 0 && (
              <div className="flex justify-between"><span className="text-gray-600 dark:text-slate-400">Discount ({invoice.discountPercentage}%)</span><span className="dark:text-white">-₹{invoice.discountAmount.toFixed(2)}</span></div>
            )}
            {invoice.gstType === 0 ? (
              <>
                <div className="flex justify-between"><span className="text-gray-600 dark:text-slate-400">CGST</span><span className="dark:text-white">₹{invoice.cgstAmount.toFixed(2)}</span></div>
                <div className="flex justify-between"><span className="text-gray-600 dark:text-slate-400">SGST</span><span className="dark:text-white">₹{invoice.sgstAmount.toFixed(2)}</span></div>
              </>
            ) : (
              <div className="flex justify-between"><span className="text-gray-600 dark:text-slate-400">IGST</span><span className="dark:text-white">₹{invoice.igstAmount.toFixed(2)}</span></div>
            )}
            <hr className="dark:border-slate-700" />
            <div className="flex justify-between font-bold text-base"><span className="dark:text-white">Total</span><span className="dark:text-white">₹{invoice.totalAmount.toFixed(2)}</span></div>
            {invoice.amountPaid > 0 && (
              <>
                <div className="flex justify-between text-green-600 dark:text-green-400"><span>Paid</span><span>-₹{invoice.amountPaid.toFixed(2)}</span></div>
                <div className="flex justify-between font-bold dark:text-white"><span>Balance Due</span><span>₹{invoice.balanceDue.toFixed(2)}</span></div>
              </>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Payment History */}
      {payments && payments.length > 0 && (
        <Card className="mb-6">
          <CardHeader><h2 className="font-semibold dark:text-white">Payment History</h2></CardHeader>
          <CardContent className="p-0">
            <table className="w-full text-sm">
              <thead className="bg-gray-50 dark:bg-slate-800/50 border-b dark:border-slate-700">
                <tr>
                  <th className="px-4 py-2 text-left font-medium text-gray-600 dark:text-slate-400">Date</th>
                  <th className="px-4 py-2 text-left font-medium text-gray-600 dark:text-slate-400">Mode</th>
                  <th className="px-4 py-2 text-left font-medium text-gray-600 dark:text-slate-400">Reference</th>
                  <th className="px-4 py-2 text-right font-medium text-gray-600 dark:text-slate-400">Amount</th>
                </tr>
              </thead>
              <tbody className="divide-y dark:divide-slate-700/50">
                {payments.map((p) => (
                  <tr key={p.id}>
                    <td className="px-4 py-2 dark:text-slate-300">{new Date(p.paymentDate).toLocaleDateString()}</td>
                    <td className="px-4 py-2 dark:text-slate-300">{paymentModes[p.paymentMode] || '-'}{p.isRefund && <Badge variant="danger" className="ml-2">Refund</Badge>}</td>
                    <td className="px-4 py-2 text-gray-600 dark:text-slate-400">{p.referenceNumber || '-'}</td>
                    <td className="px-4 py-2 text-right font-medium dark:text-white">{p.isRefund ? '-' : ''}₹{p.amount.toFixed(2)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </CardContent>
        </Card>
      )}

      {/* Notes */}
      {(invoice.notes || invoice.termsAndConditions) && (
        <Card>
          <CardContent>
            {invoice.notes && <><p className="text-sm font-medium text-gray-700 dark:text-slate-300">Notes</p><p className="text-sm text-gray-600 dark:text-slate-400 mb-3">{invoice.notes}</p></>}
            {invoice.termsAndConditions && <><p className="text-sm font-medium text-gray-700 dark:text-slate-300">Terms & Conditions</p><p className="text-sm text-gray-600 dark:text-slate-400">{invoice.termsAndConditions}</p></>}
          </CardContent>
        </Card>
      )}

      {/* Record Payment Modal */}
      {showPaymentForm && (
        <div className="fixed inset-0 z-50 flex items-center justify-center">
          <div className="absolute inset-0 bg-black/30" onClick={() => setShowPaymentForm(false)} />
          <div className="relative bg-white dark:bg-[#131a2e] rounded-xl shadow-xl p-6 w-full max-w-sm">
            <div className="flex items-center justify-between mb-4">
              <h3 className="text-lg font-semibold dark:text-white">Record Payment</h3>
              <button onClick={() => setShowPaymentForm(false)} className="p-1 hover:bg-gray-100 dark:hover:bg-slate-700 rounded"><X className="h-5 w-5 dark:text-slate-400" /></button>
            </div>
            <form onSubmit={handleRecordPayment} className="space-y-4">
              <Input label="Amount (₹)" type="number" step="0.01" value={paymentAmount}
                onChange={(e) => setPaymentAmount(Number(e.target.value))} required />
              <div>
                <label className="block text-sm font-medium text-gray-700 dark:text-slate-300 mb-1">Payment Mode</label>
                <select value={paymentMode} onChange={(e) => setPaymentMode(Number(e.target.value))}
                  className="w-full rounded-lg border border-gray-300 dark:border-slate-600 dark:bg-[#1a2236] dark:text-white px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500">
                  {paymentModes.map((mode, i) => <option key={i} value={i}>{mode}</option>)}
                </select>
              </div>
              <Input label="Reference #" value={paymentRef} onChange={(e) => setPaymentRef(e.target.value)} />
              <Button type="submit" loading={recording} className="w-full">Record Payment</Button>
            </form>
          </div>
        </div>
      )}

      <div className="mt-6">
        <Button variant="ghost" onClick={() => navigate('/invoices')}>← Back to Invoices</Button>
      </div>
    </div>
  );
}
