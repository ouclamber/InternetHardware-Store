using MediatR;
using Backend.Application.Admin.Commands;
using Backend.Domain.Shared;
using Backend.Domain.Users;
using Backend.Domain.Users.Repositories;

namespace Backend.Application.Admin.Handlers;

public class DeleteUserHandler : IRequestHandler<DeleteUserCommand>
{
    private readonly IUserRepository _userRepository;

    public DeleteUserHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task Handle(
        DeleteUserCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
            throw new DomainException($"Пользователь с Id={request.UserId} не найден");

        // Запрет удалять админов
        if (user.Role == Role.Admin)
            throw new DomainException("Нельзя удалить администратора");

        await _userRepository.DeleteAsync(user, cancellationToken);
    }
}