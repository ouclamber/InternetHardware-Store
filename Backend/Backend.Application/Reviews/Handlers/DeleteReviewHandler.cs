using MediatR;
using Backend.Application.Reviews.Commands;
using Backend.Domain.Reviews.Repositories;
using Backend.Domain.Shared;

namespace Backend.Application.Reviews.Handlers;

public class DeleteReviewHandler : IRequestHandler<DeleteReviewCommand>
{
    private readonly IReviewRepository _reviewRepository;

    public DeleteReviewHandler(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task Handle(
        DeleteReviewCommand request,
        CancellationToken cancellationToken)
    {
        var review = await _reviewRepository.GetByIdAsync(request.ReviewId, cancellationToken);
        if (review == null)
            throw new DomainException($"Отзыв с Id={request.ReviewId} не найден");

        if (!request.IsAdmin && review.UserId != request.RequestingUserId)
            throw new DomainException("Нет доступа к удалению этого отзыва");

        await _reviewRepository.DeleteAsync(review, cancellationToken);
    }
}