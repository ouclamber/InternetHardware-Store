namespace Backend.Application.Admin.DTOs;

public class AdminStatsDto
{
    // Пользователи
    public int TotalUsers { get; set; }
    public int TotalAdmins { get; set; }
    public int TotalRegularUsers { get; set; }

    // Товары
    public int TotalProducts { get; set; }
    public int ActiveProducts { get; set; }
    public int InactiveProducts { get; set; }

    // Заказы
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
}