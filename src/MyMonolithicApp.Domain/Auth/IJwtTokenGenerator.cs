using MyMonolithicApp.Domain.Users;

namespace MyMonolithicApp.Domain.Auth
{
    public interface IJwtTokenGenerator
    {
        string CreateToken(User user);
    }
}
