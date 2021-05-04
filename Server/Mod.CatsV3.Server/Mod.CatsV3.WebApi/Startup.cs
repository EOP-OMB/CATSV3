
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Mod.Framework.WebApi.Extensions;
using Mod.CatsV3.Email.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Mod.CatsV3.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddModAspNetCore();
            services.AddCATV3Manager();


            //Inject CATSEmailAPIConfig configuration appsetting.json
            services.Configure<CATSEmailAPIConfiguration>(Configuration.GetSection("CATSEmailAPIConfig"));
            var settings = Configuration.GetSection("CATSEmailAPIConfig").Get<CATSEmailAPIConfiguration>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseExceptionHandler("/error");

            app.UseModAspNetCore();

            app.UseCors("AllowAllHeaders");

            app.UseExceptionHandler(c => c.Run(async context =>
            {
                var exception = context.Features
                    .Get<IExceptionHandlerPathFeature>()
                    .Error;
                //var response = new { error = exception.Message };

                //await context.Response.WriteAsJsonAsync(response);

                //var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                //var exception = exceptionHandlerPathFeature.Error;
                var result = JsonConvert.SerializeObject(new { error = exception.Message });
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(result);

            }));

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
