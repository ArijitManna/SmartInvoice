import { api } from './api';

export interface CustomerResponse {
  id: string;
  name: string;
  email: string | null;
  phone: string | null;
  contactPerson: string | null;
  notes: string | null;
  gstin: string | null;
  pan: string | null;
  gstStateCode: string | null;
  billingStreet: string | null;
  billingCity: string | null;
  billingState: string | null;
  billingPostalCode: string | null;
  billingCountry: string | null;
  shippingStreet: string | null;
  shippingCity: string | null;
  shippingState: string | null;
  shippingPostalCode: string | null;
  shippingCountry: string | null;
  createdAt: string;
}

export interface CustomerRequest {
  name: string;
  email?: string;
  phone?: string;
  contactPerson?: string;
  notes?: string;
  gstin?: string;
  pan?: string;
  gstStateCode?: string;
  billingStreet?: string;
  billingCity?: string;
  billingState?: string;
  billingPostalCode?: string;
  billingCountry?: string;
  shippingStreet?: string;
  shippingCity?: string;
  shippingState?: string;
  shippingPostalCode?: string;
  shippingCountry?: string;
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

interface CustomerListParams {
  page?: number;
  pageSize?: number;
  search?: string;
  sortBy?: string;
  sortDesc?: boolean;
}

export const customerApi = api.injectEndpoints({
  endpoints: (builder) => ({
    getCustomers: builder.query<PagedResult<CustomerResponse>, CustomerListParams>({
      query: (params) => ({ url: '/customers', params }),
      providesTags: ['Customers'],
    }),
    getCustomer: builder.query<CustomerResponse, string>({
      query: (id) => `/customers/${id}`,
      providesTags: ['Customers'],
    }),
    createCustomer: builder.mutation<CustomerResponse, CustomerRequest>({
      query: (body) => ({ url: '/customers', method: 'POST', body }),
      invalidatesTags: ['Customers'],
    }),
    updateCustomer: builder.mutation<CustomerResponse, { id: string; data: CustomerRequest }>({
      query: ({ id, data }) => ({ url: `/customers/${id}`, method: 'PUT', body: data }),
      invalidatesTags: ['Customers'],
    }),
    deleteCustomer: builder.mutation<void, string>({
      query: (id) => ({ url: `/customers/${id}`, method: 'DELETE' }),
      invalidatesTags: ['Customers'],
    }),
  }),
});

export const {
  useGetCustomersQuery,
  useGetCustomerQuery,
  useCreateCustomerMutation,
  useUpdateCustomerMutation,
  useDeleteCustomerMutation,
} = customerApi;
