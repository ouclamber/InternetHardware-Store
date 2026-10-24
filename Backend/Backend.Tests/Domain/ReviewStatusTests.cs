using Backend.Domain.Reviews;
using FluentAssertions;

namespace Backend.Tests.Domain;

public class ReviewStatusTests
{
    [Theory]
    [InlineData(ReviewStatus.Pending, "pending")]
    [InlineData(ReviewStatus.Approved, "approved")]
    [InlineData(ReviewStatus.Rejected, "rejected")]
    public void ToCode_ReturnsExpected(ReviewStatus status, string expected)
    {
        status.ToCode().Should().Be(expected);
    }

    [Theory]
    [InlineData("pending", ReviewStatus.Pending)]
    [InlineData("approved", ReviewStatus.Approved)]
    [InlineData("rejected", ReviewStatus.Rejected)]
    [InlineData("PENDING", ReviewStatus.Pending)]
    public void FromCode_ValidCode_ReturnsStatus(string code, ReviewStatus expected)
    {
        ReviewStatusExtensions.FromCode(code).Should().Be(expected);
    }

    [Fact]
    public void FromCode_InvalidCode_ThrowsArgumentException()
    {
        var act = () => ReviewStatusExtensions.FromCode("nope");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ToRussianString_Approved_ContainsRussian()
    {
        ReviewStatus.Approved.ToRussianString().Should().Contain("Одобр");
    }
}