

namespace BlogMVC.Services
{
    public interface IServicioChat
    {
        Task<string> GenerarCuerpo(string titulo);
        IAsyncEnumerable<string> GenerarCuerpoStream(string titulo);
    }
}