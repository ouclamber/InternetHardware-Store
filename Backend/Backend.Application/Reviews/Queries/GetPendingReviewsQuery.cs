using MediatR;
using Backend.Application.Reviews.DTOs;

namespace Backend.Application.Reviews.Queries;

/// <summary>Отзывы, ожидающие модерации (админ).</summary>
public class GetPendingReviewsQuery : IRequest<IReadOnlyList<ReviewDto>>
{
}
