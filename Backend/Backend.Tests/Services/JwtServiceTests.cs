using Backend.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;

namespace Backend.Tests.Services;

public class JwtServiceTests
{
    private static IConfiguration CreateConfig(
        string? secretKey = null,
        string? issuer = null,
        string? audience = null)
    {
        var dict = new Dictionary<string, string?>();
        if (secretKey != null) dict["Jwt:SecretKey"] = secretKey;
        if (issuer != null) dict["Jwt:Issuer"] = issuer;
        if (audience != null) dict["Jwt:Audience"] = audience;

        return new ConfigurationBuilder()
            .AddInMemoryCollection(dict)
            .Build();
    }

    // === ГЕНЕРАЦИЯ ТОКЕНА ===

    [Fact]
    public void GenerateToken_ValidData_ReturnsToken()
    {
        var config = CreateConfig(
            secretKey: "this-is-a-super-secret-key-32-characters!",
            issuer: "TestIssuer",
            audience: "TestAudience");

        var service = new JwtService(config);
        var token = service.GenerateToken(1, "testuser", "User", "stamp-1");

        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateToken_TokenIsValidJwt()
    {
        var config = CreateConfig(secretKey: "this-is-a-super-secret-key-32-characters!");
        var service = new JwtService(config);

        var token = service.GenerateToken(1, "testuser", "User", "stamp-1");

        var handler = new JwtSecurityTokenHandler();
        handler.CanReadToken(token).Should().BeTrue();
    }

    [Fact]
    public void GenerateToken_ContainsUserIdClaim()
    {
        var config = CreateConfig(secretKey: "this-is-a-super-secret-key-32-characters!");
        var service = new JwtService(config);

        var token = service.GenerateToken(42, "testuser", "Admin", "stamp-1");
        var handler = new JwtSecurityTokenHandler();
        var parsed = handler.ReadJwtToken(token);

        parsed.Claims.Should().Contain(c => c.Type == "userId" && c.Value == "42");
    }

    [Fact]
    public void GenerateToken_ContainsUserNameClaim()
    {
        var config = CreateConfig(secretKey: "this-is-a-super-secret-key-32-characters!");
        var service = new JwtService(config);

        var token = service.GenerateToken(1, "testuser", "User", "stamp-1");
        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);

        parsed.Claims.Should().Contain(c => c.Type == "userName" && c.Value == "testuser");
    }

    [Fact]
    public void GenerateToken_ContainsRoleClaim()
    {
        var config = CreateConfig(secretKey: "this-is-a-super-secret-key-32-characters!");
        var service = new JwtService(config);

        var token = service.GenerateToken(1, "admin", "Admin", "stamp-1");
        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);

        parsed.Claims.Should().Contain(c => c.Type == "role" && c.Value == "Admin");
    }

    [Fact]
    public void GenerateToken_UsesConfiguredIssuer()
    {
        var config = CreateConfig(
            secretKey: "this-is-a-super-secret-key-32-characters!",
            issuer: "MyIssuer",
            audience: "MyAudience");

        var service = new JwtService(config);
        var token = service.GenerateToken(1, "user", "User", "stamp-1");
        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);

        parsed.Issuer.Should().Be("MyIssuer");
        parsed.Audiences.Should().Contain("MyAudience");
    }

    [Fact]
    public void Constructor_NoSecretKey_Throws()
    {
        var config = CreateConfig(); // пустая конфигурация

        var act = () => new JwtService(config);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Constructor_ShortSecretKey_Throws()
    {
        var config = CreateConfig(secretKey: "too-short");

        var act = () => new JwtService(config);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GenerateToken_OnlySecretConfigured_UsesDefaultIssuerAndAudience()
    {
        var config = CreateConfig(secretKey: "this-is-a-super-secret-key-32-characters!");
        var service = new JwtService(config);
        var token = service.GenerateToken(1, "user", "User", "stamp-1");
        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);

        parsed.Issuer.Should().Be("InternetHardwareStore");
        parsed.Audiences.Should().Contain("InternetHardwareStoreClient");
    }

    [Fact]
    public void GenerateToken_ByDefault_ExpiresInAboutOneDay()
    {
        var config = CreateConfig(secretKey: "this-is-a-super-secret-key-32-characters!");
        var service = new JwtService(config);
        var before = DateTime.UtcNow;

        var token = service.GenerateToken(1, "user", "User", "stamp-1");
        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);

        var diff = parsed.ValidTo - before;
        diff.TotalDays.Should().BeApproximately(1, 0.1);
    }

    [Fact]
    public void GenerateToken_ConfiguredLifetime_IsUsed()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = "this-is-a-super-secret-key-32-characters!",
                ["Jwt:ExpiresMinutes"] = "30"
            })
            .Build();
        var service = new JwtService(config);
        var before = DateTime.UtcNow;

        var token = service.GenerateToken(1, "user", "User", "stamp-1");
        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);

        var diff = parsed.ValidTo - before;
        diff.TotalMinutes.Should().BeApproximately(30, 1);
    }

    [Fact]
    public void GenerateToken_ContainsSecurityStampClaim()
    {
        var service = new JwtService(CreateConfig(secretKey: "this-is-a-super-secret-key-32-characters!"));

        var token = service.GenerateToken(7, "testuser", "User", "abc123stamp");
        var parsed = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().ReadJwtToken(token);

        parsed.Claims.Should().Contain(c => c.Type == JwtService.SecurityStampClaim && c.Value == "abc123stamp");
    }

    [Fact]
    public void GenerateToken_EachTokenHasUniqueId()
    {
        var service = new JwtService(CreateConfig(secretKey: "this-is-a-super-secret-key-32-characters!"));
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();

        var jti1 = handler.ReadJwtToken(service.GenerateToken(1, "u", "User", "s")).Id;
        var jti2 = handler.ReadJwtToken(service.GenerateToken(1, "u", "User", "s")).Id;

        jti1.Should().NotBeNullOrEmpty();
        jti1.Should().NotBe(jti2);
    }
}
