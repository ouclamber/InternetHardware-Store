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

    public CreateReviewHandler(
        IReviewRepository reviewRepository,
        IProductRepository productRepository)
    {
        _reviewRepository = reviewRepository;
        _productRepository = productRepository;
    }

    public async Task<int> Handle(
        CreateReviewCommand request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
            throw new DomainException($"Товар с Id={request.ProductId} не найден");

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
            autoApprove: true);

        await _reviewRepository.AddAsync(review, cancellationToken);

        return review.Id;
    }
}