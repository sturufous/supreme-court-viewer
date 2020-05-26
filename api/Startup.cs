using JCCommon.Clients.FileServices;
using JCCommon.Clients.LocationServices;
using JCCommon.Clients.LookupServices;
using JCCommon.Framework;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Scv.Api.Helpers;
using Scv.Api.Helpers.ContractResolver;
using Scv.Api.Helpers.Mapping;
using Scv.Api.Helpers.Middleware;
using Scv.Api.Services;
using System;
using System.IO;
using System.Reflection;

namespace Scv.Api
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
            services.AddMapster();

            string corsDomain = Configuration.GetValue<string>("CORS_DOMAIN");
            Console.WriteLine($"CORS_DOMAIN: {corsDomain}");

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.WithOrigins(corsDomain);
                });
            });

            services.AddHttpClient<FileServicesClient>(client =>
            {
                client.DefaultRequestHeaders.Authorization = new BasicAuthenticationHeaderValue(
                    Configuration.GetNonEmptyValue("FileServicesClient:Username"),
                    Configuration.GetNonEmptyValue("FileServicesClient:Password"));
            });

            services.AddHttpClient<LookupServiceClient>(client =>
            {
                client.DefaultRequestHeaders.Authorization = new BasicAuthenticationHeaderValue(
                    Configuration.GetNonEmptyValue("LookupServicesClient:Username"),
                    Configuration.GetNonEmptyValue("LookupServicesClient:Password"));
            });

            services.AddHttpClient<LocationServicesClient>(client =>
            {
                client.DefaultRequestHeaders.Authorization = new BasicAuthenticationHeaderValue(
                    Configuration.GetNonEmptyValue("LocationServicesClient:Username"),
                    Configuration.GetNonEmptyValue("LocationServicesClient:Password"));
            });

            services.AddScoped<FilesService>();
            services.AddScoped<LookupService>();
            services.AddScoped<LocationService>();

            services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new SafeContractResolver { NamingStrategy = new CamelCaseNamingStrategy() };
                options.SerializerSettings.Formatting = Formatting.Indented;
                options.SerializerSettings.Converters.Add(new StringEnumConverter());
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            });

            services.AddSwaggerGen(options =>
            {
                options.EnableAnnotations(true);
                options.CustomSchemaIds(o => o.FullName);
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            });
            services.AddLazyCache();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors();

            app.UseSwagger(options =>
            {
                options.RouteTemplate = "api/swagger/{documentname}/swagger.json";
            });

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("swagger/v1/swagger.json", "SCV.API");
                options.RoutePrefix = "api";
            });

            app.UseMiddleware(typeof(ErrorHandlingMiddleware));

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}