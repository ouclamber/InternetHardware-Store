using MediatR;
using Backend.Application.Users.Commands;
using Backend.Domain.Shared;
using Backend.Domain.Users.Repositories;

namespace Backend.Application.Users.Handlers;

public class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public ChangePasswordHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task Handle(
        ChangePasswordCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
            throw new DomainException("Пользователь не найден");

        var isOldPasswordValid = _passwordHasher.Verify(request.OldPassword, user.PasswordHash);
        if (!isOldPasswordValid)
            throw new DomainException("Неверный старый пароль");

        var newPasswordHash = _passwordHasher.Hash(request.NewPassword);

        user.ChangePassword(newPasswordHash);

        await _userRepository.UpdateAsync(user, cancellationToken);
    }
}