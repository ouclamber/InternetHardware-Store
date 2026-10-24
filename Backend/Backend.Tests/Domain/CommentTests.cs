using Backend.Domain.Reviews.ValueObjects;
using Backend.Domain.Shared;
using FluentAssertions;

namespace Backend.Tests.Domain;

public class CommentTests
{

    [Fact]
    public void Create_ValidComment_CreatesComment()
    {
        var comment = Comment.Create("Отличный товар, всем советую!");
        comment.Value.Should().Be("Отличный товар, всем советую!");
    }

    [Fact]
    public void Create_ExactMinLength_Succeeds()
    {
        var text = new string('a', 10);
        var comment = Comment.Create(text);
        comment.Value.Should().Be(text);
    }

    [Fact]
    public void Create_TrimsWhitespace()
    {
        var comment = Comment.Create("  Это отличный товар  ");
        comment.Value.Should().Be("Это отличный товар");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_Empty_ThrowsDomainException(string? value)
    {
        var act = () => Comment.Create(value!);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_TooShort_ThrowsDomainException()
    {
        var act = () => Comment.Create("Короткий");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_TooLong_ThrowsDomainException()
    {
        var act = () => Comment.Create(new string('a', 2001));
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Equals_SameText_ReturnsTrue()
    {
        var a = Comment.Create("Отличный товар");
        var b = Comment.Create("Отличный товар");
        a.Should().Be(b);
    }

    [Fact]
    public void ImplicitConversion_ToString_ReturnsValue()
    {
        string value = Comment.Create("Отличный товар");
        value.Should().Be("Отличный товар");
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        Comment.Create("Отличный товар").ToString().Should().Be("Отличный товар");
    }
}