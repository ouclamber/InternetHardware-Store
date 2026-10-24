using MediatR;
using Backend.Application.Sales.Commands;
using Backend.Application.Sales.Services;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Sales.Repositories;
using Backend.Domain.Shared;

namespace Backend.Application.Sales.Handlers;

public class CancelOrderHandler : IRequestHandler<CancelOrderCommand>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelOrderHandler(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        CancelOrderCommand request,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
            throw new NotFoundException($"Заказ с Id={request.OrderId} не найден");

        if (order.UserId != request.UserId)
            throw new ForbiddenException("Нет доступа к этому заказу");

        order.Cancel("Отменён клиентом");

        await OrderStock.ReleaseAsync(order, _productRepository, cancellationToken);

        // Отмена заказа и возврат остатков — одной транзакцией
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
