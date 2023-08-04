using System.Globalization;
using nordelta.cobra.service.quotations;
using nordelta.cobra.service.quotations.Configuration;
using nordelta.cobra.service.quotations.Jobs;
using nordelta.cobra.service.quotations.Services;
using nordelta.cobra.service.quotations.Services.Contracts;
using nordelta.cobra.service.quotations.Utils;
using Quartz;
using Quartz.Impl.Calendar;
using Serilog;
using Nordelta.Monitoreo.Nagios;
using nordelta.cobra.service.quotations.Models.InvertirOnline.Service.Contract;
using nordelta.cobra.service.quotations.Models.InvertirOnline.Helper;

var defaultCultureInfo = new CultureInfo("es-AR", false);
CultureInfo.CurrentCulture = defaultCultureInfo;
CultureInfo.CurrentUICulture = defaultCultureInfo;
CultureInfo.DefaultThreadCurrentCulture = defaultCultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = defaultCultureInfo;

using IHost host =  Host.CreateDefaultBuilder(args)
    .UseContentRoot(AppDomain.CurrentDomain.BaseDirectory)
    .ConfigureLogging(loggingConfiguration => loggingConfiguration.ClearProviders())
    .UseSerilog((hostingContext, loggerConfiguration) =>
    {
        loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration);
    })
    .ConfigureServices((hostContext, services) =>
    {
        IConfiguration configuration = hostContext.Configuration;

        services.Configure<CobraApiConfiguration>(configuration.GetSection("CobraApi"));
        services.Configure<InvertirOnlineConfiguration>(configuration.GetSection("QuotationConfiguration:InvertirOnlineApi"));
        services.Configure<DolarHoyConfiguration>(configuration.GetSection("QuotationConfiguration:WebDolarHoy"));
        services.Configure<OMSConfiguration>(configuration.GetSection("QuotationConfiguration:OMSApi"));
        services.Configure<HolidayApiConfiguration>(configuration.GetSection("HolidayApi"));
        services.Configure<DolarMepEspeciesConfiguration>(configuration.GetSection("QuotationConfiguration:DolarMepEspecies"));

        services.Configure<QuotationBcuConfiguration>(configuration.GetSection("QuotationConfiguration:QuotationBcu"));
        services.Configure<QuotationBnaConfiguration>(configuration.GetSection("QuotationConfiguration:QuotationBna"));
        services.Configure<QuotationBcraConfiguration>(configuration.GetSection("QuotationConfiguration:QuotationBcra"));
        services.Configure<QuotationCacConfiguration>(configuration.GetSection("QuotationConfiguration:QuotationCac"));
        services.Configure<QuotationCoinApiConfiguration>(configuration.GetSection("QuotationConfiguration:QuotationCoinApi"));
        services.Configure<ServiciosConfiguration>(configuration.GetSection("Monitoreo:Servicios"));

        services.AddTransient<IQuotationService, QuotationService>();
        services.AddTransient<IQuoteService, FinanceQuotationsService>();
        services.AddTransient<IHolidayService, HolidayService>();
        services.AddScoped<ITokenService, TokenService>();

        //Log.Logger = new LoggerConfiguration()
        //    .ReadFrom
        //    .Configuration(hostContext.Configuration)
        //    .CreateLogger();
        //Log.Logger.Debug($"Environment.CurrentDirectory : {Environment.CurrentDirectory}");
        //Log.Logger.Debug($"AppDomain.CurrentDomain.BaseDirectory:{AppDomain.CurrentDomain.BaseDirectory}");

        services.AddSingleton(Log.Logger);
        services.AddNagiosMonitoring(configuration.GetSection("Monitoreo:NagiosConfig"));
        string nombreSistema = configuration.GetSection("Monitoreo:NombreSistema").Value;
        int segundosIntervaloHealthcheck = configuration.GetSection("Monitoreo:SegundosIntervaloHealthcheck").Get<int>();
        if (segundosIntervaloHealthcheck == 0)
            segundosIntervaloHealthcheck = 300;

        IServiceProvider serviceProvider = services.BuildServiceProvider();

        Nordelta.Monitoreo.Monitor.Instancia = new Nordelta.Monitoreo.ConfiguradorMonitor()
            .ConfigurarLogger(serviceProvider.GetService<Serilog.ILogger>())
            .ConfigurarCheck(serviceProvider.GetService<Nordelta.Monitoreo.ICheck>())
            .CrearMonitor(nombreSistema);


        services.AddQuartz(q =>
        {
            q.SchedulerId = "Scheduler-Core";
            q.UseMicrosoftDependencyInjectionJobFactory();

            q.UseSimpleTypeLoader();
            q.UseInMemoryStore();
            q.UseDefaultThreadPool(tp =>
            {
                tp.MaxConcurrency = 10;
            });

            var jobKeyQuotes = new JobKey("quick access quotations job", "quotation group");
            q.AddJob<FinanceQuotationsJob>(jobKeyQuotes, j => j
                .WithDescription("Get quotations for quick access")
            );

            var jobKeyDolarMep = new JobKey("dolar mep job", "quotation group");
            q.AddJob<DolarMepJob>(jobKeyDolarMep, j => j
                .WithDescription("Get dolar MEP quotation")
            );

            var jobKeyBcu = new JobKey("quotation bcu job", "quotation group");
            q.AddJob<QuotationBcuJob>(jobKeyBcu, j => j
                .WithDescription("Get quotations from BCU")
            );

            var jobKeyBna = new JobKey("quotation bna job", "quotation group");
            q.AddJob<QuotationBnaJob>(jobKeyBna, j => j
                .WithDescription("Get USD and EUR quotations from BNA.")
            );

            var jobKeyBcra = new JobKey("quotation bcra job", "quotation group");
            q.AddJob<QuotationBcraJob>(jobKeyBcra, j => j
                .WithDescription("Get USD Mayorista Com. A3500 quotations from BCRA.")
            );

            var jobKeyCac = new JobKey("quotation cac job", "quotation group");
            q.AddJob<CacJob>(jobKeyCac, j => j
                .WithDescription("Get cac quote from Camarco")
            );

            var jobKeyCoinApi = new JobKey("quotation coinapi job", "quotation group");
            q.AddJob<QuotationCoinApiJob>(jobKeyCoinApi, j => j
                .WithDescription("Get cryptocurrency quotations from ConApi")
            );

            var jobHealthcheck = new JobKey("nagios healthckeck", "monitoring");
            q.AddJob<HealthcheckJob>(jobHealthcheck, j => j
                .WithDescription("Send frequent helthchecks for this service to NagiosXI")
            );

            const string holidayCalendarName = "myHolidayCalendar";
            
            q.AddCalendar<HolidayCalendar>(
                name: holidayCalendarName,
                replace: true,
                updateTriggers: true,
                x =>
                {
                    var holidayService = serviceProvider.GetService<IHolidayService>();
                    if (holidayService is null) return;
                    var holidays = holidayService.GetHolidaysAsync().GetAwaiter().GetResult();
                    foreach (var holiday in holidays)
                    {
                        x.AddExcludedDate(new DateTime(holiday.Year, holiday.Month, holiday.Day));
                    }
                }
            );

            q.AddTrigger(t => t
              .WithIdentity("Chrono Trigger FinanceQuotations")
              .ForJob(jobKeyQuotes)
              .StartAt(DateBuilder.EvenSecondDate(DateTimeOffset.UtcNow.AddSeconds(5)))
              .WithCronSchedule(configuration[$"Quartz:{nameof(FinanceQuotationsJob)}"], opt => opt.InTimeZone(LocalDateTime.GetTimeZone("Argentina Standard Time")))
              .WithDescription("chrono time interval trigger")
              .ModifiedByCalendar(holidayCalendarName)
            );
            
            q.AddTrigger(t => t
                .WithIdentity("Chrono Trigger DolarMep")
                .ForJob(jobKeyDolarMep)
                .StartAt(DateBuilder.EvenSecondDate(DateTimeOffset.UtcNow.AddSeconds(5)))
                .WithCronSchedule(configuration[$"Quartz:{nameof(DolarMepJob)}"], opt => opt.InTimeZone(LocalDateTime.GetTimeZone("Argentina Standard Time")))
                .WithDescription("chrono time interval trigger")
                .ModifiedByCalendar(holidayCalendarName)
            );

            q.AddTrigger(t => t
                .WithIdentity("Chrono Trigger QuotationBcu")
                .ForJob(jobKeyBcu)
                .StartAt(DateBuilder.EvenSecondDate(DateTimeOffset.UtcNow.AddSeconds(5)))
                .WithCronSchedule(configuration[$"Quartz:{nameof(QuotationBcuJob)}"], opt => opt.InTimeZone(LocalDateTime.GetTimeZone("Argentina Standard Time")))
                .WithDescription("chrono time interval trigger")
                .ModifiedByCalendar(holidayCalendarName)
            );

            q.AddTrigger(t => t
                .WithIdentity("Chrono Trigger QuotationBna")
                .ForJob(jobKeyBna)
                .StartAt(DateBuilder.EvenSecondDate(DateTimeOffset.UtcNow.AddSeconds(5)))
                .WithCronSchedule(configuration[$"Quartz:{nameof(QuotationBnaJob)}"], opt => opt.InTimeZone(LocalDateTime.GetTimeZone("Argentina Standard Time")))
                .WithDescription("chrono time interval trigger")
                .ModifiedByCalendar(holidayCalendarName)
            );

            q.AddTrigger(t => t
                .WithIdentity("Chrono Trigger QuotationBcra")
                .ForJob(jobKeyBcra)
                .StartAt(DateBuilder.EvenSecondDate(DateTimeOffset.UtcNow.AddSeconds(5)))
                .WithCronSchedule(configuration[$"Quartz:{nameof(QuotationBcraJob)}"], opt => opt.InTimeZone(LocalDateTime.GetTimeZone("Argentina Standard Time")))
                .WithDescription("chrono time interval trigger")
                .ModifiedByCalendar(holidayCalendarName)
            );

            q.AddTrigger(t => t
                .WithIdentity("Chrono Trigger CAC")
                .ForJob(jobKeyCac)
                .StartAt(DateBuilder.EvenSecondDate(DateTimeOffset.UtcNow.AddSeconds(5)))
                .WithCronSchedule(configuration[$"Quartz:{nameof(CacJob)}"], opt => opt.InTimeZone(LocalDateTime.GetTimeZone("Argentina Standard Time")))
                .WithDescription("chrono time interval trigger")
                .ModifiedByCalendar(holidayCalendarName)
            );

            q.AddTrigger(t => t
                .WithIdentity("Chrono Trigger CoinApi")
                .ForJob(jobKeyCoinApi)
                .StartAt(DateBuilder.EvenSecondDate(DateTimeOffset.UtcNow.AddSeconds(5)))
                .WithCronSchedule(configuration[$"Quartz:{nameof(QuotationCoinApiJob)}"], opt => opt.InTimeZone(LocalDateTime.GetTimeZone("Argentina Standard Time")))
                .WithDescription("chrono time interval trigger")
                .ModifiedByCalendar(holidayCalendarName)
            );

            q.AddTrigger(t => t
                .WithIdentity("Simple Trigger send healthcheck")
                .ForJob(jobHealthcheck)
                .StartAt(DateBuilder.EvenSecondDate(DateTimeOffset.UtcNow.AddSeconds(4)))
                .WithSimpleSchedule(sb => sb
                    .WithIntervalInSeconds(segundosIntervaloHealthcheck)
                    .RepeatForever())
                .WithDescription("NagiosXI Healthckeck send interval trigger")
            );

            q.UseTimeZoneConverter();
        });
     
        services.AddQuartzHostedService(
            q => q.WaitForJobsToComplete = true);

    })
    //.UseWindowsService(options =>
    //{
    //    options.ServiceName = "Quotation Worker Service";
    //})
    .ConfigureWebHostDefaults(builder =>
    {
        builder.UseStartup<Startup>();
    })
    .Build();

await host.RunAsync();
