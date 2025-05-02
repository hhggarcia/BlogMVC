namespace BlogMVC.Models
{
    public class UsersRolesUserVM
    {
        public required string UserId { get; set; }
        public required string Email { get; set; }
        public IEnumerable<UserRoleVM> Roles { get; set; } = [];
    }
}
