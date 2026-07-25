import { api } from './api';

export interface ProductResponse {
  id: string;
  name: string;
  description: string | null;
  type: number;
  sku: string | null;
  hsnSacCode: string | null;
  unit: string;
  price: number;
  taxRate: number;
  categoryId: string | null;
  categoryName: string | null;
  createdAt: string;
}

export interface ProductRequest {
  name: string;
  description?: string;
  type: number;
  sku?: string;
  hsnSacCode?: string;
  unit: string;
  price: number;
  taxRate: number;
  categoryId?: string | null;
}

export interface CategoryResponse {
  id: string;
  name: string;
  description: string | null;
  parentCategoryId: string | null;
  parentCategoryName: string | null;
}

export interface CategoryRequest {
  name: string;
  description?: string;
  parentCategoryId?: string | null;
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

interface ProductListParams {
  page?: number;
  pageSize?: number;
  search?: string;
  categoryId?: string;
  sortBy?: string;
  sortDesc?: boolean;
}

export const productApi = api.injectEndpoints({
  endpoints: (builder) => ({
    getProducts: builder.query<PagedResult<ProductResponse>, ProductListParams>({
      query: (params) => ({ url: '/products', params }),
      providesTags: ['Products'],
    }),
    createProduct: builder.mutation<ProductResponse, ProductRequest>({
      query: (body) => ({ url: '/products', method: 'POST', body }),
      invalidatesTags: ['Products'],
    }),
    updateProduct: builder.mutation<ProductResponse, { id: string; data: ProductRequest }>({
      query: ({ id, data }) => ({ url: `/products/${id}`, method: 'PUT', body: data }),
      invalidatesTags: ['Products'],
    }),
    deleteProduct: builder.mutation<void, string>({
      query: (id) => ({ url: `/products/${id}`, method: 'DELETE' }),
      invalidatesTags: ['Products'],
    }),
    getCategories: builder.query<CategoryResponse[], void>({
      query: () => '/categories',
      providesTags: ['Categories'],
    }),
    createCategory: builder.mutation<CategoryResponse, CategoryRequest>({
      query: (body) => ({ url: '/categories', method: 'POST', body }),
      invalidatesTags: ['Categories'],
    }),
  }),
});

export const {
  useGetProductsQuery,
  useCreateProductMutation,
  useUpdateProductMutation,
  useDeleteProductMutation,
  useGetCategoriesQuery,
  useCreateCategoryMutation,
} = productApi;
