using MediatR;

namespace Backend.Application.Users.Commands;

/// <summary>
/// Регистрация нового пользователя.
/// Роль намеренно НЕ принимается от клиента: все, кто регистрируется через API,
/// получают роль User. Администратор создаётся только при старте приложения
/// из конфигурации (Admin:UserName / Admin:Password), см. AdminSeeder.
/// </summary>
public class SignUpCommand : IRequest<int>
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}
