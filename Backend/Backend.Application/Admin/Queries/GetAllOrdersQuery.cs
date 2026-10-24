using MediatR;
using Backend.Application.Admin.DTOs;
using Backend.Domain.Shared;

namespace Backend.Application.Admin.Queries;

/// <summary>
/// Все заказы для админки — постранично, с необязательным фильтром по статусу.
/// </summary>
public class GetAllOrdersQuery : IRequest<PagedResult<AdminOrderDto>>
{
    /// <summary>Код статуса: pending / paid / shipped / delivered / cancelled. null — все.</summary>
    public string? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = Paging.DefaultPageSize;
}
