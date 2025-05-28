namespace BlogMVC.Models
{
    public class EntradasComentariosNegativosVM
    {
        public IEnumerable<EntradaComentariosNegativosVM> Entradas { get; set; } = [];
    }

    public class EntradaComentariosNegativosVM
    {
        public int Id { get; set; }
        public required string Titulo { get; set; }
        public int CantidadComentariosNegativos { get; set; }
    }
}
