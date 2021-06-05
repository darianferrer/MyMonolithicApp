using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using MyMonolithicApp.Infrastructure.Data;
using MyMonolithicApp.Infrastructure.Data.Entities;

namespace MyMonolithicApp.Infrastructure.Tests
{
    public static class MockHelpers
    {
        public static Mock<UserManager<TUser>> MockUserManager<TUser>() where TUser : class
        {
            var store = new Mock<IUserStore<TUser>>();
            var mgr = new Mock<UserManager<TUser>>(store.Object, null, null, null, null, null, null, null, null);
            mgr.Object.UserValidators.Add(new UserValidator<TUser>());
            mgr.Object.PasswordValidators.Add(new PasswordValidator<TUser>());
            return mgr;
        }

        public static Mock<SignInManager<TUser>> MockSignInManager<TUser>(UserManager<TUser> userManager) where TUser : class
        {
            var mgr = new Mock<SignInManager<TUser>>(userManager,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<UserEntity>>(),
                Mock.Of<IOptions<IdentityOptions>>(),
                null,
                Mock.Of<IAuthenticationSchemeProvider>(),
                new DefaultUserConfirmation<UserEntity>());
            return mgr;
        }

        public static ApplicationDbContext MockApplicationDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("MyMonolithicApp")
                .Options;
            return new ApplicationDbContext(options);
        }
    }
}
