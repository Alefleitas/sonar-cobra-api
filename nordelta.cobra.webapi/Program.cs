using System.Globalization;
using System.Security.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace nordelta.cobra.webapi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var defaultCultureInfo = new CultureInfo("es-AR", false);
            CultureInfo.CurrentCulture = defaultCultureInfo;
            CultureInfo.CurrentUICulture = defaultCultureInfo;
            CultureInfo.DefaultThreadCurrentCulture = defaultCultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = defaultCultureInfo;

            CreateWebHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateWebHostBuilder(string[] args) =>
           Host.CreateDefaultBuilder(args)
               .ConfigureWebHostDefaults(webBuilder =>
               {
                   webBuilder.UseStartup<Startup>();
                   webBuilder.ConfigureKestrel(serverOptions => {
                        serverOptions.ConfigureHttpsDefaults(listenOptions =>
                        {
                            listenOptions.ClientCertificateMode = ClientCertificateMode.AllowCertificate;
                            listenOptions.AllowAnyClientCertificate();
                            listenOptions.ClientCertificateValidation =
                                (cert, chain, policyErrors) => true;

                            listenOptions.SslProtocols = SslProtocols.Tls12;
                        });
                   });
               })
               .ConfigureLogging(loggingConfiguration => loggingConfiguration.ClearProviders())
               .UseSerilog((hostingContext, loggerConfiguration) =>
               {
                   loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration);
               });
    }
}
