using MediatR;
using Backend.Application.Users.Commands;
using Backend.Application.Users.DTOs;
using Backend.Domain.Shared;
using Backend.Domain.Users;
using Backend.Domain.Users.Repositories;
using Backend.Domain.Users.ValueObjects;

namespace Backend.Application.Users.Handlers;

public class SignInHandler : IRequestHandler<SignInCommand, AuthResultDto>
{
    public const string InvalidCredentialsMessage = "Неверное имя пользователя или пароль";

    /// <summary>
    /// Хеш случайного пароля. Проверяется, когда пользователь не найден, чтобы время ответа
    /// не позволяло определить, существует ли логин (защита от перебора имён по таймингу).
    /// </summary>
    private static readonly PasswordHash DummyHash =
        PasswordHash.FromHash("$2a$12$LzewE/jhrl44HV.E4.1cE.bX5eiy35vKJgdWjUM63rcSDD/Cn8aqC");

    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public SignInHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<AuthResultDto> Handle(
        SignInCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UserName))
            throw new DomainException("Имя пользователя обязательно");

        if (string.IsNullOrWhiteSpace(request.Password))
            throw new DomainException("Пароль обязателен");

        UserName userName;
        try
        {
            userName = UserName.Create(request.UserName);
        }
        catch (DomainException)
        {
            // Не раскрываем правила формата логина при входе
            throw new DomainException(InvalidCredentialsMessage);
        }

        var user = await _userRepository.GetByUserNameAsync(userName, cancellationToken);
        if (user == null)
        {
            _passwordHasher.Verify(request.Password, DummyHash);
            throw new DomainException(InvalidCredentialsMessage);
        }

        var isValid = _passwordHasher.Verify(request.Password, user.PasswordHash);
        if (!isValid)
            throw new DomainException(InvalidCredentialsMessage);

        if (!string.IsNullOrWhiteSpace(request.Role))
        {
            if (!RoleExtensions.IsValid(request.Role.Trim()))
                throw new DomainException("Роль должна быть 'Admin' или 'User'");

            var expectedRole = RoleExtensions.FromCode(request.Role);
            if (user.Role != expectedRole)
                throw new DomainException("Роль не соответствует");
        }

        var token = _jwtService.GenerateToken(
            user.Id, user.UserName.Value, user.Role.ToCode(), user.SecurityStamp);

        return new AuthResultDto
        {
            IsSuccess = true,
            Message = "Вход выполнен успешно",
            Token = token,
            UserId = user.Id,
            UserName = user.UserName.Value,
            Role = user.Role.ToCode()
        };
    }
}
