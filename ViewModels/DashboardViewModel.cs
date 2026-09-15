using MiniErp.Models;

namespace MiniErp.ViewModels;

public class DashboardViewModel
{
    public int ActiveCustomers { get; init; }
    public int ActiveProducts { get; init; }
    public int OpenOrders { get; init; }
    public decimal SalesTotal { get; init; }
    public IReadOnlyCollection<SalesOrder> RecentOrders { get; init; } = [];
}
