using System.Text;
using Backend.Domain.Shared;

namespace Backend.Application.Common;

/// <summary>
/// Единые требования к паролю — для регистрации и смены пароля.
/// </summary>
public static class PasswordPolicy
{
    public const int MinLength = 8;

    /// <summary>
    /// BCrypt учитывает только первые 72 байта пароля: всё, что дальше, молча отбрасывается.
    /// Поэтому длиннее не разрешаем — иначе пароли с общим началом считались бы одинаковыми.
    /// </summary>
    public const int MaxBytes = 72;

    public static void Validate(string? password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < MinLength)
            throw new DomainException($"Пароль должен содержать не менее {MinLength} символов");

        if (Encoding.UTF8.GetByteCount(password) > MaxBytes)
            throw new DomainException($"Пароль слишком длинный (максимум {MaxBytes} байт в UTF-8)");

        if (!password.Any(char.IsLetter) || !password.Any(char.IsDigit))
            throw new DomainException("Пароль должен содержать хотя бы одну букву и одну цифру");
    }
}
