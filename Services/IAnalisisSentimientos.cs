
namespace BlogMVC.Services
{
    public interface IAnalisisSentimientos
    {
        Task AnalizarComentariosPendientes();
        Task ProcesarLotesPendinetes();
    }
}