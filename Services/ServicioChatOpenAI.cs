using BlogMVC.Config;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace BlogMVC.Services
{
    public class ServicioChatOpenAI : IServicioChat
    {
        private readonly IOptions<ConfiguracionesIA> _options;
        private readonly OpenAIClient _clientOpenAI;
        private readonly string systemPromptGenerarCuerpo = """
            Eres un ingeniero de software experto en .NET Core.
            Escribes articulos con un tono jovial y amigable.
            Te esfuerzas para que los principiantes entiendan las cosas dando ejemplos practicos
            """;
        private string ObtenerPromptGenerarCuerpo(string titulo) => $"""
            Crear un articulo para un blog. El titulo del articulo sera {titulo}.
            Si lo entiendes conveniente, debes insertar tips.

            El formato de respuesta es HTML. Por tanto, debes colocar negritas donde consideres,
            titulos, subtitulos, entre otras cosas que ayuden a resaltar el formato.

            La respuesta no debe ser un documento HTML, sino solamente el articulo en formato HTML,
            con sus parrafos bien separados. Por tanto, nada de DOCTYPE, ni head, ni body. Solo el articulo.

            No incluyas el titulo del articulo en el articulo.
            """;
        public ServicioChatOpenAI(IOptions<ConfiguracionesIA> options, OpenAIClient clientOpenAI)
        {
            _options = options;
            _clientOpenAI = clientOpenAI;
        }

        public async Task<string> GenerarCuerpo(string titulo)
        {
            var modeloTexto = _options.Value.ModeloTexto;
            var clienteChat = _clientOpenAI.GetChatClient(modeloTexto);

            var mensajeSistema = new SystemChatMessage(systemPromptGenerarCuerpo);
            var promptUsuario = ObtenerPromptGenerarCuerpo(titulo);
            var mensajeUsuario = new UserChatMessage(promptUsuario);

            ChatMessage[] mensajes = { mensajeSistema, mensajeUsuario };
            var respuesta = await clienteChat.CompleteChatAsync(mensajes);
            var cuerpo = respuesta.Value.Content[0].Text;

            return cuerpo;
        }

        public async IAsyncEnumerable<string> GenerarCuerpoStream(string titulo)
        {
            var modeloTexto = _options.Value.ModeloTexto;
            var clienteChat = _clientOpenAI.GetChatClient(modeloTexto);

            var mensajeSistema = new SystemChatMessage(systemPromptGenerarCuerpo);
            var promptUsuario = ObtenerPromptGenerarCuerpo(titulo);
            var mensajeUsuario = new UserChatMessage(promptUsuario);
            ChatMessage[] mensajes = { mensajeSistema, mensajeUsuario };

            await foreach (var completionUpdate in clienteChat.CompleteChatStreamingAsync(mensajes))
            {
                foreach (var contenido in completionUpdate.ContentUpdate)
                {
                    yield return contenido.Text;
                }

            }
        }

    }
}
