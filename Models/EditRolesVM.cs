namespace BlogMVC.Models
{
    public class EditRolesVM
    {
        public required string UserId { get; set; }
        public List<string> RolesSelects { get; set; } = [];
    }
}
