namespace BlogMVC.Models
{
    public class UserListVM
    {
        public IEnumerable<UserVM> Users { get; set; } = [];
        public string? Mensaje { get; set; }
    }
}
