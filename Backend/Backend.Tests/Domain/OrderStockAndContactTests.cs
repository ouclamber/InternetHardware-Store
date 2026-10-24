using Backend.Domain.Sales.OrderAggregate;
using Backend.Domain.Sales.ValueObjects;
using Backend.Domain.Shared;
using FluentAssertions;

namespace Backend.Tests.Domain;

public class OrderStockReservationTests
{
    private static Order CreateOrder() => new(
        1,
        Address.Create("ул. Ленина 1", "Москва"),
        new[]
        {
            (ProductId: 10, ProductName: "A", Quantity: 2, UnitPrice: Money.Rub(100)),
            (ProductId: 20, ProductName: "B", Quantity: 1, UnitPrice: Money.Rub(50))
        });

    [Fact]
    public void NewOrder_StockNotReserved()
    {
        CreateOrder().StockReserved.Should().BeFalse();
    }

    [Fact]
    public void MarkStockReserved_PendingOrder_SetsFlag()
    {
        var order = CreateOrder();

        order.MarkStockReserved();

        order.StockReserved.Should().BeTrue();
    }

    [Fact]
    public void MarkStockReserved_NotPending_Throws()
    {
        var order = CreateOrder();
        order.MarkAsPaid();

        var act = () => order.MarkStockReserved();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void TakeStockToRelease_NotCancelled_Throws()
    {
        var order = CreateOrder();
        order.MarkStockReserved();

        var act = () => order.TakeStockToRelease();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void TakeStockToRelease_Cancelled_ReturnsItemsOnce()
    {
        var order = CreateOrder();
        order.MarkStockReserved();
        order.Cancel();

        var first = order.TakeStockToRelease();
        var second = order.TakeStockToRelease();

        first.Should().HaveCount(2);
        first.Should().Contain((10, 2));
        first.Should().Contain((20, 1));
        second.Should().BeEmpty();
        order.StockReserved.Should().BeFalse();
    }

    [Fact]
    public void TakeStockToRelease_LegacyOrderWithoutReservation_ReturnsEmpty()
    {
        var order = CreateOrder();
        order.Cancel();

        order.TakeStockToRelease().Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithContact_StoresContact()
    {
        var contact = CustomerContact.Create("Иван", "Петров", "IVAN@Mail.ru", "+7 (900) 123-45-67");

        var order = new Order(
            1,
            Address.Create("ул. Ленина 1", "Москва"),
            new[] { (1, "A", 1, Money.Rub(100)) },
            contact: contact);

        order.Contact.Should().Be(contact);
        order.Contact!.FullName.Should().Be("Иван Петров");
    }
}

public class CustomerContactTests
{
    [Fact]
    public void Create_Valid_NormalizesEmailAndPhone()
    {
        var contact = CustomerContact.Create(" Иван ", "Петров", " IVAN@Mail.RU ", "+7 (900) 123-45-67");

        contact.FirstName.Should().Be("Иван");
        contact.Email.Should().Be("ivan@mail.ru");
        contact.Phone.Should().Be("+79001234567");
    }

    [Fact]
    public void Create_PhoneWithoutPlus_KeepsDigitsOnly()
    {
        CustomerContact.Create("A", "B", "a@b.co", "8 900 123 45 67").Phone.Should().Be("89001234567");
    }

    [Theory]
    [InlineData("", "Петров", "a@b.ru", "+79001234567")]
    [InlineData("Иван", " ", "a@b.ru", "+79001234567")]
    [InlineData("Иван", "Петров", "", "+79001234567")]
    [InlineData("Иван", "Петров", "not-an-email", "+79001234567")]
    [InlineData("Иван", "Петров", "a@b", "+79001234567")]
    [InlineData("Иван", "Петров", "a@b.ru", "")]
    [InlineData("Иван", "Петров", "a@b.ru", "12345")]
    [InlineData("Иван", "Петров", "a@b.ru", "+1234567890123456")]
    public void Create_Invalid_Throws(string first, string last, string email, string phone)
    {
        var act = () => CustomerContact.Create(first, last, email, phone);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_TooLongName_Throws()
    {
        var act = () => CustomerContact.Create(new string('a', CustomerContact.MaxNameLength + 1), "B", "a@b.ru", "+79001234567");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void ToString_DoesNotLeakPersonalData()
    {
        var contact = CustomerContact.Create("Иван", "Петров", "ivan@mail.ru", "+79001234567");

        contact.ToString().Should().NotContain("ivan");
        contact.ToString().Should().NotContain("7900");
    }

    [Fact]
    public void Equality_ByValue()
    {
        var a = CustomerContact.Create("Иван", "Петров", "ivan@mail.ru", "+79001234567");
        var b = CustomerContact.Create("Иван", "Петров", "IVAN@mail.ru", "+7 900 123 45 67");

        a.Should().Be(b);
    }
}
