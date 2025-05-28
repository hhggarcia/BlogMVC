
using BlogMVC.Services;

namespace BlogMVC.Jobs
{
    public class AnalisisSentimientos : BackgroundService
    {
        private readonly IServiceProvider serviceProvider;

        public AnalisisSentimientos(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var analisis = scope.ServiceProvider.GetRequiredService<IAnalisisSentimientos>();
                    await analisis.AnalizarComentariosPendientes();
                    await analisis.ProcesarLotesPendinetes();
                }
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
