using MediatR;
using Backend.Application.Reviews.Commands;
using Backend.Domain.Catalog.Repositories;
using Backend.Domain.Reviews;
using Backend.Domain.Reviews.Repositories;
using Backend.Domain.Reviews.ValueObjects;
using Backend.Domain.Shared;

namespace Backend.Application.Reviews.Handlers;

public class CreateReviewHandler : IRequestHandler<CreateReviewCommand, int>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IProductRepository _productRepository;
    private readonly ReviewOptions _options;

    public CreateReviewHandler(
        IReviewRepository reviewRepository,
        IProductRepository productRepository,
        ReviewOptions? options = null)
    {
        _reviewRepository = reviewRepository;
        _productRepository = productRepository;
        _options = options ?? new ReviewOptions();
    }

    public async Task<int> Handle(
        CreateReviewCommand request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
            throw new NotFoundException($"Товар с Id={request.ProductId} не найден");

        if (!product.IsActive)
            throw new DomainException("Нельзя оставить отзыв на товар, снятый с продажи");

        var exists = await _reviewRepository.ExistsAsync(
            request.UserId,
            request.ProductId,
            cancellationToken);

        if (exists)
            throw new DomainException("Вы уже оставили отзыв на этот товар");

        var rating = Rating.Of(request.Rating);
        var comment = Comment.Create(request.Comment);

        var review = new Review(
            request.UserId,
            request.ProductId,
            rating,
            comment,
            autoApprove: !_options.RequireModeration);

        await _reviewRepository.AddAsync(review, cancellationToken);

        return review.Id;
    }
}