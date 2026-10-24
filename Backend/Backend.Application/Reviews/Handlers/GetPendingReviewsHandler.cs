using MediatR;
using Backend.Application.Reviews.DTOs;
using Backend.Application.Reviews.Mappers;
using Backend.Application.Reviews.Queries;
using Backend.Domain.Reviews.Repositories;

namespace Backend.Application.Reviews.Handlers;

public class GetPendingReviewsHandler : IRequestHandler<GetPendingReviewsQuery, IReadOnlyList<ReviewDto>>
{
    private readonly IReviewRepository _reviewRepository;

    public GetPendingReviewsHandler(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task<IReadOnlyList<ReviewDto>> Handle(
        GetPendingReviewsQuery request,
        CancellationToken cancellationToken)
    {
        var reviews = await _reviewRepository.GetPendingAsync(cancellationToken);
        return reviews.Select(ReviewMapper.MapToDto).ToList();
    }
}
