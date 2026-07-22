using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Options;
using TaskManager.Infrastructure.Security;
using Xunit;

namespace TaskManager.Tests.Infrastructure;

public class JwtTokenServiceTests
{
    [Fact]
    public void CreateToken_includes_name_identifier_claim()
    {
        var settings = Options.Create(new JwtSettings
        {
            Key = "TEST_ONLY_TASKMANAGER_SUPER_SECRET_KEY_32CHARS",
            Issuer = "TaskManager",
            Audience = "TaskManager",
            ExpiryMinutes = 60
        });

        var sut = new JwtTokenService(settings);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "demo@taskmanager.local",
            DisplayName = "Demo User"
        };

        var token = sut.CreateToken(user);

        Assert.False(string.IsNullOrWhiteSpace(token));

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Contains(jwt.Claims, c => c.Value == user.Id.ToString());
        Assert.Equal(user.Id.ToString(), jwt.Subject);
    }
}
