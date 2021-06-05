using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using MyMonolithicApp.Acceptance.Tests.Contracts;
using MyMonolithicApp.Acceptance.Tests.MockedData;
using MyMonolithicApp.Infrastructure.Data;
using MyMonolithicApp.Infrastructure.Data.Entities;

namespace MyMonolithicApp.Acceptance.Tests.Features.Token
{
    public static class TokenTestsSteps
    {
        public static void CleanDatabase(this Scenario scenario)
        {
            var dbContext = scenario.GetService<ApplicationDbContext>();

            var role = dbContext.Roles.FirstOrDefault(x => x.Name == Data._testRoleName);
            if (role != null)
            {
                dbContext.Roles.Remove(role);
            }

            var user = dbContext.Users.FirstOrDefault(x => x.Username == Data._testUsername);
            if (user != null)
            {
                dbContext.Users.Remove(user);
            }

            var permission = dbContext.Permissions.FirstOrDefault(x => x.Code == Data._testPermissionCode);
            if (permission != null)
            {
                dbContext.Permissions.Remove(permission);
            }

            dbContext.SaveChanges();
        }

        public static async Task<Scenario<TokenSubmitTestContext>> Given_UserInDatabase(this Scenario scenario)
        {
            var context = await AddMockData(scenario);
            return scenario.SetContext(context);
        }

        public static async Task<Scenario<TokenSuccessResponseTestContext>> When_SubmitValidCredentials(this Task<Scenario<TokenSubmitTestContext>> scenarioTask)
        {
            var scenario = await scenarioTask;
            var contract = new { scenario.Context.Username, scenario.Context.Password };
            return await scenario.When_SendContract("/token/connect",
                HttpMethod.Post,
                contract,
                response => new TokenSuccessResponseTestContext(contract.Username, Data._testUserEmail, response));
        }

        public static async Task<Scenario<TokenFailResponseTestContext>> When_SubmitInvalidCredentials(this Task<Scenario<TokenSubmitTestContext>> scenarioTask)
        {
            var scenario = await scenarioTask;
            var contract = new { scenario.Context.Username, Password = Guid.NewGuid().ToString() };
            var error = new Error("Correctable", "login.invalidCredentials", "Credentials are not valid");
            return await scenario.When_SendContract("/token/connect",
                HttpMethod.Post,
                contract,
                response => new TokenFailResponseTestContext(new ErrorResponse(new[] { error }), response));
        }

        public static async Task<Scenario<TokenSuccessResponseTestContext>> Then_ReceiveToken(this Task<Scenario<TokenSuccessResponseTestContext>> scenarioTask)
        {
            var scenario = await scenarioTask;
            scenario.Then_ReceiveStatus(HttpStatusCode.OK);
            await scenario.Then_ResponseMatches(new { scenario.Context.Username, scenario.Context.Email });
            return scenario;
        }

        public static async Task<Scenario<TokenFailResponseTestContext>> Then_ReceiveBadRequest(this Task<Scenario<TokenFailResponseTestContext>> scenarioTask)
        {
            var scenario = await scenarioTask;
            scenario.Then_ReceiveStatus(HttpStatusCode.BadRequest);
            await scenario.Then_ResponseMatches(scenario.Context.ErrorResponse);
            return scenario;
        }

        private static async Task<TokenSubmitTestContext> AddMockData(Scenario scenario)
        {
            var dbContext = scenario.GetService<ApplicationDbContext>();
            var lookupNormalizer = scenario.GetService<ILookupNormalizer>();
            var passwordHasher = scenario.GetService<IPasswordHasher<UserEntity>>();

            var testUserPassword = "1234*Abcd";

            var testPermission = new PermissionEntity
            {
                Code = Data._testPermissionCode,
                Name = "Test Permission",
            };
            var testRole = new RoleEntity
            {
                Id = Guid.NewGuid(),
                Name = Data._testRoleName,
                ConcurrencyStamp = Guid.NewGuid().ToString(),
            };
            testRole.NormalisedName = lookupNormalizer.NormalizeName(testRole.Name);
            var rolePermission = new RolePermissionEntity
            {
                PermissionCode = testPermission.Code,
                Permission = testPermission,
                RoleId = testRole.Id,
                Role = testRole,
            };
            var testUser = new UserEntity
            {
                Id = Guid.NewGuid(),
                Email = Data._testUserEmail,
                Username = Data._testUsername,
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                RoleId = testRole.Id,
                Role = testRole,
            };
            testUser.NormalisedUsername = lookupNormalizer.NormalizeName(Data._testUsername);
            testUser.PasswordHash = passwordHasher.HashPassword(testUser, testUserPassword);

            dbContext.Users.Add(testUser);
            dbContext.Roles.Add(testRole);
            dbContext.RolesPermissions.Add(rolePermission);
            dbContext.Permissions.Add(testPermission);

            await dbContext.SaveChangesAsync();

            return new TokenSubmitTestContext(Data._testUsername, testUserPassword);
        }
    }
}
