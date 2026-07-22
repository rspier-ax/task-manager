using TaskManager.Infrastructure.Security;
using Xunit;

namespace TaskManager.Tests.Infrastructure;

public class PasswordHasherTests
{
    private readonly PasswordHasher _sut = new();

    [Fact]
    public void Hash_and_Verify_round_trip()
    {
        var hash = _sut.Hash("Demo123!");

        Assert.False(string.IsNullOrWhiteSpace(hash));
        Assert.True(_sut.Verify("Demo123!", hash));
        Assert.False(_sut.Verify("wrong", hash));
    }
}
