namespace Backend.Domain.Shared;

/// <summary>
/// Запрошенный объект не найден (HTTP 404).
/// Наследуется от <see cref="DomainException"/>, поэтому код, который ловит
/// DomainException, продолжает работать.
/// </summary>
public class NotFoundException : DomainException
{
    public NotFoundException(string message) : base(message) { }
}

/// <summary>
/// Операция запрещена для текущего пользователя (HTTP 403).
/// </summary>
public class ForbiddenException : DomainException
{
    public ForbiddenException(string message) : base(message) { }
}

/// <summary>
/// Конфликт состояния: данные изменены параллельным запросом,
/// нарушена уникальность и т.п. (HTTP 409).
/// </summary>
public class ConflictException : DomainException
{
    public ConflictException(string message) : base(message) { }

    public ConflictException(string message, Exception innerException) : base(message, innerException) { }
}
