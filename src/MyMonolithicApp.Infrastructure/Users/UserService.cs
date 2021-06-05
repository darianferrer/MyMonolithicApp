using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using MyMonolithicApp.Domain.Users;
using MyMonolithicApp.Infrastructure.Data.Entities;

namespace MyMonolithicApp.Infrastructure.UserManagement
{
    public class UserService : IUserService
    {
        private readonly UserManager<UserEntity> _userManager;

        public UserService(UserManager<UserEntity> userManager)
        {
            _userManager = userManager;
        }

        public async Task<User?> FindByUsernameAsync(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            return user != null
                ? UserEntity.ToModel(user)
                : null;
        }
    }
}
