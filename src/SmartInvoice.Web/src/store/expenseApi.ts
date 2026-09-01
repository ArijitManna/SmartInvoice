import { api } from './api';

export interface ExpenseCategoryRequest {
  name: string;
  description?: string;
}

export interface ExpenseCategoryResponse extends ExpenseCategoryRequest {
  id: string;
  createdAt: string;
}

export interface ExpenseRequest {
  categoryId: string;
  amount: number;
  description?: string;
  date: string;
  paymentMethod?: string;
}

export interface ExpenseResponse extends ExpenseRequest {
  id: string;
  categoryName: string;
  createdAt: string;
}

export const expenseApi = api.injectEndpoints({
  endpoints: (builder) => ({
    getExpenseCategories: builder.query<ExpenseCategoryResponse[], void>({
      query: () => '/expense-categories',
      providesTags: ['ExpenseCategories'],
    }),

    createExpenseCategory: builder.mutation<ExpenseCategoryResponse, ExpenseCategoryRequest>({
      query: (data) => ({
        url: '/expense-categories',
        method: 'POST',
        body: data,
      }),
      invalidatesTags: ['ExpenseCategories'],
    }),

    getExpenses: builder.query<{ items: ExpenseResponse[]; total: number }, { page?: number; pageSize?: number; from?: string; to?: string }>({
      query: (params) => ({
        url: '/expenses',
        params,
      }),
      providesTags: ['Expenses'],
    }),

    createExpense: builder.mutation<ExpenseResponse, ExpenseRequest>({
      query: (data) => ({
        url: '/expenses',
        method: 'POST',
        body: data,
      }),
      invalidatesTags: ['Expenses'],
    }),

    updateExpense: builder.mutation<ExpenseResponse, { id: string; data: ExpenseRequest }>({
      query: ({ id, data }) => ({
        url: `/expenses/${id}`,
        method: 'PUT',
        body: data,
      }),
      invalidatesTags: ['Expenses'],
    }),

    deleteExpense: builder.mutation<void, string>({
      query: (id) => ({
        url: `/expenses/${id}`,
        method: 'DELETE',
      }),
      invalidatesTags: ['Expenses'],
    }),

    getExpenseSummary: builder.query<{ totalAmount: number; byCategory: { category: string; amount: number }[] }, { from?: string; to?: string }>({
      query: (params) => ({
        url: '/expenses/summary',
        params,
      }),
      providesTags: ['Expenses'],
    }),
  }),
});

export const {
  useGetExpenseCategoriesQuery,
  useCreateExpenseCategoryMutation,
  useGetExpensesQuery,
  useCreateExpenseMutation,
  useUpdateExpenseMutation,
  useDeleteExpenseMutation,
  useGetExpenseSummaryQuery,
} = expenseApi;
