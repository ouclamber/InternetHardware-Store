using MediatR;
using Backend.Application.Common;
using Backend.Application.Users.Commands;
using Backend.Domain.Shared;
using Backend.Domain.Users;
using Backend.Domain.Users.Repositories;
using Backend.Domain.Users.ValueObjects;

namespace Backend.Application.Users.Handlers;

public class SignUpHandler : IRequestHandler<SignUpCommand, int>
{
    public const int MinPasswordLength = PasswordPolicy.MinLength;

    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public SignUpHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<int> Handle(
        SignUpCommand request,
        CancellationToken cancellationToken)
    {
        PasswordPolicy.Validate(request.Password);

        if (request.Password != request.ConfirmPassword)
            throw new DomainException("Пароли не совпадают");

        var userName = UserName.Create(request.UserName);

        var exists = await _userRepository.ExistsAsync(userName, cancellationToken);
        if (exists)
            throw new DomainException("Пользователь с таким именем уже существует");

        var passwordHash = _passwordHasher.Hash(request.Password);

        // Роль всегда User — повысить права через регистрацию нельзя.
        var user = new User(userName, passwordHash, Role.User);

        await _userRepository.AddAsync(user, cancellationToken);

        return user.Id;
    }
}
