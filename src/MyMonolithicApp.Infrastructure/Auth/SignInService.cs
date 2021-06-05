using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using MyMonolithicApp.Domain.Auth;
using MyMonolithicApp.Domain.Exceptions;
using MyMonolithicApp.Infrastructure.Data.Entities;
using OneOf;

namespace MyMonolithicApp.Infrastructure.Auth
{
    public class SignInService : ISignInService
    {
        private readonly SignInManager<UserEntity> _signInManager;
        private readonly UserManager<UserEntity> _userManager;

        private static readonly Error _invalidCredentials = new(Severity.Correctable,
            "login.invalidCredentials",
            "Credentials are not valid");

        public SignInService(SignInManager<UserEntity> signInManager, UserManager<UserEntity> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public async Task<OneOf<bool, Error>> SignInAsync(string username, string password)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                return _invalidCredentials;
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
            return result.Succeeded
                ? true
                : _invalidCredentials;
        }
    }
}
