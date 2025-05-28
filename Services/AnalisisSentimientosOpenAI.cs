using BlogMVC.Config;
using BlogMVC.Data;
using BlogMVC.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BlogMVC.Services
{
    public class AnalisisSentimientosOpenAI : IAnalisisSentimientos
    {
        private readonly ApplicationDbContext context;
        private readonly IOptions<ConfiguracionesIA> options;
        private readonly HttpClient httpClient;

        public AnalisisSentimientosOpenAI(ApplicationDbContext context,
            IOptions<ConfiguracionesIA> options,
            HttpClient httpClient)
        {
            this.context = context;
            this.options = options;
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.Value.KeyOpenAI);
            this.httpClient = httpClient;
        }

        public async Task AnalizarComentariosPendientes()
        {
            var comentariosPendientesDeAnalisis = await context.Comments
                .Where(c => c.Puntuacion == null)
                .Take(1_000)
                .Select(c => new Comment() { Id = c.Id, Cuerpo = c.Cuerpo })
                .ToListAsync();

            if (!comentariosPendientesDeAnalisis.Any())
            {
                return;
            }

            string jsonContent = ArmarPeticiones(comentariosPendientesDeAnalisis);
            var inputFileId = await SubirArchivosPeticiones(jsonContent);
            var batchId = await EnviarBatch(inputFileId);
            await GuardarLoteYActualizarComentarios(comentariosPendientesDeAnalisis, batchId);
        }

        private string ArmarPeticiones(List<Comment> comentarios)
        {
            var configuracionesIA = options.Value;

            var requests = new StringBuilder();

            foreach (var comentario in comentarios)
            {
                var request = new
                {
                    custom_id = comentario.Id.ToString(),
                    method = "POST",
                    url = "v1/chat/completions",
                    body = new
                    {
                        model = configuracionesIA.ModeloSentimientos,
                        messages = new[]
                        {
                            new { role = "system", content = "Eres un analista de sentimientos. Devuelve una puntuacion del 1 al 5 segun la emocion expresada en el mensaje. Solo debes retornar la puntuacion y nada mas."},
                            new { role = "user", content = comentario.Cuerpo }
                        }
                    }
                };

                var requestJSON = JsonSerializer.Serialize(request);
                requests.AppendLine(requestJSON);
            }

            var jsonContent = requests.ToString();

            return jsonContent;
        }

        private async Task<string> SubirArchivosPeticiones(string jsonContent)
        {
            var configIA = options.Value;

            using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonContent));
            using var fileContent = new StreamContent(memoryStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            using var form = new MultipartFormDataContent();
            form.Add(fileContent, "file", "batch_comentarios.jsonl");
            form.Add(new StringContent("batch"), "purpose");

            HttpResponseMessage respuestaSubidaArhivo = await httpClient.PostAsync("https://api.openai.com/v1/files", form);

            string responseContent = await respuestaSubidaArhivo.Content.ReadAsStringAsync();

            var inputFileId = JsonDocument.Parse(responseContent).RootElement.GetProperty("id").GetString();

            return inputFileId!;
        }

        private async Task<string> EnviarBatch(string inputFileId)
        {
            var configIA = options.Value;

            var batchRequest = new
            {
                input_file_id = inputFileId,
                endpoint = "v1/chat/completions",
                completion_window = "24h"
            };

            var jsonRequest = JsonSerializer.Serialize(batchRequest);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("https://api.openai.com/v1/batches", content);
            var jsonResponse = await response.Content.ReadAsStringAsync();

            var batchId = JsonDocument.Parse(jsonResponse).RootElement.GetProperty("id").GetString();
            return batchId!;
        }

        private async Task GuardarLoteYActualizarComentarios(List<Comment> comentarios, string batchId)
        {
            var lote = new Lote()
            {
                Id = batchId,
                Estatus = "in_progress",
                FechaCreacion = DateTime.UtcNow
            };

            context.Add(lote);
            await context.SaveChangesAsync();

            var idsComentarios = comentarios.Select(c => c.Id);

            await context.Comments
                .Where(c => idsComentarios.Contains(c.Id))
                .ExecuteUpdateAsync(setters => setters.SetProperty(c => c.Puntuacion, -1));
        }

        public async Task ProcesarLotesPendinetes()
        {
            var lotesSinProcesar = await context.Lotes
                .Where(c => c.Estatus != "failed" && c.Estatus != "completed" && c.Estatus != "expired")
                .ToListAsync();

            if (!lotesSinProcesar.Any())
            {
                return;
            }

            var configAI = options.Value;
            foreach (var lote in lotesSinProcesar)
            {
                var response = await httpClient.GetAsync($"https://api.openai.com/v1/batches/{lote.Id}");
                var responseString = await response.Content.ReadAsStringAsync();
                var respuestaJSON = JsonDocument.Parse(responseString).RootElement;

                var status = respuestaJSON.GetProperty("status").GetString();

                lote.Estatus = status!;

                if (status == "completed")
                {
                    var outputFileId = respuestaJSON.GetProperty("output_file_id").GetString();

                    var respuestaContenidoArchivo = await httpClient.GetAsync($"https://api.openai.com/v1/files/{outputFileId}/content");

                    var respuestaContentString = await respuestaContenidoArchivo.Content.ReadAsStringAsync();

                    var lineas = respuestaContentString.Split('\n', StringSplitOptions.RemoveEmptyEntries);

                    foreach (var linea in lineas)
                    {
                        var json = JsonDocument.Parse(linea).RootElement;
                        var comentarioId = int.Parse(json.GetProperty("custom_id").GetString()!);
                        var puntuacion = int.Parse(json.GetProperty("response")
                            .GetProperty("body")
                            .GetProperty("choices")
                            .EnumerateArray().First()
                            .GetProperty("message")
                            .GetProperty("content")
                            .GetString()!);

                        var comentario = new Comment { Id = comentarioId };
                        context.Attach(comentario);
                        comentario.Puntuacion = puntuacion;
                    }
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
