using MediatR;
using Backend.Application.Reviews.Commands;
using Backend.Domain.Reviews.Repositories;
using Backend.Domain.Reviews.ValueObjects;
using Backend.Domain.Shared;

namespace Backend.Application.Reviews.Handlers;

public class UpdateReviewHandler : IRequestHandler<UpdateReviewCommand>
{
    private readonly IReviewRepository _reviewRepository;

    public UpdateReviewHandler(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task Handle(
        UpdateReviewCommand request,
        CancellationToken cancellationToken)
    {
        var review = await _reviewRepository.GetByIdAsync(request.ReviewId, cancellationToken);
        if (review == null)
            throw new DomainException($"Отзыв с Id={request.ReviewId} не найден");

        if (!request.IsAdmin && review.UserId != request.RequestingUserId)
            throw new DomainException("Нет доступа к редактированию этого отзыва");

        var newRating = Rating.Of(request.NewRating);
        var newComment = Comment.Create(request.NewComment);

        review.Edit(newRating, newComment);

        await _reviewRepository.UpdateAsync(review, cancellationToken);
    }
}