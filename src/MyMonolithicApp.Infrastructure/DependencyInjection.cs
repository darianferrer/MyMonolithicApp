using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MyMonolithicApp.Domain.Auth;
using MyMonolithicApp.Domain.Users;
using MyMonolithicApp.Infrastructure.Auth;
using MyMonolithicApp.Infrastructure.Data;
using MyMonolithicApp.Infrastructure.Data.Entities;
using MyMonolithicApp.Infrastructure.Data.Stores;
using MyMonolithicApp.Infrastructure.UserManagement;
using PermissionBasedAuthorisation;

namespace MyMonolithicApp.Infrastructure
{
    public static class DependencyInjection
    {
        private class AuthConfig
        {
            public string TokenKey { get; set; }
            public string Issuer { get; set; }
            public string Audience { get; set; }
        }

        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            var authConfig = new AuthConfig();
            config.GetSection("Auth")
                .Bind(authConfig);

            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(authConfig.TokenKey));
            var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            var tokenValidationParameters = new TokenValidationParameters
            {
                // The signing key must match!
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingCredentials.Key,
                // Validate the JWT Issuer (iss) claim
                ValidateIssuer = true,
                ValidIssuer = authConfig.Issuer,
                // Validate the JWT Audience (aud) claim
                ValidateAudience = true,
                ValidAudience = authConfig.Audience,
                // Validate the token expiry
                ValidateLifetime = true,
                // If you want to allow a certain amount of clock drift, set that here:
                ClockSkew = TimeSpan.Zero
            };

            services.Configure<JwtIssuerOptions>(options =>
            {
                options.Issuer = authConfig.Issuer;
                options.Audience = authConfig.Audience;
                options.SigningCredentials = signingCredentials;
            });
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = tokenValidationParameters;
                });
            services.AddAuthorization(config =>
            {
                var defaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .Build();
                config.DefaultPolicy = defaultPolicy;
            });
            services.AddPbacCore<PermissionVerificationService>();

            services.AddDbContextPool<ApplicationDbContext>(options
                => options.UseSqlServer(config.GetConnectionString("DefaultConnection"),
                    x => x.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            services.AddIdentityCore<UserEntity>()
                .AddUserStore<CustomUserStore>()
                .AddSignInManager()
                .AddRoles<RoleEntity>()
                .AddRoleStore<CustomRoleStore>()
                .AddRoleManager<RoleManager<RoleEntity>>()
                .AddDefaultTokenProviders();

            services.AddScoped<ISignInService, SignInService>();
            services.AddScoped<SecurityTokenHandler, JwtSecurityTokenHandler>();
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddScoped<IUserService, UserService>();

            return services;
        }
    }
}
