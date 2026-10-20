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
        // Валидация роли
        if (!RoleExtensions.IsValid(request.NewRole))
            throw new DomainException("Роль должна быть 'Admin' или 'User'");

        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
            throw new DomainException($"Пользователь с Id={request.UserId} не найден");

        var newRole = RoleExtensions.FromCode(request.NewRole);

        // Бизнес-метод домена (сам проверит, что не та же роль)
        user.ChangeRole(newRole);

        await _userRepository.UpdateAsync(user, cancellationToken);
    }
}