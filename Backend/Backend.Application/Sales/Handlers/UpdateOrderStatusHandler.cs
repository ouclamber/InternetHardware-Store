using MediatR;
using Backend.Application.Sales.Commands;
using Backend.Application.Sales.Services;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Sales.Repositories;
using Backend.Domain.Shared;

namespace Backend.Application.Sales.Handlers;

public class UpdateOrderStatusHandler : IRequestHandler<UpdateOrderStatusCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateOrderStatusHandler(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        UpdateOrderStatusCommand request,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
            throw new NotFoundException($"Заказ с Id={request.OrderId} не найден");

        switch ((request.NewStatus ?? string.Empty).Trim().ToLowerInvariant())
        {
            case "paid":
                order.MarkAsPaid();
                break;

            case "shipped":
                order.Ship();
                break;

            case "delivered":
                order.Deliver();
                break;

            case "cancelled":
                order.Cancel($"Отменён администратором (Id={request.AdminUserId})");
                await OrderStock.ReleaseAsync(order, _productRepository, cancellationToken);
                break;

            default:
                throw new DomainException($"Недопустимый статус: {request.NewStatus}");
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
