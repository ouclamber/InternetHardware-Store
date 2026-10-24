using MediatR;
using Backend.Application.Admin.Commands;
using Backend.Domain.Shared;
using Backend.Domain.Users;
using Backend.Domain.Users.Repositories;

namespace Backend.Application.Admin.Handlers;

public class UpdateUserRoleHandler : IRequestHandler<UpdateUserRoleCommand>
{
    private readonly IUserRepository _userRepository;

    public UpdateUserRoleHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task Handle(
        UpdateUserRoleCommand request,
        CancellationToken cancellationToken)
    {
        if (!RoleExtensions.IsValid(request.NewRole))
            throw new DomainException("Роль должна быть 'Admin' или 'User'");

        if (request.RequestingUserId > 0 && request.RequestingUserId == request.UserId)
            throw new ForbiddenException("Нельзя изменить собственную роль");

        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
            throw new NotFoundException($"Пользователь с Id={request.UserId} не найден");

        var newRole = RoleExtensions.FromCode(request.NewRole);

        // Магазин не должен остаться без администратора
        if (user.Role == Role.Admin && newRole != Role.Admin)
        {
            var admins = await _userRepository.CountByRoleAsync(Role.Admin, cancellationToken);
            if (admins <= 1)
                throw new DomainException("Нельзя понизить последнего администратора");
        }

        // Домен сам игнорирует смену на ту же роль и обновляет SecurityStamp —
        // старый токен пользователя с прежней ролью перестаёт действовать.
        user.ChangeRole(newRole);

        await _userRepository.UpdateAsync(user, cancellationToken);
    }
}
