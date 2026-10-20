using MediatR;
using Backend.Application.Reviews.Commands;
using Backend.Domain.Reviews.Repositories;
using Backend.Domain.Shared;

namespace Backend.Application.Reviews.Handlers;

public class ApproveReviewHandler : IRequestHandler<ApproveReviewCommand>
{
    private readonly IReviewRepository _reviewRepository;

    public ApproveReviewHandler(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task Handle(
        ApproveReviewCommand request,
        CancellationToken cancellationToken)
    {
        var review = await _reviewRepository.GetByIdAsync(request.ReviewId, cancellationToken);
        if (review == null)
            throw new DomainException($"Отзыв с Id={request.ReviewId} не найден");

        review.Approve(request.AdminUserId);

        await _reviewRepository.UpdateAsync(review, cancellationToken);
    }
}