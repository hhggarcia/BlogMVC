namespace BlogMVC.Models
{
    public class HomeIndexEntradasVM
    {
        public int Id { get; set; }
        public required string Titulo { get; set; }
        public string? PortadaUrl { get; set; }
        public DateTime FechaPublicacion { get; set; }
    }
}
