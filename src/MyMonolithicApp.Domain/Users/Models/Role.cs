using System;
using System.Collections.Generic;

namespace MyMonolithicApp.Domain.Users
{
    public record Role(Guid Id, string Name, ICollection<Permission>? Permissions);
}
