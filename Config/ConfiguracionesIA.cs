using System.ComponentModel.DataAnnotations;

namespace BlogMVC.Config
{
    public class ConfiguracionesIA
    {
        public const string Seccion = "ConfiguracionesIA";
        [Required]
        public required string ModeloTexto { get; set; }
        [Required]
        public required string ModeloImagenes { get; set; }
        public required string ModeloSentimientos { get; set; }
        [Required]
        public required string KeyOpenAI { get; set; }
    }
}
