using System;

namespace MyMonolithicApp.Domain.Users
{
    public record User(Guid Id, string Username, string Email, Role Role);
}
