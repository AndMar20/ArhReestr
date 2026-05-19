namespace WebApp.ViewModels;

public record AdminDashboardStats(
    int TotalObjects,
    int ActiveObjects,
    int TotalUsers,
    int Agents,
    int Clients,
    int TotalInteractions,
    int ClosedDeals);
