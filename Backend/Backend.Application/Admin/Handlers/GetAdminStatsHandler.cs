using MediatR;
using Backend.Application.Admin.DTOs;
using Backend.Application.Admin.Queries;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Reviews.Repositories;
using Backend.Domain.Sales.Repositories;
using Backend.Domain.Users;
using Backend.Domain.Users.Repositories;

namespace Backend.Application.Admin.Handlers;

public class GetAdminStatsHandler : IRequestHandler<GetAdminStatsQuery, AdminStatsDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IReviewRepository _reviewRepository;

    public GetAdminStatsHandler(
        IUserRepository userRepository,
        IProductRepository productRepository,
        IOrderRepository orderRepository,
        IReviewRepository reviewRepository)
    {
        _userRepository = userRepository;
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _reviewRepository = reviewRepository;
    }

    public async Task<AdminStatsDto> Handle(
        GetAdminStatsQuery request,
        CancellationToken cancellationToken)
    {
        // Пользователи
        var totalUsers = await _userRepository.CountAsync(cancellationToken);
        var totalAdmins = await _userRepository.CountByRoleAsync(Role.Admin, cancellationToken);
        var totalRegularUsers = totalUsers - totalAdmins;

        // Товары
        var totalProducts = await _productRepository.CountAsync(cancellationToken);
        var activeProducts = await _productRepository.CountActiveAsync(cancellationToken);
        var inactiveProducts = totalProducts - activeProducts;
        var outOfStockProducts = await _productRepository.CountOutOfStockAsync(cancellationToken);

        // Заказы
        var totalOrders = await _orderRepository.CountAsync(cancellationToken);
        var totalRevenue = await _orderRepository.SumRevenueAsync(cancellationToken);

        // Отзывы
        var pendingReviews = await _reviewRepository.CountPendingAsync(cancellationToken);

        return new AdminStatsDto
        {
            TotalUsers = totalUsers,
            TotalAdmins = totalAdmins,
            TotalRegularUsers = totalRegularUsers,

            TotalProducts = totalProducts,
            ActiveProducts = activeProducts,
            InactiveProducts = inactiveProducts,
            OutOfStockProducts = outOfStockProducts,

            TotalOrders = totalOrders,
            TotalRevenue = totalRevenue,

            PendingReviews = pendingReviews
        };
    }
}