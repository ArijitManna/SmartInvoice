import { api } from './api';

export interface VendorRequest {
  name: string;
  email?: string;
  phone?: string;
  contactPerson?: string;
  notes?: string;
  gstin?: string;
  pan?: string;
  stateCode?: string;
  street?: string;
  city?: string;
  state?: string;
  postalCode?: string;
  country?: string;
  openingBalance?: number;
}

export interface VendorResponse extends VendorRequest {
  id: string;
  outstandingBalance: number;
  openingBalance: number;
  createdAt: string;
  updatedAt?: string;
}

export const vendorApi = api.injectEndpoints({
  endpoints: (builder) => ({
    getVendors: builder.query<{ items: VendorResponse[]; total: number }, { page?: number; pageSize?: number; search?: string }>({
      query: (params) => ({
        url: '/vendors',
        params,
      }),
      providesTags: ['Vendors'],
    }),

    getVendor: builder.query<VendorResponse, string>({
      query: (id) => `/vendors/${id}`,
      providesTags: ['Vendors'],
    }),

    createVendor: builder.mutation<VendorResponse, VendorRequest>({
      query: (data) => ({
        url: '/vendors',
        method: 'POST',
        body: data,
      }),
      invalidatesTags: ['Vendors'],
    }),

    updateVendor: builder.mutation<VendorResponse, { id: string; data: VendorRequest }>({
      query: ({ id, data }) => ({
        url: `/vendors/${id}`,
        method: 'PUT',
        body: data,
      }),
      invalidatesTags: ['Vendors'],
    }),

    deleteVendor: builder.mutation<void, string>({
      query: (id) => ({
        url: `/vendors/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['Vendors'],
    }),
  }),
});

export const {
  useGetVendorsQuery,
  useGetVendorQuery,
  useCreateVendorMutation,
  useUpdateVendorMutation,
  useDeleteVendorMutation,
} = vendorApi;
