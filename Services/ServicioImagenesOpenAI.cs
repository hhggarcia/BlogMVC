using BlogMVC.Config;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Images;

namespace BlogMVC.Services
{
    public class ServicioImagenesOpenAI : IServicioImagenes
    {
        private readonly IOptions<ConfiguracionesIA> _options;
        private readonly OpenAIClient _clientOpenAI;

        public ServicioImagenesOpenAI(IOptions<ConfiguracionesIA> options, OpenAIClient openAIClient)
        {
            _options = options;
            _clientOpenAI = openAIClient;
        }

        public async Task<byte[]> GenerarPortadaEntrada(string titulo)
        {
            string prompt = $"""
                Una imagen foto-realista inspirada en el tema '{titulo}'.
                La escena debe reflejar el concepto central del articulo con una composicion atractiva.
                La iluminacion debe ser natural y realista, con una profundidad de campo bien definida.
                La imagen debe ser moderna y tecnologica, evitando cliches exagerados como fondos de neon 
                o codigo flotante en el aire.
                """;

            var imagenGenerationOptions = new ImageGenerationOptions
            {
                Quality = GeneratedImageQuality.Standard,
                Size = GeneratedImageSize.W1792xH1024,
                Style = GeneratedImageStyle.Natural,
                ResponseFormat = GeneratedImageFormat.Bytes
            };

            var modeloImagenes = _options.Value.ModeloImagenes;
            var clienteImagenes = _clientOpenAI.GetImageClient(modeloImagenes);
            var imagenGenerada = await clienteImagenes.GenerateImageAsync(prompt, imagenGenerationOptions);
            var bytes = imagenGenerada.Value.ImageBytes.ToArray();
            return bytes;
        }
    }
}
