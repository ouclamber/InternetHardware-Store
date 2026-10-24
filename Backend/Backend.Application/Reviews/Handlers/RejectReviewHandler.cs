using MediatR;
using Backend.Application.Reviews.Commands;
using Backend.Domain.Reviews.Repositories;
using Backend.Domain.Shared;

namespace Backend.Application.Reviews.Handlers;

public class RejectReviewHandler : IRequestHandler<RejectReviewCommand>
{
    private readonly IReviewRepository _reviewRepository;

    public RejectReviewHandler(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task Handle(RejectReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await _reviewRepository.GetByIdAsync(request.ReviewId, cancellationToken);
        if (review == null)
            throw new NotFoundException($"Отзыв с Id={request.ReviewId} не найден");

        review.Reject(request.AdminUserId);

        await _reviewRepository.UpdateAsync(review, cancellationToken);
    }
}
