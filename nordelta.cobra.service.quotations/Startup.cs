using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.OpenApi.Models;
using Quartz.Impl;
using Quartz.Spi;

namespace nordelta.cobra.service.quotations
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApiVersioning(options => { options.AssumeDefaultVersionWhenUnspecified = true; });
            services.AddMvcCore()
                    .AddApiExplorer();

            services.AddCors(opt =>
            {
                opt.AddPolicy("CorsRule", rule =>
                {
                    rule.AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowAnyOrigin();
                });
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Quotation Service API",
                    Description = "Quotation API"
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
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {


            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseHsts();

            app.UseCors("CorsRule");
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Quotation Service API v1");
            });
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Quotations Api is running..");
                });
                endpoints.MapSwagger();
                endpoints.MapControllers();
            });
        }
    }
}
