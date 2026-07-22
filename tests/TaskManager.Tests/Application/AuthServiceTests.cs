using NSubstitute;
using TaskManager.Application.DTOs.Auth;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Interfaces;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;
using Xunit;

namespace TaskManager.Tests.Application;

public class AuthServiceTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _sut = new AuthService(_users, _passwordHasher, _tokenService);
        _passwordHasher.Hash(Arg.Any<string>()).Returns(call => $"hashed:{call.Arg<string>()}");
        _tokenService.CreateToken(Arg.Any<User>()).Returns("test-token");
    }

    [Fact]
    public async Task RegisterAsync_creates_user_and_returns_token()
    {
        _users.GetByEmailAsync("demo@taskmanager.local", Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var response = await _sut.RegisterAsync(
            new RegisterRequest("Demo@TaskManager.Local", "Demo123!", "Demo User"));

        Assert.Equal("test-token", response.Token);
        Assert.Equal("demo@taskmanager.local", response.Email);
        Assert.Equal("Demo User", response.DisplayName);
        await _users.Received(1).AddAsync(
            Arg.Is<User>(u =>
                u.Email == "demo@taskmanager.local" &&
                u.DisplayName == "Demo User" &&
                u.PasswordHash == "hashed:Demo123!"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RegisterAsync_rejects_duplicate_email()
    {
        _users.GetByEmailAsync("demo@taskmanager.local", Arg.Any<CancellationToken>())
            .Returns(new User { Id = Guid.NewGuid(), Email = "demo@taskmanager.local" });

        await Assert.ThrowsAsync<ConflictException>(() =>
            _sut.RegisterAsync(new RegisterRequest("demo@taskmanager.local", "Demo123!", "Demo")));
    }

    [Fact]
    public async Task LoginAsync_returns_token_for_valid_credentials()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "demo@taskmanager.local",
            DisplayName = "Demo",
            PasswordHash = "hashed:secret"
        };

        _users.GetByEmailAsync("demo@taskmanager.local", Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify("secret", "hashed:secret").Returns(true);

        var response = await _sut.LoginAsync(new LoginRequest("demo@taskmanager.local", "secret"));

        Assert.Equal("test-token", response.Token);
        Assert.Equal(user.Id, response.UserId);
    }

    [Fact]
    public async Task LoginAsync_rejects_invalid_password()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "demo@taskmanager.local",
            PasswordHash = "hashed:secret"
        };

        _users.GetByEmailAsync("demo@taskmanager.local", Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify("wrong", "hashed:secret").Returns(false);

        await Assert.ThrowsAsync<ValidationException>(() =>
            _sut.LoginAsync(new LoginRequest("demo@taskmanager.local", "wrong")));
    }
}
