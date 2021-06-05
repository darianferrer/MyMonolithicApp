using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MyMonolithicApp.Domain.Auth;
using MyMonolithicApp.Domain.Exceptions;
using MyMonolithicApp.Domain.Users;
using OneOf;

namespace MyMonolithicApp.Application.Auth
{
    public record LoginCommand(string Username, string Password) : IRequest<OneOf<LoginResponse, Error>>;

    public record LoginResponse(string Username, string Email, string Token)
    {
        public static LoginResponse FromModel(User user, string token) =>
            new(user.Username, user.Email, token);
    }

    public class LoginCommandHandler : IRequestHandler<LoginCommand, OneOf<LoginResponse, Error>>
    {
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly ISignInService _signInService;
        private readonly IUserService _userService;

        public LoginCommandHandler(IJwtTokenGenerator jwtTokenGenerator,
            ISignInService signInService,
            IUserService userService)
        {
            _jwtTokenGenerator = jwtTokenGenerator;
            _signInService = signInService;
            _userService = userService;
        }

        public async Task<OneOf<LoginResponse, Error>> Handle(LoginCommand request,
            CancellationToken cancellationToken = default)
        {
            var result = await _signInService.SignInAsync(request.Username, request.Password);
            if (result.IsT1)
            {
                return result.AsT1;
            }

            var user = await _userService.FindByUsernameAsync(request.Username);
            if (user == null)
            {
                throw new System.Exception("Unexpected error, user not found after logged in");
            }

            var token = _jwtTokenGenerator.CreateToken(user);
            return LoginResponse.FromModel(user, token);
        }
    }
}
