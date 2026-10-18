using FluentValidation;
using FootballTickets.Application.Common;
using FootballTickets.Domain.Entities;
using FootballTickets.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FootballTickets.Application.Auth;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct);
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct);
}

public class AuthService : IAuthService
{
    private readonly IAppDbContext _db;
    private readonly IPasswordService _passwords;
    private readonly IJwtTokenGenerator _jwt;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;

    public AuthService(IAppDbContext db, IPasswordService passwords, IJwtTokenGenerator jwt,
        IValidator<RegisterRequest> registerValidator, IValidator<LoginRequest> loginValidator)
    {
        _db = db; _passwords = passwords; _jwt = jwt;
        _registerValidator = registerValidator; _loginValidator = loginValidator;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        await _registerValidator.ValidateAndThrowAsync(request, ct);
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _db.Users.AnyAsync(u => u.Email == email, ct))
            throw new ConflictException("Email is already registered");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            FullName = request.FullName.Trim(),
            PasswordHash = _passwords.Hash(request.Password),
            Role = UserRole.User,
            CreatedAt = DateTime.UtcNow
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);
        return new AuthResponse(_jwt.Generate(user));
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        await _loginValidator.ValidateAndThrowAsync(request, ct);
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
        if (user is null || !_passwords.Verify(user.PasswordHash, request.Password))
            throw new UnauthorizedException("Invalid email or password");

        return new AuthResponse(_jwt.Generate(user));
    }
}
