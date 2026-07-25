import { api } from './api';

export interface CompanyResponse {
  id: string;
  name: string;
  phone: string | null;
  email: string | null;
  website: string | null;
  logoUrl: string | null;
  signatureUrl: string | null;
  defaultCurrency: string;
  timeZone: string;
  invoicePrefix: string | null;
  nextInvoiceNumber: number;
  street: string | null;
  city: string | null;
  state: string | null;
  postalCode: string | null;
  country: string | null;
  gstin: string | null;
  pan: string | null;
  gstStateCode: string | null;
  bankName: string | null;
  accountNumber: string | null;
  ifscCode: string | null;
  accountHolderName: string | null;
  branchName: string | null;
  upiId: string | null;
}

export interface CreateCompanyRequest {
  name: string;
  phone?: string;
  email?: string;
  website?: string;
  defaultCurrency?: string;
  timeZone?: string;
  invoicePrefix?: string;
  street?: string;
  city?: string;
  state?: string;
  postalCode?: string;
  country?: string;
  gstin?: string;
  pan?: string;
  gstStateCode?: string;
  bankName?: string;
  accountNumber?: string;
  ifscCode?: string;
  accountHolderName?: string;
  branchName?: string;
  upiId?: string;
}

export interface UpdateCompanyRequest extends CreateCompanyRequest {
  logoUrl?: string;
  signatureUrl?: string;
}

export const companyApi = api.injectEndpoints({
  endpoints: (builder) => ({
    getCurrentCompany: builder.query<CompanyResponse, void>({
      query: () => '/companies/current',
      providesTags: ['Company'],
    }),
    createCompany: builder.mutation<{ company: CompanyResponse }, CreateCompanyRequest>({
      query: (body) => ({ url: '/companies', method: 'POST', body }),
      invalidatesTags: ['Company'],
    }),
    updateCompany: builder.mutation<CompanyResponse, UpdateCompanyRequest>({
      query: (body) => ({ url: '/companies/current', method: 'PUT', body }),
      invalidatesTags: ['Company'],
    }),
  }),
});

export const {
  useGetCurrentCompanyQuery,
  useCreateCompanyMutation,
  useUpdateCompanyMutation,
} = companyApi;
