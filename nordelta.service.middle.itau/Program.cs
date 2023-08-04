using Microsoft.AspNetCore.Server.Kestrel.Https;
using nordelta.service.middle.itau;
using Serilog;
using Serilog.Events;
using System.Globalization;
using System.Security.Authentication;

public class Program
{
    public static void Main(string[] args)
    {
        var defaultCultureInfo = new CultureInfo("es-AR", false);
        CultureInfo.CurrentCulture = defaultCultureInfo;
        CultureInfo.CurrentUICulture = defaultCultureInfo;
        CultureInfo.DefaultThreadCurrentCulture = defaultCultureInfo;
        CultureInfo.DefaultThreadCurrentUICulture = defaultCultureInfo;
        Serilog.Debugging.SelfLog.Enable(msg => Console.WriteLine(msg));
       
        CreateWebHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateWebHostBuilder(string[] args) =>
     Host.CreateDefaultBuilder(args)
         .ConfigureWebHostDefaults(webBuilder =>
         {
             webBuilder.UseKestrel(serverOptions =>
             {
                 serverOptions.ConfigureHttpsDefaults(listenOptions =>
                 {
                     listenOptions.ClientCertificateMode = ClientCertificateMode.AllowCertificate;
                     listenOptions.AllowAnyClientCertificate();
                     listenOptions.ClientCertificateValidation =
                         (cert, chain, policyErrors) => true;

                     listenOptions.SslProtocols = SslProtocols.Tls12;
                 });
             });
             webBuilder.UseStartup<Startup>();
         })
        .ConfigureLogging(loggingConfiguration => loggingConfiguration.ClearProviders())
        .UseSerilog((hostingContext, loggerConfiguration) =>
        {
            loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration);
        });

}