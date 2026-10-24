using Backend.Application.Wishlist.Commands;
using Backend.Application.Wishlist.Handlers;
using Backend.Application.Wishlist.Queries;
using Backend.Domain.Catalog.Entities;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Shared;
using Backend.Domain.Wishlist;
using Backend.Domain.Wishlist.Repositories;
using Backend.Tests.TestHelpers;
using FluentAssertions;
using Moq;

namespace Backend.Tests.Wishlist;

public class WishlistHandlerTests
{
    private readonly Mock<IWishlistRepository> _wishlist = new();
    private readonly Mock<IProductRepository> _products = new();

    private AddToWishlistHandler AddHandler() => new(_wishlist.Object, _products.Object);

    [Fact]
    public async Task Add_NewProduct_Saves()
    {
        var product = TestData.Product(id: 5);
        _products.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        WishlistItem? saved = null;
        _wishlist.Setup(r => r.AddAsync(It.IsAny<WishlistItem>(), It.IsAny<CancellationToken>()))
                 .Callback<WishlistItem, CancellationToken>((w, _) => saved = w)
                 .Returns(Task.CompletedTask);

        await AddHandler().Handle(new AddToWishlistCommand(1, 5), CancellationToken.None);

        saved!.UserId.Should().Be(1);
        saved.ProductId.Should().Be(5);
    }

    [Fact]
    public async Task Add_AlreadyInWishlist_IsIdempotent()
    {
        _wishlist.Setup(r => r.ExistsAsync(1, 5, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await AddHandler().Handle(new AddToWishlistCommand(1, 5), CancellationToken.None);

        _wishlist.Verify(r => r.AddAsync(It.IsAny<WishlistItem>(), It.IsAny<CancellationToken>()), Times.Never);
        _products.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Add_UnknownProduct_ThrowsNotFound()
    {
        _products.Setup(r => r.GetByIdAsync(404, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        var act = async () => await AddHandler().Handle(new AddToWishlistCommand(1, 404), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Add_InactiveProduct_Throws()
    {
        _products.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(TestData.Product(id: 5, active: false));

        var act = async () => await AddHandler().Handle(new AddToWishlistCommand(1, 5), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task Add_LimitReached_Throws()
    {
        _products.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(TestData.Product(id: 5));
        _wishlist.Setup(r => r.CountByUserAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(WishlistItem.MaxItemsPerUser);

        var act = async () => await AddHandler().Handle(new AddToWishlistCommand(1, 5), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage($"*{WishlistItem.MaxItemsPerUser}*");
        _wishlist.Verify(r => r.AddAsync(It.IsAny<WishlistItem>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Remove_CallsRepository_EvenIfMissing()
    {
        _wishlist.Setup(r => r.RemoveAsync(1, 5, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await new RemoveFromWishlistHandler(_wishlist.Object).Handle(new RemoveFromWishlistCommand(1, 5), CancellationToken.None);

        _wishlist.Verify(r => r.RemoveAsync(1, 5, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Get_MapsProducts_AndSkipsMissing()
    {
        var withProduct = new WishlistItem(1, TestData.Product(id: 3, name: "Pixel", stock: 0));
        _wishlist.Setup(r => r.GetByUserAsync(1, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new[] { withProduct });

        var result = await new GetWishlistHandler(_wishlist.Object).Handle(new GetWishlistQuery(1), CancellationToken.None);

        result.Should().ContainSingle();
        result[0].ProductId.Should().Be(3);
        result[0].Product.Name.Should().Be("Pixel");
        result[0].Product.InStock.Should().BeFalse();
    }

    [Fact]
    public async Task GetIds_ReturnsRepositoryIds()
    {
        _wishlist.Setup(r => r.GetProductIdsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new[] { 3, 1 });

        var ids = await new GetWishlistHandler(_wishlist.Object).Handle(new GetWishlistProductIdsQuery(1), CancellationToken.None);

        ids.Should().Equal(3, 1);
    }

    [Fact]
    public async Task Add_ConcurrentDuplicate_IsStillIdempotent()
    {
        // Два параллельных запроса прошли проверку ExistsAsync; второй упирается в уникальный индекс
        _products.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(TestData.Product(id: 5));
        _wishlist.Setup(r => r.AddAsync(It.IsAny<WishlistItem>(), It.IsAny<CancellationToken>()))
                 .ThrowsAsync(new ConflictException("duplicate"));

        var act = async () => await AddHandler().Handle(new AddToWishlistCommand(1, 5), CancellationToken.None);

        await act.Should().NotThrowAsync();
    }
}
