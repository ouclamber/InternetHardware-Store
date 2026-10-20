using MediatR;
using Backend.Application.Admin.DTOs;
using Backend.Application.Admin.Queries;
using Backend.Domain.Users;
using Backend.Domain.Users.Repositories;

namespace Backend.Application.Admin.Handlers;

public class GetUsersHandler : IRequestHandler<GetUsersQuery, IReadOnlyList<UserListItemDto>>
{
    private readonly IUserRepository _userRepository;

    public GetUsersHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IReadOnlyList<UserListItemDto>> Handle(
        GetUsersQuery request,
        CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);

        return users.Select(u => new UserListItemDto
        {
            Id = u.Id,
            UserName = u.UserName.Value,
            Role = u.Role.ToCode(),
            CreatedAt = u.CreatedAt
        }).ToList();
    }
}