using SmartInvoice.Application.Dashboard.DTOs;

namespace SmartInvoice.Application.Dashboard;

public interface IDashboardService
{
    Task<DashboardResponse> GetDashboardAsync();
}
