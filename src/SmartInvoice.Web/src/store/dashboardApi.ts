import { api } from './api';

export interface DashboardResponse {
  todaySales: number;
  outstandingAmount: number;
  invoicesCreatedThisMonth: number;
  paidInvoices: number;
  pendingInvoices: number;
  overdueInvoices: number;
  gstCollected: number;
  monthlyRevenue: { month: string; amount: number }[];
  topCustomers: { customerId: string; name: string; totalAmount: number }[];
  recentPayments: { invoiceId: string; invoiceNumber: string; customerName: string; amount: number; paymentDate: string }[];
  upcomingDueInvoices: { invoiceId: string; invoiceNumber: string; customerName: string; balanceDue: number; dueDate: string }[];
}

export interface SalesReportResponse {
  totalSales: number;
  totalInvoices: number;
  totalTax: number;
  items: { invoiceId: string; invoiceNumber: string; invoiceDate: string; customerName: string; subTotal: number; taxAmount: number; totalAmount: number; status: string }[];
}

export interface GstReportResponse {
  totalTaxCollected: number;
  totalCgst: number;
  totalSgst: number;
  totalIgst: number;
  items: { invoiceId: string; invoiceNumber: string; invoiceDate: string; customerName: string; customerGstin: string | null; taxableAmount: number; cgstAmount: number; sgstAmount: number; igstAmount: number; totalTax: number }[];
}

export interface OutstandingReportResponse {
  totalOutstanding: number;
  totalInvoices: number;
  items: { invoiceId: string; invoiceNumber: string; invoiceDate: string; dueDate: string | null; customerName: string; totalAmount: number; amountPaid: number; balanceDue: number; daysOverdue: number }[];
}

export const dashboardApi = api.injectEndpoints({
  endpoints: (builder) => ({
    getDashboard: builder.query<DashboardResponse, void>({
      query: () => '/dashboard',
      providesTags: ['Dashboard'],
    }),
    getSalesReport: builder.query<SalesReportResponse, { from: string; to: string; customerId?: string }>({
      query: (params) => ({ url: '/reports/sales', params }),
    }),
    getGstReport: builder.query<GstReportResponse, { from: string; to: string }>({
      query: (params) => ({ url: '/reports/gst', params }),
    }),
    getOutstandingReport: builder.query<OutstandingReportResponse, void>({
      query: () => '/reports/outstanding',
    }),
  }),
});

export const {
  useGetDashboardQuery,
  useGetSalesReportQuery,
  useGetGstReportQuery,
  useGetOutstandingReportQuery,
} = dashboardApi;
