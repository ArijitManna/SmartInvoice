import { api } from './api';

export interface PurchaseOrderRequest {
  vendorId: string;
  orderDate: string;
  expectedDeliveryDate?: string;
  notes?: string;
}

export interface PurchaseOrderResponse extends PurchaseOrderRequest {
  id: string;
  orderNumber: string;
  status: string;
  vendorName: string;
  totalAmount: number;
  createdAt: string;
}

export interface PurchaseBillRequest {
  vendorId: string;
  billNumber: string;
  billDate: string;
  dueDate?: string;
  amount: number;
  notes?: string;
}

export interface PurchaseBillResponse extends PurchaseBillRequest {
  id: string;
  status: string;
  vendorName: string;
  createdAt: string;
}

export const purchaseApi = api.injectEndpoints({
  endpoints: (builder) => ({
    getPurchaseOrders: builder.query<{ items: PurchaseOrderResponse[]; total: number }, { page?: number; pageSize?: number }>({
      query: (params) => ({ url: '/purchase-orders', params }),
      providesTags: ['PurchaseOrders'],
    }),

    createPurchaseOrder: builder.mutation<PurchaseOrderResponse, PurchaseOrderRequest>({
      query: (data) => ({ url: '/purchase-orders', method: 'POST', body: data }),
      invalidatesTags: ['PurchaseOrders'],
    }),

    updatePurchaseOrder: builder.mutation<PurchaseOrderResponse, { id: string; data: PurchaseOrderRequest }>({
      query: ({ id, data }) => ({ url: `/purchase-orders/${id}`, method: 'PUT', body: data }),
      invalidatesTags: ['PurchaseOrders'],
    }),

    deletePurchaseOrder: builder.mutation<void, string>({
      query: (id) => ({ url: `/purchase-orders/${id}`, method: 'DELETE' }),
      invalidatesTags: ['PurchaseOrders'],
    }),

    getPurchaseBills: builder.query<{ items: PurchaseBillResponse[]; total: number }, { page?: number; pageSize?: number }>({
      query: (params) => ({ url: '/purchase-bills', params }),
      providesTags: ['PurchaseBills'],
    }),

    createPurchaseBill: builder.mutation<PurchaseBillResponse, PurchaseBillRequest>({
      query: (data) => ({ url: '/purchase-bills', method: 'POST', body: data }),
      invalidatesTags: ['PurchaseBills'],
    }),

    updatePurchaseBill: builder.mutation<PurchaseBillResponse, { id: string; data: PurchaseBillRequest }>({
      query: ({ id, data }) => ({ url: `/purchase-bills/${id}`, method: 'PUT', body: data }),
      invalidatesTags: ['PurchaseBills'],
    }),

    deletePurchaseBill: builder.mutation<void, string>({
      query: (id) => ({ url: `/purchase-bills/${id}`, method: 'DELETE' }),
      invalidatesTags: ['PurchaseBills'],
    }),
  }),
});

export const {
  useGetPurchaseOrdersQuery,
  useCreatePurchaseOrderMutation,
  useUpdatePurchaseOrderMutation,
  useDeletePurchaseOrderMutation,
  useGetPurchaseBillsQuery,
  useCreatePurchaseBillMutation,
  useUpdatePurchaseBillMutation,
  useDeletePurchaseBillMutation,
} = purchaseApi;
