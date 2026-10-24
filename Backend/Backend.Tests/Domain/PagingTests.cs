using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Shared;
using FluentAssertions;

namespace Backend.Tests.Domain;

public class PagedResultTests
{
    [Fact]
    public void TotalPages_IsCeiling()
    {
        var page = new PagedResult<int>(new[] { 1, 2 }, totalCount: 21, page: 1, pageSize: 10);

        page.TotalPages.Should().Be(3);
        page.HasNextPage.Should().BeTrue();
        page.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public void LastPage_HasNoNext()
    {
        var page = new PagedResult<int>(new[] { 1 }, totalCount: 21, page: 3, pageSize: 10);

        page.HasNextPage.Should().BeFalse();
        page.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    public void Empty_HasZeroPages()
    {
        var page = PagedResult<string>.Empty();

        page.Items.Should().BeEmpty();
        page.TotalPages.Should().Be(0);
        page.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public void Map_KeepsPagingInfo()
    {
        var page = new PagedResult<int>(new[] { 1, 2 }, 12, 2, 2).Map(x => x * 10);

        page.Items.Should().Equal(10, 20);
        page.TotalCount.Should().Be(12);
        page.Page.Should().Be(2);
        page.PageSize.Should().Be(2);
    }

    [Theory]
    [InlineData(0, 0, 1, Paging.DefaultPageSize)]
    [InlineData(-5, -1, 1, Paging.DefaultPageSize)]
    [InlineData(3, 10, 3, 10)]
    [InlineData(1, 100_000, 1, Paging.MaxPageSize)]
    public void Paging_Normalize(int page, int size, int expectedPage, int expectedSize)
    {
        var (p, s) = Paging.Normalize(page, size);

        p.Should().Be(expectedPage);
        s.Should().Be(expectedSize);
    }
}

public class ProductQueryTests
{
    [Fact]
    public void Normalize_TrimsSearch_AndDropsEmpty()
    {
        new ProductQuery { Search = "  macbook  " }.Normalize().Search.Should().Be("macbook");
        new ProductQuery { Search = "   " }.Normalize().Search.Should().BeNull();
    }

    [Fact]
    public void Normalize_CutsLongSearch()
    {
        var q = new ProductQuery { Search = new string('a', 500) }.Normalize();

        q.Search!.Length.Should().Be(ProductQuery.MaxSearchLength);
    }

    [Fact]
    public void Normalize_MinGreaterThanMax_Throws()
    {
        var act = () => new ProductQuery { MinPrice = 100, MaxPrice = 10 }.Normalize();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Normalize_NegativePrice_Throws()
    {
        var act = () => new ProductQuery { MinPrice = -1 }.Normalize();

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Normalize_CleansIdsAndPaging()
    {
        var q = new ProductQuery
        {
            CategoryId = 0,
            TypeId = -2,
            BrandIds = new[] { 3, 3, 0, -1, 5 },
            Page = 0,
            PageSize = 1000
        }.Normalize();

        q.CategoryId.Should().BeNull();
        q.TypeId.Should().BeNull();
        q.BrandIds.Should().BeEquivalentTo(new[] { 3, 5 });
        q.Page.Should().Be(1);
        q.PageSize.Should().Be(Paging.MaxPageSize);
    }

    [Fact]
    public void Normalize_EmptyBrandList_BecomesNull()
    {
        new ProductQuery { BrandIds = new[] { 0 } }.Normalize().BrandIds.Should().BeNull();
    }

    [Fact]
    public void Normalize_UndefinedSort_FallsBackToDefault()
    {
        new ProductQuery { Sort = (ProductSort)42 }.Normalize().Sort.Should().Be(ProductSort.Default);
    }
}
