using MediatR;
using Backend.Application.Admin.DTOs;
using Backend.Application.Admin.Queries;
using Backend.Domain.Sales.OrderAggregate;
using Backend.Domain.Sales.Repositories;
using Backend.Domain.Shared;
using Backend.Domain.Users.Repositories;

namespace Backend.Application.Admin.Handlers;

public class GetAllOrdersHandler : IRequestHandler<GetAllOrdersQuery, PagedResult<AdminOrderDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;

    public GetAllOrdersHandler(
        IOrderRepository orderRepository,
        IUserRepository userRepository)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
    }

    public async Task<PagedResult<AdminOrderDto>> Handle(
        GetAllOrdersQuery request,
        CancellationToken cancellationToken)
    {
        OrderStatus? status = null;
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            try
            {
                status = OrderStatusExtensions.FromCode(request.Status.Trim());
            }
            catch (ArgumentException)
            {
                throw new DomainException($"Недопустимый статус: {request.Status}");
            }
        }

        var (page, pageSize) = Paging.Normalize(request.Page, request.PageSize);

        var orders = await _orderRepository.GetPagedAsync(status, page, pageSize, cancellationToken);

        // Имена пользователей — одним запросом (раньше был отдельный запрос на каждого: N+1)
        var userIds = orders.Items.Select(o => o.UserId).Distinct().ToList();
        var users = userIds.Count == 0
            ? new Dictionary<int, string>()
            : await _userRepository.GetUserNamesAsync(userIds, cancellationToken);

        return orders.Map(o => new AdminOrderDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber.Value,
            UserId = o.UserId,
            UserName = users.TryGetValue(o.UserId, out var name) ? name : "Неизвестно",
            CustomerName = o.Contact?.FullName,
            Status = o.Status.ToCode(),
            StatusText = o.Status.ToRussianString(),
            TotalAmount = o.TotalAmount.Amount,
            ItemsCount = o.Items.Count,
            TotalQuantity = o.Items.Sum(i => i.Quantity),
            CreatedAt = o.CreatedAt
        });
    }
}
