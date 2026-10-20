using MediatR;
using Backend.Application.Reviews.DTOs;
using Backend.Application.Reviews.Mappers;
using Backend.Application.Reviews.Queries;
using Backend.Domain.Reviews.Repositories;

namespace Backend.Application.Reviews.Handlers;

public class GetProductReviewsHandler 
    : IRequestHandler<GetProductReviewsQuery, ProductReviewsResultDto>
{
    private readonly IReviewRepository _reviewRepository;

    public GetProductReviewsHandler(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task<ProductReviewsResultDto> Handle(
        GetProductReviewsQuery request,
        CancellationToken cancellationToken)
    {
        var reviews = await _reviewRepository.GetByProductAsync(request.ProductId, cancellationToken);

        var totalReviews = reviews.Count;
        var averageRating = totalReviews > 0
            ? Math.Round(reviews.Average(r => r.Rating.Value), 1)
            : 0;

        return new ProductReviewsResultDto
        {
            ProductId = request.ProductId,
            TotalReviews = totalReviews,
            AverageRating = averageRating,
            Reviews = reviews.Select(ReviewMapper.MapToDto).ToList()
        };
    }
}