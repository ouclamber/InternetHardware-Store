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

        var userName = UserName.Create(request.UserName);

        var user = await _userRepository.GetByUserNameAsync(userName, cancellationToken);
        if (user == null)
            throw new DomainException("Неверное имя пользователя или пароль");

        var isValid = _passwordHasher.Verify(request.Password, user.PasswordHash);
        if (!isValid)
            throw new DomainException("Неверное имя пользователя или пароль");

        var expectedRole = RoleExtensions.FromCode(request.Role);
        if (user.Role != expectedRole)
            throw new DomainException("Роль не соответствует");

        var token = _jwtService.GenerateToken(user.Id, user.UserName.Value, user.Role.ToCode());

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