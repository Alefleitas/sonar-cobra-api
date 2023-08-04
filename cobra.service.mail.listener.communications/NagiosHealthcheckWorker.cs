using cobra.service.mail.listener.communications.Configuration;
using Microsoft.Extensions.Options;

namespace cobra.service.mail.listener.communications
{
    internal class NagiosHealthcheckWorker : BackgroundService
    {
        private readonly ServiciosConfiguration _servicios;
        private readonly IConfiguration _configuration;
        private readonly int intervaloMilisegundos;

        public NagiosHealthcheckWorker(IOptions<ServiciosConfiguration> servicios, IConfiguration configuration)
        {
            _servicios = servicios.Value;
            _configuration = configuration;
            intervaloMilisegundos = _configuration.GetValue<int>("Monitoreo:SegundosIntervaloHealthcheck") * 1000;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                Nordelta.Monitoreo.Monitor.Ok("El servicio de Email está ejecutándose correctamente", _servicios.Healthcheck);
                await Task.Delay(intervaloMilisegundos, stoppingToken).ConfigureAwait(false);
            }
        }
    }
}