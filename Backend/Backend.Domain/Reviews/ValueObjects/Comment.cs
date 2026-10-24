using Backend.Domain.Shared;

namespace Backend.Domain.Reviews.ValueObjects;

public sealed class Comment : ValueObject
{
    public const int MinLength = 10;
    public const int MaxLength = 2000;

    public string Value { get; }

    private Comment(string value)
    {
        Value = value;
    }

    public static Comment Create(string comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
            throw new DomainException("Комментарий не может быть пустым");

        var trimmed = comment.Trim();

        if (trimmed.Length < MinLength)
            throw new DomainException($"Комментарий должен быть не короче {MinLength} символов");

        if (trimmed.Length > MaxLength)
            throw new DomainException($"Комментарий не может быть длиннее {MaxLength} символов");

        return new Comment(trimmed);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(Comment comment) => comment.Value;
}