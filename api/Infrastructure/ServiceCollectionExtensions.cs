using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Scv.Api.Helpers;
using Scv.Api.Helpers.Extensions;
using Scv.Api.Infrastructure.Authentication;
using Scv.Api.Infrastructure.Authorization;
using Scv.Db.Models;
using Mapster;
using MapsterMapper;
using System.Reflection;
using JCCommon.Clients.FileServices;
using JCCommon.Clients.LocationServices;
using JCCommon.Clients.LookupCodeServices;
using JCCommon.Clients.UserService;
using Microsoft.Extensions.Hosting;
using Scv.Api.Services;
using Scv.Api.Services.Files;
using BasicAuthenticationHeaderValue = JCCommon.Framework.BasicAuthenticationHeaderValue;

namespace Scv.Api.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMapster(this IServiceCollection services, Action<TypeAdapterConfig> options = null)
        {
            var config = TypeAdapterConfig.GlobalSettings;
            config.Scan(Assembly.GetAssembly(typeof(Startup)) ?? throw new InvalidOperationException());

            options?.Invoke(config);

            services.AddSingleton(config);
            services.AddScoped<IMapper, ServiceMapper>();

            return services;
        }

        public static IServiceCollection AddHttpClientsAndScvServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient<FileServicesClient>(client =>
            {
                client.DefaultRequestHeaders.Authorization = new BasicAuthenticationHeaderValue(
                    configuration.GetNonEmptyValue("FileServicesClient:Username"),
                    configuration.GetNonEmptyValue("FileServicesClient:Password"));
                client.BaseAddress = new Uri(configuration.GetNonEmptyValue("FileServicesClient:Url").EnsureEndingForwardSlash());
            });

            services.AddHttpClient<LookupCodeServicesClient>(client =>
            {
                client.DefaultRequestHeaders.Authorization = new BasicAuthenticationHeaderValue(
                    configuration.GetNonEmptyValue("LookupServicesClient:Username"),
                    configuration.GetNonEmptyValue("LookupServicesClient:Password"));
                client.BaseAddress = new Uri(configuration.GetNonEmptyValue("LookupServicesClient:Url").EnsureEndingForwardSlash());
            });

            services.AddHttpClient<LocationServicesClient>(client =>
            {
                client.DefaultRequestHeaders.Authorization = new BasicAuthenticationHeaderValue(
                    configuration.GetNonEmptyValue("LocationServicesClient:Username"),
                    configuration.GetNonEmptyValue("LocationServicesClient:Password"));
                client.BaseAddress = new Uri(configuration.GetNonEmptyValue("LocationServicesClient:Url").EnsureEndingForwardSlash());
            });

            services.AddHttpClient<UserServiceClient>(client =>
            {
                client.DefaultRequestHeaders.Authorization = new BasicAuthenticationHeaderValue(
                    configuration.GetNonEmptyValue("UserServicesClient:Username"),
                    configuration.GetNonEmptyValue("UserServicesClient:Password"));
                client.BaseAddress = new Uri(configuration.GetNonEmptyValue("UserServicesClient:Url")
                    .EnsureEndingForwardSlash());
            });
            services.AddHttpContextAccessor();
            services.AddTransient(s => s.GetService<IHttpContextAccessor>().HttpContext.User);
            services.AddScoped<FilesService>();
            services.AddScoped<LookupService>();
            services.AddScoped<LocationService>();
            services.AddScoped<CourtListService>();
            services.AddScoped<VcCivilFileAccessHandler>();
            services.AddSingleton<JCUserService>();

            return services;
        }
    }
}
