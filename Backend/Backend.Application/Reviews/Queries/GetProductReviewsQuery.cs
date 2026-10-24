using MediatR;
using Backend.Application.Reviews.DTOs;

namespace Backend.Application.Reviews.Queries;

public class GetProductReviewsQuery : IRequest<ProductReviewsResultDto>
{
    public int ProductId { get; set; }

    public GetProductReviewsQuery(int productId)
    {
        ProductId = productId;
    }
}

public class ProductReviewsResultDto
{
    public int ProductId { get; set; }
    public int TotalReviews { get; set; }
    public double AverageRating { get; set; }

    /// <summary>Сколько отзывов с каждой оценкой: ключ 1..5.</summary>
    public Dictionary<int, int> RatingDistribution { get; set; } = new();
    public List<ReviewDto> Reviews { get; set; } = new();
}