using TaskManager.Domain.Entities;
using Xunit;

namespace TaskManager.Tests.Domain;

public class UserTests
{
    [Fact]
    public void User_can_be_created_with_email_and_display_name()
    {
        var id = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow;

        var user = new User
        {
            Id = id,
            Email = "demo@taskmanager.local",
            PasswordHash = "hash-placeholder",
            DisplayName = "Demo User",
            CreatedAt = createdAt
        };

        Assert.Equal(id, user.Id);
        Assert.Equal("demo@taskmanager.local", user.Email);
        Assert.Equal("Demo User", user.DisplayName);
        Assert.Equal("hash-placeholder", user.PasswordHash);
        Assert.Equal(createdAt, user.CreatedAt);
    }
}
