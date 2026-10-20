using MediatR;
using Backend.Application.Admin.DTOs;

namespace Backend.Application.Admin.Queries;

public class GetAllOrdersQuery : IRequest<IReadOnlyList<AdminOrderDto>>
{
}