using System;
using System.Collections.Generic;
using System.Linq;
using MyMonolithicApp.Domain.Users;

namespace MyMonolithicApp.Infrastructure.Data.Entities
{
    public class RoleEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string NormalisedName { get; set; }
        public virtual ICollection<RolePermissionEntity> Permissions { get; set; }
        public string ConcurrencyStamp { get; set; }

        public static Role ToModel(RoleEntity role) =>
            new(role.Id, role.Name, role.Permissions?.Select(p => PermissionEntity.ToModel(p.Permission)).ToList());
    }
}
