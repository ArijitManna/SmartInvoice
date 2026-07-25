import { api } from './api';

export interface InvoiceItemResponse {
  id: string;
  productId: string | null;
  productName: string | null;
  description: string;
  hsnSacCode: string | null;
  quantity: number;
  unit: string;
  rate: number;
  discountPercentage: number;
  discountAmount: number;
  taxRate: number;
  taxAmount: number;
  amount: number;
}

export interface InvoiceResponse {
  id: string;
  invoiceNumber: string;
  type: number;
  status: number;
  invoiceDate: string;
  dueDate: string | null;
  customerId: string;
  customerName: string;
  subTotal: number;
  discountPercentage: number;
  discountAmount: number;
  taxAmount: number;
  cgstAmount: number;
  sgstAmount: number;
  igstAmount: number;
  totalAmount: number;
  amountPaid: number;
  balanceDue: number;
  currency: string;
  gstType: number;
  notes: string | null;
  termsAndConditions: string | null;
  referenceNumber: string | null;
  items: InvoiceItemResponse[];
  createdAt: string;
}

export interface InvoiceListItem {
  id: string;
  invoiceNumber: string;
  type: number;
  status: number;
  invoiceDate: string;
  dueDate: string | null;
  customerId: string;
  customerName: string;
  totalAmount: number;
  balanceDue: number;
  currency: string;
  createdAt: string;
}

export interface InvoiceItemRequest {
  productId?: string | null;
  description: string;
  hsnSacCode?: string;
  quantity: number;
  unit: string;
  rate: number;
  discountPercentage: number;
  taxRate: number;
}

export interface CreateInvoiceRequest {
  customerId: string;
  type: number;
  dueDate?: string;
  discountPercentage: number;
  notes?: string;
  termsAndConditions?: string;
  referenceNumber?: string;
  items: InvoiceItemRequest[];
}

export interface UpdateInvoiceRequest {
  customerId: string;
  dueDate?: string;
  discountPercentage: number;
  notes?: string;
  termsAndConditions?: string;
  referenceNumber?: string;
  items: InvoiceItemRequest[];
}

interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

interface InvoiceListParams {
  page?: number;
  pageSize?: number;
  status?: number;
  customerId?: string;
  from?: string;
  to?: string;
  sortBy?: string;
  sortDesc?: boolean;
}

export const invoiceApi = api.injectEndpoints({
  endpoints: (builder) => ({
    getInvoices: builder.query<PagedResult<InvoiceListItem>, InvoiceListParams>({
      query: (params) => ({ url: '/invoices', params }),
      providesTags: ['Invoices'],
    }),
    getInvoice: builder.query<InvoiceResponse, string>({
      query: (id) => `/invoices/${id}`,
      providesTags: ['Invoices'],
    }),
    createInvoice: builder.mutation<InvoiceResponse, CreateInvoiceRequest>({
      query: (body) => ({ url: '/invoices', method: 'POST', body }),
      invalidatesTags: ['Invoices'],
    }),
    updateInvoice: builder.mutation<InvoiceResponse, { id: string; data: UpdateInvoiceRequest }>({
      query: ({ id, data }) => ({ url: `/invoices/${id}`, method: 'PUT', body: data }),
      invalidatesTags: ['Invoices'],
    }),
    duplicateInvoice: builder.mutation<InvoiceResponse, string>({
      query: (id) => ({ url: `/invoices/${id}/duplicate`, method: 'POST' }),
      invalidatesTags: ['Invoices'],
    }),
    deleteInvoice: builder.mutation<void, string>({
      query: (id) => ({ url: `/invoices/${id}`, method: 'DELETE' }),
      invalidatesTags: ['Invoices'],
    }),
    sendInvoice: builder.mutation<{ message: string }, { id: string; email?: string }>({
      query: ({ id, email }) => ({ url: `/invoices/${id}/send`, method: 'POST', body: { email } }),
      invalidatesTags: ['Invoices'],
    }),
    getInvoicePdfUrl: builder.query<string, string>({
      query: (id) => ({ url: `/invoices/${id}/pdf`, responseHandler: 'text' }),
    }),
  }),
});

export const {
  useGetInvoicesQuery,
  useGetInvoiceQuery,
  useCreateInvoiceMutation,
  useUpdateInvoiceMutation,
  useDuplicateInvoiceMutation,
  useDeleteInvoiceMutation,
  useSendInvoiceMutation,
} = invoiceApi;
