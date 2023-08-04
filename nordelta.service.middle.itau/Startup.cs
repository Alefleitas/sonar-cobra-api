namespace nordelta.service.middle.itau
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using nordelta.service.middle.itau.Services.Interfaces;
    using nordelta.service.middle.itau.Services;
    using Newtonsoft.Json;
    using System.Text.Json.Serialization;

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApiVersioning(options => { options.AssumeDefaultVersionWhenUnspecified = true; });
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddSingleton<HttpClient>();
            services.AddSingleton<IProcessNotificationService, ProcessNotificationService>();

            services.AddControllers()
               .AddNewtonsoftJson(o =>
               {
                   
                   o.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                   
               })
               .AddJsonOptions(o =>
               {
                   o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
               })
               .ConfigureApiBehaviorOptions(o =>
               {
                   o.SuppressModelStateInvalidFilter = true;
               });

           
         
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
           
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }

}
