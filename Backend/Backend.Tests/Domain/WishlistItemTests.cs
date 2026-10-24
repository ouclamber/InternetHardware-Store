using Backend.Domain.Shared;
using Backend.Domain.Wishlist;
using Backend.Tests.TestHelpers;
using FluentAssertions;

namespace Backend.Tests.Domain;

public class WishlistItemTests
{
    [Fact]
    public void Create_Valid_SetsFields()
    {
        var product = TestData.Product(id: 5);

        var item = new WishlistItem(3, product);

        item.UserId.Should().Be(3);
        item.ProductId.Should().Be(5);
        item.Product.Should().BeSameAs(product);
        item.AddedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_InvalidUser_Throws()
    {
        var act = () => new WishlistItem(0, TestData.Product());

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_NullProduct_Throws()
    {
        var act = () => new WishlistItem(1, null!);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_InactiveProduct_Throws()
    {
        var act = () => new WishlistItem(1, TestData.Product(active: false));

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_OutOfStockProduct_IsAllowed()
    {
        // Отложить можно и отсутствующий товар — пользователь ждёт поступления
        var item = new WishlistItem(1, TestData.Product(stock: 0));

        item.ProductId.Should().Be(1);
    }
}
