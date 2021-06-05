using FluentValidation;

namespace MyMonolithicApp.Application.Auth
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Username).NotEmpty().WithMessage("login.emptyUserName");
            RuleFor(x => x.Password).NotEmpty().WithMessage("login.emptyPassword");
        }
    }
}
