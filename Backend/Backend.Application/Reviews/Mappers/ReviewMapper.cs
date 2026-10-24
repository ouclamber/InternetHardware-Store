using Backend.Application.Reviews.DTOs;
using Backend.Domain.Reviews;

namespace Backend.Application.Reviews.Mappers;

internal static class ReviewMapper
{
    public static ReviewDto MapToDto(Review review)
    {
        return new ReviewDto
        {
            Id = review.Id,
            ProductId = review.ProductId,
            ProductName = review.Product?.Name.Value,
            UserId = review.UserId,
            UserName = review.User?.UserName.Value ?? "Аноним",
            Rating = review.Rating.Value,
            Comment = review.Comment.Value,
            Status = review.Status.ToCode(),
            StatusText = review.Status.ToRussianString(),
            CreatedAt = review.CreatedAt,
            UpdatedAt = review.UpdatedAt,
            ModeratedAt = review.ModeratedAt
        };
    }
}