using Backend.Domain.Catalog.Entities;
using Backend.Domain.Catalog.ValueObjects;
using Backend.Domain.Reviews;
using Backend.Domain.Reviews.ValueObjects;
using Backend.Domain.Shared;
using Backend.Domain.Users;
using Backend.Domain.Users.ValueObjects;
using Backend.Infrastructure.Persistence;
using Backend.Infrastructure.Repositories;
using Backend.Tests.Persistence;
using FluentAssertions;

namespace Backend.Tests.Repositories;

public class ReviewRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly ReviewRepository _repo;
    private readonly int _userId;
    private readonly int _productId;

    public ReviewRepositoryTests()
    {
        _context = TestDbContextFactory.CreateInMemory();
        _repo = new ReviewRepository(_context);

        // Создаём User
        var user = new User(
            UserName.Create("testuser"),
            PasswordHash.FromHash(new string('a', 60)),
            Role.User);
        _context.Users.Add(user);

        // Создаём Brand, Category, Type, Product
        var brand = new Brand("TestBrand");
        var category = new Category("TestCategory");
        var type = new ProductType("TestType");
        _context.Brands.Add(brand);
        _context.Categories.Add(category);
        _context.Types.Add(type);
        _context.SaveChanges();

        var product = new Product(
            ProductName.Create("Test Product"),
            Money.Rub(1000),
            brand.Id,
            category.Id,
            type.Id);
        _context.Products.Add(product);
        _context.SaveChanges();

        _userId = user.Id;
        _productId = product.Id;
    }

    public void Dispose() => _context.Dispose();

    private Review CreateReview(int? userId = null, int? productId = null, int rating = 5, bool autoApprove = true)
    {
        return new Review(
            userId ?? _userId,
            productId ?? _productId,
            Rating.Of(rating),
            Comment.Create("Отличный товар, всё понравилось!"),
            autoApprove);
    }

    [Fact]
    public async Task AddAsync_ValidReview_Saves()
    {
        await _repo.AddAsync(CreateReview());
        _context.Reviews.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByIdAsync_Existing_ReturnsReview()
    {
        var review = CreateReview();
        await _repo.AddAsync(review);

        var found = await _repo.GetByIdAsync(review.Id);

        found.Should().NotBeNull();
        found!.UserId.Should().Be(_userId);
    }

    [Fact]
    public async Task GetByIdAsync_NonExisting_ReturnsNull()
    {
        var found = await _repo.GetByIdAsync(999);
        found.Should().BeNull();
    }

    [Fact]
    public async Task GetByProductAsync_ReturnsApprovedReviews()
    {
        await _repo.AddAsync(CreateReview(autoApprove: true));
        await _repo.AddAsync(CreateReview(autoApprove: true));
        await _repo.AddAsync(CreateReview(autoApprove: false));

        var found = await _repo.GetByProductAsync(_productId);

        found.Should().HaveCount(2);
        found.Should().OnlyContain(r => r.IsApproved);
    }

    [Fact]
    public async Task GetByUserAsync_ReturnsUserReviews()
    {
        await _repo.AddAsync(CreateReview());
        await _repo.AddAsync(CreateReview());

        var found = await _repo.GetByUserAsync(_userId);

        found.Should().HaveCount(2);
    }

    [Fact]
    public async Task ExistsAsync_Existing_ReturnsTrue()
    {
        await _repo.AddAsync(CreateReview());

        var exists = await _repo.ExistsAsync(_userId, _productId);
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_NonExisting_ReturnsFalse()
    {
        var exists = await _repo.ExistsAsync(999, 999);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task GetByUserAndProductAsync_Existing_ReturnsReview()
    {
        await _repo.AddAsync(CreateReview());

        var found = await _repo.GetByUserAndProductAsync(_userId, _productId);

        found.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByUserAndProductAsync_NonExisting_ReturnsNull()
    {
        var found = await _repo.GetByUserAndProductAsync(999, 999);
        found.Should().BeNull();
    }

    [Fact]
    public async Task GetCountByProductAsync_CountsApproved()
    {
        await _repo.AddAsync(CreateReview(autoApprove: true));
        await _repo.AddAsync(CreateReview(autoApprove: true));
        await _repo.AddAsync(CreateReview(autoApprove: false));

        var count = await _repo.GetCountByProductAsync(_productId);

        count.Should().Be(2);
    }

    [Fact]
    public async Task GetAverageRatingAsync_ReturnsAverage()
    {
        await _repo.AddAsync(CreateReview(rating: 5, autoApprove: true));
        await _repo.AddAsync(CreateReview(rating: 3, autoApprove: true));
        await _repo.AddAsync(CreateReview(rating: 4, autoApprove: true));

        var avg = await _repo.GetAverageRatingAsync(_productId);

        avg.Should().BeApproximately(4.0, 0.01);
    }

    [Fact]
    public async Task GetAverageRatingAsync_NoReviews_ReturnsZero()
    {
        var avg = await _repo.GetAverageRatingAsync(999);
        avg.Should().Be(0);
    }

    [Fact]
    public async Task GetPendingAsync_ReturnsOnlyPending()
    {
        await _repo.AddAsync(CreateReview(autoApprove: false));
        await _repo.AddAsync(CreateReview(autoApprove: false));
        await _repo.AddAsync(CreateReview(autoApprove: true));

        var pending = await _repo.GetPendingAsync();

        pending.Should().HaveCount(2);
        pending.Should().OnlyContain(r => r.IsPending);
    }

    [Fact]
    public async Task UpdateAsync_ModifiesReview()
    {
        var review = CreateReview(rating: 5);
        await _repo.AddAsync(review);

        review.Edit(Rating.Of(3), Comment.Create("Изменил мнение о товаре"));
        await _repo.UpdateAsync(review);

        var found = await _repo.GetByIdAsync(review.Id);
        found.Should().NotBeNull();
        found!.Rating.Value.Should().Be(3);
    }

    [Fact]
    public async Task DeleteAsync_RemovesReview()
    {
        var review = CreateReview();
        await _repo.AddAsync(review);

        await _repo.DeleteAsync(review);

        _context.Reviews.Should().BeEmpty();
    }
}