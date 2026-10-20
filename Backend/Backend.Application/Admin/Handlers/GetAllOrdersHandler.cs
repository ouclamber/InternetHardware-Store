using MediatR;
using Backend.Application.Admin.DTOs;
using Backend.Application.Admin.Queries;
using Backend.Domain.Sales.OrderAggregate;  
using Backend.Domain.Sales.Repositories;
using Backend.Domain.Users.Repositories;

namespace Backend.Application.Admin.Handlers;

public class GetAllOrdersHandler : IRequestHandler<GetAllOrdersQuery, IReadOnlyList<AdminOrderDto>>
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

    public async Task<IReadOnlyList<AdminOrderDto>> Handle(
        GetAllOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetAllAsync(cancellationToken);

        // Загружаем всех пользователей один раз 
        var userIds = orders.Select(o => o.UserId).Distinct().ToList();
        var users = new Dictionary<int, string>();

        foreach (var userId in userIds)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user != null)
                users[userId] = user.UserName.Value;
        }

        return orders.Select(o => new AdminOrderDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber.Value,
            UserId = o.UserId,
            UserName = users.TryGetValue(o.UserId, out var name) ? name : "Неизвестно",
            Status = o.Status.ToCode(),                 // extension-метод
            StatusText = o.Status.ToRussianString(),    // extension-метод
            TotalAmount = o.TotalAmount.Amount,
            ItemsCount = o.Items.Count,
            TotalQuantity = o.Items.Sum(i => i.Quantity),
            CreatedAt = o.CreatedAt
        }).ToList();
    }
}