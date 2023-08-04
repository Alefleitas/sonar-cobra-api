using Microsoft.Extensions.Options;
using nordelta.cobra.service.quotations.Configuration;
using Quartz;
using Monitoreo = Nordelta.Monitoreo;

namespace nordelta.cobra.service.quotations.Jobs
{
    public class HealthcheckJob : IJob
    {
        public ServiciosConfiguration _servicios { get; set; }
        public HealthcheckJob(IOptions<ServiciosConfiguration> servicios)
        {
            _servicios = servicios.Value;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await Task.Run(() => Monitoreo.Monitor.Ok("Microservicio de cotizaciones de Cobra corriendo correctamente", _servicios.Healthcheck));
        }
    }
}
