using System;
using MyMonolithicApp.Domain.Users;

namespace MyMonolithicApp.Infrastructure.Data.Entities
{
    public class UserEntity
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string NormalisedUsername { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public Guid RoleId { get; set; }
        public virtual RoleEntity Role { get; set; }
        public string ConcurrencyStamp { get; set; }

        public static User ToModel(UserEntity user) =>
            new(user.Id, user.Username, user.Email, RoleEntity.ToModel(user.Role));
    }
}
