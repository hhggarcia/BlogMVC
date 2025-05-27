
namespace BlogMVC.Services
{
    public interface IServicioImagenes
    {
        Task<byte[]> GenerarPortadaEntrada(string titulo);
    }
}