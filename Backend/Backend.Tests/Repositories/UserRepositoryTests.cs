using Backend.Domain.Users;
using Backend.Domain.Users.ValueObjects;
using Backend.Infrastructure.Persistence;
using Backend.Infrastructure.Repositories;
using Backend.Tests.Persistence;
using FluentAssertions;

namespace Backend.Tests.Repositories;

public class UserRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly UserRepository _repo;

    public UserRepositoryTests()
    {
        _context = TestDbContextFactory.CreateInMemory();
        _repo = new UserRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    private User CreateUser(string name, Role role = Role.User)
    {
        var userName = UserName.Create(name);
        var passwordHash = PasswordHash.FromHash(new string('a', 60));
        return new User(userName, passwordHash, role);
    }

    [Fact]
    public async Task AddAsync_ValidUser_SavesToDatabase()
    {
        var user = CreateUser("testuser");

        await _repo.AddAsync(user);

        _context.Users.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingUser_ReturnsUser()
    {
        var user = CreateUser("testuser");
        await _repo.AddAsync(user);

        var found = await _repo.GetByIdAsync(user.Id);

        found.Should().NotBeNull();
        found!.UserName.Value.Should().Be("testuser");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingUser_ReturnsNull()
    {
        var found = await _repo.GetByIdAsync(999);
        found.Should().BeNull();
    }

    [Fact]
    public async Task GetByUserNameAsync_ExistingUser_ReturnsUser()
    {
        var user = CreateUser("testuser");
        await _repo.AddAsync(user);

        var found = await _repo.GetByUserNameAsync(UserName.Create("testuser"));

        found.Should().NotBeNull();
        found!.UserName.Value.Should().Be("testuser");
    }

    [Fact]
    public async Task GetByUserNameAsync_ByString_ReturnsUser()
    {
        var user = CreateUser("testuser");
        await _repo.AddAsync(user);

        var found = await _repo.GetByUserNameAsync("testuser");

        found.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByUserNameAsync_NonExisting_ReturnsNull()
    {
        var found = await _repo.GetByUserNameAsync(UserName.Create("nonexistent"));
        found.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllUsers()
    {
        await _repo.AddAsync(CreateUser("user1"));
        await _repo.AddAsync(CreateUser("user2"));
        await _repo.AddAsync(CreateUser("user3"));

        var users = await _repo.GetAllAsync();

        users.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllAsync_OrderedById()
    {
        await _repo.AddAsync(CreateUser("user1"));
        await _repo.AddAsync(CreateUser("user2"));

        var users = await _repo.GetAllAsync();

        users[0].Id.Should().BeLessThan(users[1].Id);
    }

    [Fact]
    public async Task ExistsAsync_ExistingUser_ReturnsTrue()
    {
        await _repo.AddAsync(CreateUser("testuser"));

        var exists = await _repo.ExistsAsync(UserName.Create("testuser"));

        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_NonExisting_ReturnsFalse()
    {
        var exists = await _repo.ExistsAsync(UserName.Create("nonexistent"));
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ModifiesUser()
    {
        var user = CreateUser("oldname");
        await _repo.AddAsync(user);

        user.Rename(UserName.Create("newname"));
        await _repo.UpdateAsync(user);

        var found = await _repo.GetByIdAsync(user.Id);
        found!.UserName.Value.Should().Be("newname");
    }

    [Fact]
    public async Task DeleteAsync_RemovesUser()
    {
        var user = CreateUser("testuser");
        await _repo.AddAsync(user);

        await _repo.DeleteAsync(user);

        _context.Users.Should().BeEmpty();
    }

    [Fact]
    public async Task CountAsync_ReturnsCorrectCount()
    {
        await _repo.AddAsync(CreateUser("user1"));
        await _repo.AddAsync(CreateUser("user2"));

        var count = await _repo.CountAsync();

        count.Should().Be(2);
    }

    [Fact]
    public async Task CountAsync_EmptyDb_ReturnsZero()
    {
        var count = await _repo.CountAsync();
        count.Should().Be(0);
    }

    [Fact]
    public async Task CountByRoleAsync_ReturnsAdminsCount()
    {
        await _repo.AddAsync(CreateUser("admin1", Role.Admin));
        await _repo.AddAsync(CreateUser("admin2", Role.Admin));
        await _repo.AddAsync(CreateUser("user1", Role.User));

        var admins = await _repo.CountByRoleAsync(Role.Admin);

        admins.Should().Be(2);
    }

    [Fact]
    public async Task CountByRoleAsync_ReturnsUsersCount()
    {
        await _repo.AddAsync(CreateUser("admin1", Role.Admin));
        await _repo.AddAsync(CreateUser("user1", Role.User));
        await _repo.AddAsync(CreateUser("user2", Role.User));

        var users = await _repo.CountByRoleAsync(Role.User);

        users.Should().Be(2);
    }
}