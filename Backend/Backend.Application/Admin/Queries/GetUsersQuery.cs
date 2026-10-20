using MediatR;
using Backend.Application.Admin.DTOs;

namespace Backend.Application.Admin.Queries;

public class GetUsersQuery : IRequest<IReadOnlyList<UserListItemDto>>
{
}