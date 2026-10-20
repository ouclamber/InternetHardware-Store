using MediatR;
using Backend.Application.Sales.DTOs;
using Backend.Application.Sales.Queries;
using Backend.Domain.Sales.Repositories;
using Backend.Domain.Catalog.Entities;

namespace Backend.Application.Sales.Handlers;

public class GetCartHandler : IRequestHandler<GetCartQuery, CartDto?>
{
    private readonly ICartRepository _cartRepository;

    public GetCartHandler(ICartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    public async Task<CartDto?> Handle(
        GetCartQuery request,
        CancellationToken cancellationToken)
    {
        var cart = await _cartRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        if (cart == null || cart.IsEmpty)
            return new CartDto
            {
                UserId = request.UserId,
                Items = new List<CartItemDto>(),
                TotalQuantity = 0,
                TotalAmount = 0
            };

        return new CartDto
        {
            UserId = cart.UserId,
            Items = cart.Items.Select(MapItemToDto).ToList(),
            TotalQuantity = cart.TotalQuantity,
            TotalAmount = cart.TotalAmount.Amount
        };
    }

    private static CartItemDto MapItemToDto(Domain.Sales.CartAggregate.CartItem item)
    {
        return new CartItemDto
        {
            Id = item.Id,
            ProductId = item.ProductId,
            ProductName = item.Product?.Name.Value ?? "Товар",
            BrandName = item.Product?.Brand?.Name,
            MainImage = GetMainImage(item.Product),
            UnitPrice = item.UnitPrice.Amount,
            Quantity = item.Quantity.Value,
            TotalPrice = item.TotalPrice.Amount
        };
    }

    private static string? GetMainImage(Product? product)
    {
        if (product?.Images == null || !product.Images.Any())
            return null;

        var main = product.Images.FirstOrDefault(i => i.IsMain) ?? product.Images.First();
        return main.ImageUrl;
    }
}