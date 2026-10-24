using MediatR;
using Backend.Application.Reviews.Commands;
using Backend.Domain.Reviews.Repositories;
using Backend.Domain.Reviews.ValueObjects;
using Backend.Domain.Shared;

namespace Backend.Application.Reviews.Handlers;

public class UpdateReviewHandler : IRequestHandler<UpdateReviewCommand>
{
    private readonly IReviewRepository _reviewRepository;
    private readonly ReviewOptions _options;

    public UpdateReviewHandler(IReviewRepository reviewRepository, ReviewOptions? options = null)
    {
        _reviewRepository = reviewRepository;
        _options = options ?? new ReviewOptions();
    }

    public async Task Handle(
        UpdateReviewCommand request,
        CancellationToken cancellationToken)
    {
        var review = await _reviewRepository.GetByIdAsync(request.ReviewId, cancellationToken);
        if (review == null)
            throw new NotFoundException($"Отзыв с Id={request.ReviewId} не найден");

        if (!request.IsAdmin && review.UserId != request.RequestingUserId)
            throw new ForbiddenException("Нет доступа к редактированию этого отзыва");

        var newRating = Rating.Of(request.NewRating);
        var newComment = Comment.Create(request.NewComment);

        // При включённой модерации изменённый автором отзыв снова уходит на проверку
        review.Edit(newRating, newComment, resetModeration: _options.RequireModeration && !request.IsAdmin);

        await _reviewRepository.UpdateAsync(review, cancellationToken);
    }
}