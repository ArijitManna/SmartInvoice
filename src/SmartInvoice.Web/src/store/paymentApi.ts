import { api } from './api';

export interface PaymentResponse {
  id: string;
  invoiceId: string;
  amount: number;
  paymentMode: number;
  paymentDate: string;
  referenceNumber: string | null;
  notes: string | null;
  isRefund: boolean;
  createdAt: string;
}

export interface RecordPaymentRequest {
  amount: number;
  paymentMode: number;
  paymentDate?: string;
  referenceNumber?: string;
  notes?: string;
}

export const paymentApi = api.injectEndpoints({
  endpoints: (builder) => ({
    getPaymentsByInvoice: builder.query<PaymentResponse[], string>({
      query: (invoiceId) => `/invoices/${invoiceId}/payments`,
      providesTags: ['Payments'],
    }),
    recordPayment: builder.mutation<PaymentResponse, { invoiceId: string; data: RecordPaymentRequest }>({
      query: ({ invoiceId, data }) => ({ url: `/invoices/${invoiceId}/payments`, method: 'POST', body: data }),
      invalidatesTags: ['Payments', 'Invoices'],
    }),
    refundPayment: builder.mutation<PaymentResponse, { paymentId: string; notes?: string }>({
      query: ({ paymentId, notes }) => ({ url: `/payments/${paymentId}/refund`, method: 'POST', body: { notes } }),
      invalidatesTags: ['Payments', 'Invoices'],
    }),
  }),
});

export const {
  useGetPaymentsByInvoiceQuery,
  useRecordPaymentMutation,
  useRefundPaymentMutation,
} = paymentApi;
