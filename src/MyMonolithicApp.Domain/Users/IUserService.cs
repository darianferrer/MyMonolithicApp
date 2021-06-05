using System.Threading.Tasks;

namespace MyMonolithicApp.Domain.Users
{
    public interface IUserService
    {
        Task<User?> FindByUsernameAsync(string username);
    }
}
