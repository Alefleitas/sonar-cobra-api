using cobra.service.mail.listener.communications;
using cobra.service.mail.listener.communications.Configuration;
using cobra.service.mail.listener.communications.Services;
using Serilog;
using Nordelta.Monitoreo.Nagios;
using Monitoreo = Nordelta.Monitoreo;
using cobra.service.mail.listener.communications.Utils;

using IHost host = Host.CreateDefaultBuilder(args)
    .UseSerilog()
    .ConfigureServices((hostContext, services) =>
    {
        IConfiguration configuration = hostContext.Configuration;

        services.Configure<CobraApiConfiguration>(configuration.GetSection(nameof(CobraApiConfiguration)));
        services.Configure<EmailSenderConfig>(configuration.GetSection(nameof(EmailSenderConfig)));

        services.Configure<EmailListenerConfig>(EmailListenerConfig.EmailListenerSmtp, configuration.GetSection("EmailListenerSmtp"));
        services.Configure<EmailListenerConfig>(EmailListenerConfig.EmailListenerImap, configuration.GetSection("EmailListenerImap"));

        services.Configure<AzureAdCredentialConfig>(AzureAdCredentialConfig.AzureAdEmailListener, configuration.GetSection("AzureAdCredentialSettings"))
                .Configure<AzureAdCredentialConfig>(AzureAdCredentialConfig.AzureAdEmailListener, x =>
                {
                    x.Password = AesManager.GetPassword(x.Password, configuration.GetSection("SecretKeyUser").Value);
                    x.ClientId = AesManager.GetPassword(x.ClientId, configuration.GetSection("SecretKeyCredential").Value);
                    x.TenantId = AesManager.GetPassword(x.TenantId, configuration.GetSection("SecretKeyCredential").Value);
                    x.ClientSecret = AesManager.GetPassword(x.ClientSecret, configuration.GetSection("SecretKeyCredential").Value);
                    x.RequesUri = x.RequesUri.Replace("TENANT_ID", x.TenantId);
                });

        services.Configure<ServiciosConfiguration>(configuration.GetSection("Monitoreo:Servicios"));
        services.AddNagiosMonitoring(configuration.GetSection("Monitoreo:NagiosConfig"));

        string nombreSistema = configuration.GetSection("Monitoreo:NombreSistema").Value;

        services.AddSingleton<IEmailListenerService, EmailListenerService>();
        services.AddTransient<ICommunicationService, CommunicationService>();


        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        Log.Logger.Debug($"Environment.CurrentDirectory : {Environment.CurrentDirectory}");
        Log.Logger.Debug($"AppDomain.CurrentDomain.BaseDirectory:{AppDomain.CurrentDomain.BaseDirectory}");

        services.AddSingleton(Log.Logger);

        ServiceProvider serviceProvider = services.BuildServiceProvider();

        var configuradorMonitor = new Monitoreo.ConfiguradorMonitor()
            .ConfigurarLogger(serviceProvider.GetService<Serilog.ILogger>())
            .ConfigurarCheck(serviceProvider.GetService<Monitoreo.ICheck>());

        Monitoreo.Monitor.Instancia = configuradorMonitor.CrearMonitor(nombreSistema);

        services.AddHostedService<MailWorker>();
        services.AddHostedService<NagiosHealthcheckWorker>();
    }).UseWindowsService(options =>
    {
        options.ServiceName = "CommunicationMailListener Worker Service";
    })
    .Build();

await host.RunAsync();
