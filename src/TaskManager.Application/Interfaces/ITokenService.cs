using TaskManager.Domain.Entities;

namespace TaskManager.Application.Interfaces;

public interface ITokenService
{
    string CreateToken(User user);
}
