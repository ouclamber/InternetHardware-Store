using MediatR;
using Backend.Application.Common;
using Backend.Application.Users.Commands;
using Backend.Application.Users.DTOs;
using Backend.Domain.Shared;
using Backend.Domain.Users;
using Backend.Domain.Users.Repositories;

namespace Backend.Application.Users.Handlers;

public class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, AuthResultDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public ChangePasswordHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<AuthResultDto> Handle(
        ChangePasswordCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
            throw new NotFoundException("Пользователь не найден");

        var isOldPasswordValid = _passwordHasher.Verify(request.OldPassword, user.PasswordHash);
        if (!isOldPasswordValid)
            throw new DomainException("Неверный старый пароль");

        // Раньше новый пароль не проверялся вовсе — можно было сменить его на "1".
        PasswordPolicy.Validate(request.NewPassword);

        if (request.NewPassword == request.OldPassword)
            throw new DomainException("Новый пароль должен отличаться от старого");

        var newPasswordHash = _passwordHasher.Hash(request.NewPassword);

        // Домен сам меняет SecurityStamp → старые токены перестают приниматься
        user.ChangePassword(newPasswordHash);

        await _userRepository.UpdateAsync(user, cancellationToken);

        return new AuthResultDto
        {
            IsSuccess = true,
            Message = "Пароль успешно изменён",
            Token = _jwtService.GenerateToken(user.Id, user.UserName.Value, user.Role.ToCode(), user.SecurityStamp),
            UserId = user.Id,
            UserName = user.UserName.Value,
            Role = user.Role.ToCode()
        };
    }
}
