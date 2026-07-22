using TaskManager.Application.DTOs.Auth;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Services;

public class AuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthService(
        IUserRepository users,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);
        var displayName = request.DisplayName?.Trim() ?? string.Empty;
        var password = request.Password ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ValidationException("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ValidationException("Display name is required.");
        }

        if (password.Length < 6)
        {
            throw new ValidationException("Password must be at least 6 characters.");
        }

        var existing = await _users.GetByEmailAsync(email, cancellationToken);
        if (existing is not null)
        {
            throw new ConflictException("A user with this email already exists.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            DisplayName = displayName,
            PasswordHash = _passwordHasher.Hash(password),
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _users.AddAsync(user, cancellationToken);

        return ToAuthResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);
        var password = request.Password ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            throw new ValidationException("Email and password are required.");
        }

        var user = await _users.GetByEmailAsync(email, cancellationToken);
        if (user is null || !_passwordHasher.Verify(password, user.PasswordHash))
        {
            throw new ValidationException("Invalid email or password.");
        }

        return ToAuthResponse(user);
    }

    private AuthResponse ToAuthResponse(User user) =>
        new(_tokenService.CreateToken(user), user.Id, user.Email, user.DisplayName);

    private static string NormalizeEmail(string? email) =>
        (email ?? string.Empty).Trim().ToLowerInvariant();
}
