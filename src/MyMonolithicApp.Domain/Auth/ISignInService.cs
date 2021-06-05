using System.Threading.Tasks;
using MyMonolithicApp.Domain.Exceptions;
using OneOf;

namespace MyMonolithicApp.Domain.Auth
{
    public interface ISignInService
    {
        Task<OneOf<bool, Error>> SignInAsync(string username, string password);
    }
}
