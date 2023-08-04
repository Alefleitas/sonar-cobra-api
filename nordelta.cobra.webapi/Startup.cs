using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using nordelta.cobra.webapi.Repositories.Contexts;
using nordelta.cobra.webapi.Services.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nordelta.HttpRequestMiddleware;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using System.Threading.Tasks;
using nordelta.cobra.webapi.Controllers.ActionFilters;
using Hangfire.SqlServer;
using Microsoft.Extensions.Caching.Distributed;
using System.Text;
using System.Text.Json;
using nordelta.cobra.webapi.Utils;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Monitoreo = Nordelta.Monitoreo;
using nordelta.cobra.webapi.Websocket;

namespace nordelta.cobra.webapi
{

    public class Startup
    {
        public static IConfiguration StaticConfig { get; private set; }
        public IConfiguration Configuration { get; }
        private const string CrossOriginResourceSharingPolicy = "CrossOriginResourceSharingPolicy";
        private const int MaxDefaultWorkerCount = 20;

        public Startup(IConfiguration configuration, IWebHostEnvironment hostEnvironment)
        {
            var optionsBuilder1 = new DbContextOptionsBuilder<HangfireContext>();
            optionsBuilder1.UseSqlServer(AesManager.GetConnectionString(configuration.GetConnectionString("hangfire_db"), configuration.GetSection("SecretKeyDB").Value));
            var context1 = new HangfireContext(optionsBuilder1.Options);
            context1.Database.Migrate();

            Configuration = configuration;
            StaticConfig = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApiVersioning(options => { options.AssumeDefaultVersionWhenUnspecified = true; });
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo()
                {
                    Version = "v1",
                    Title = "COBRA API",
                    Description = "Web API for COBRA",
                    TermsOfService = new Uri("http://cobra.novit.com.ar"),
                    Contact = new OpenApiContact() { Name = "Talking Dotnet", Email = "contact@talkingdotnet.com", Url = new Uri("http://www.talkingdotnet.com") }
                });
                c.AddSecurityDefinition("Token", new OpenApiSecurityScheme()
                {
                    Description = "JWT Authorization header. Example: \"Authorization: {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme{
                            Reference = new OpenApiReference{
                                Id = "Token", //The name of the previously defined security scheme.
                                Type = ReferenceType.SecurityScheme
                            }
                        },new List<string>()
                    }
                });
            });
            string[] allowedOrigins = Configuration.GetSection("Cors").GetSection("AllowedOrigins").Get<string[]>();
            services.AddCors(options =>
            {
                options.AddPolicy(CrossOriginResourceSharingPolicy,
                    builder => builder.WithOrigins(allowedOrigins).AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                    );
            });

            #region DbContext Config
            // Add Cobra DB connection
            string connectionString = AesManager.GetConnectionString(Configuration.GetConnectionString("mssql_db"), Configuration.GetSection("SecretKeyDB").Value);
            services.AddDbContext<RelationalDbContext>(options =>
            {
                options.UseSqlServer(connectionString, x => x.EnableRetryOnFailure())
                    .EnableSensitiveDataLogging();
            }, ServiceLifetime.Scoped); //DBContext Singleton porque sino el bot cuando llega un mail le dice que ya fue disposeado.

            // InMemoryDbConnection
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            services.AddDbContext<InMemoryDbContext>(options =>
                options.UseSqlite(connection), ServiceLifetime.Transient, ServiceLifetime.Transient);

            // Hangfire DB Connection
            string connectionStringHangfire = AesManager.GetConnectionString(Configuration.GetConnectionString("hangfire_db"), Configuration.GetSection("SecretKeyDB").Value);
            services.AddDbContext<HangfireContext>(options => options.UseSqlServer(connectionStringHangfire));
            #endregion DbContext Config

            services.AddCobraConfigurations(Configuration);
            services.AddCobraRepositories();
            services.AddCobraServices(Configuration);

            services.AddHangfire(configuration => configuration
                //.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(connectionStringHangfire, new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    UsePageLocksOnDequeue = true,
                    DisableGlobalLocks = true
                })
            );

            services.AddHangfireServer(
                options =>
                {
                    options.ServerName = "Cobra Hangfire";
                    options.WorkerCount = Math.Min(Environment.ProcessorCount * 5, MaxDefaultWorkerCount);
                    options.Queues = new[] {"cache", "mssql", "sqlite", "files", "rejectionfiles", "default" };
                }
            );

            services.AddControllers()
                .AddNewtonsoftJson(o =>
                {
                    o.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    o.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                })
                .ConfigureApiBehaviorOptions(o =>
                {
                    o.SuppressModelStateInvalidFilter = true;
                });

            //if (_hostEnvironment.IsDevelopment())
            //{
            //    services.AddDistributedMemoryCache();
            //}
            //else
            //{
            var slidingExpirationInMinutes = double.Parse(Configuration.GetSection("CacheSlidingExpirationTimeInMinutes").Value);
            var expiredDeletionInterval = double.Parse(Configuration.GetSection("CacheExpiredDeletionTimeInMinutes").Value);
            services.AddDistributedSqlServerCache(options =>
                {
                    options.ConnectionString =
                        connectionString;
                    options.SchemaName = "dbo";
                    options.TableName = "CobraCache";
                    options.DefaultSlidingExpiration = TimeSpan.FromMinutes(slidingExpirationInMinutes);
                    options.ExpiredItemsDeletionInterval = TimeSpan.FromMinutes(expiredDeletionInterval);
                });
            //}
            services.AddHealthChecks();
            services.AddSignalR(o =>
            {
                o.EnableDetailedErrors = true;
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, RelationalDbContext relationalContext, InMemoryDbContext inMemoryContext, IServiceProvider serviceProvider, IHostApplicationLifetime lifetime, IDistributedCache cache)
        {
            var logger = serviceProvider.GetService<Serilog.ILogger>();
            var check = serviceProvider.GetService<Monitoreo.ICheck>();
            Monitoreo.ConfiguradorMonitor configurador = new Monitoreo.ConfiguradorMonitor();
            Monitoreo.Monitor.Instancia = configurador.ConfigurarLogger(logger)
                .ConfigurarCheck(check)
                .CrearMonitor(Configuration.GetSection("Monitoreo:NombreSistema").Value);

            lifetime.ApplicationStarted.Register(() =>
            {
                var currentTimeUTC = DateTime.UtcNow.ToString();
                byte[] encodedCurrentTimeUTC = Encoding.UTF8.GetBytes(currentTimeUTC);
                var options = new DistributedCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(20));
                cache.Set("cachedTimeUTC", encodedCurrentTimeUTC, options);
            });
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            // COMPRUEBA QUE LA BD ESTÁ CREADA Y SI NO LA CREA.
            inMemoryContext.Database.EnsureCreated();

            relationalContext.Database.Migrate();

            // Add Initial Data to RelationalDbContext
            StartupConfigHelper.AddInitialDataRelationalContext(relationalContext, Configuration);

            // app.UseHttpContextLogger();

            // app.ConfigureExceptionHandler();

            app.UseHttpRequestMiddleware();

            //app.UseHttpsRedirection();
            if (env.EnvironmentName.Equals("QA"))
            {
                app.UseHangfireDashboard("/hangfire", new DashboardOptions()
                {
                    DashboardTitle = "Cobra Hangfire Dashboard",
                    Authorization = Enumerable.Repeat(new HangfireAuthorizationFilter(), 1)
                });
            }
            else
            {
                app.UseHangfireDashboard("/hangfire", new DashboardOptions()
                {
                    DashboardTitle = "Cobra Hangfire Dashboard",
                });
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cobra API V1");
            });
            app.UseRouting();
            app.UseCors(CrossOriginResourceSharingPolicy);
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => {
                endpoints.MapHealthChecks("/healthchecks", new HealthCheckOptions
                {
                    ResponseWriter = WriteHealthCheckResponse
                });
                endpoints.MapControllers();
                endpoints.MapHub<FinanceQuotationsHub>("/hub/quotation");
            });

            //When IncludeRequestBody is set to true(or when using IncludeRequestBodyFor/ ExcludeRequestBodyFor), you must enable rewind on the request body stream, otherwise the controller won't be able to read the request body since by default, it's a forwand - only stream that can be read only once.You can enable rewind on your startup logic with the following code:
            app.Use(async (context, next) =>
            {
                context.Request.EnableBuffering();
                await next();
            });

            app.UseStaticFiles(); // For the wwwroot folder

            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot","images")),
                RequestPath = new PathString("/Images")
            });

            if (Configuration.GetSection("EnableQuotationBot").Get<bool>())
            {
                Task.Run(async () =>
                {
                    IQuotationBotService quotationBotService = serviceProvider.CreateScope().ServiceProvider.GetService<IQuotationBotService>();
                    while (true)
                    {
                        await quotationBotService.ListenAllChannelsAsync();
                    }
                });
            }

            StartupConfigHelper.ConfigureHangfireRecurringJobs(serviceProvider, relationalContext, Configuration);
        }

        private static Task WriteHealthCheckResponse(HttpContext context, HealthReport healthReport)
        {
            context.Response.ContentType = "application/json; charset=utf-8";

            var options = new JsonWriterOptions { Indented = true };

            using var memoryStream = new MemoryStream();
            using (var jsonWriter = new Utf8JsonWriter(memoryStream, options))
            {
                jsonWriter.WriteStartObject();
                jsonWriter.WriteString("status", healthReport.Status == HealthStatus.Healthy ? "ok" : "nok");
                jsonWriter.WriteString("message", healthReport.Status.ToString());
                jsonWriter.WriteString("timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
                jsonWriter.WriteEndObject();
            }

            return context.Response.WriteAsync(
                Encoding.UTF8.GetString(memoryStream.ToArray()));
        }
    }
}
