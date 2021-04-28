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

        public static IServiceCollection AddAuthorizationAndAuthentication(this IServiceCollection services,
            IWebHostEnvironment env, IConfiguration configuration)
        {
            var baseUrl = configuration.GetNonEmptyValue("WebBaseHref");
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.Cookie.Name = "SCV";
                if (env.IsDevelopment())
                    options.Cookie.Name += ".Development";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToAccessDenied = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return context.Response.CompleteAsync();
                    },
                    OnValidatePrincipal = async cookieCtx =>
                    {
                        if (cookieCtx.Principal.Identity.AuthenticationType ==
                            SiteMinderAuthenticationHandler.SiteMinder)
                            return;

                        var accessTokenExpiration = DateTimeOffset.Parse(cookieCtx.Properties.GetTokenValue("expires_at"));
                        var timeRemaining = accessTokenExpiration.Subtract(DateTimeOffset.UtcNow);
                        var refreshThreshold = TimeSpan.Parse(configuration.GetNonEmptyValue("TokenRefreshThreshold"));

                        if (timeRemaining > refreshThreshold)
                            return;

                        var refreshToken = cookieCtx.Properties.GetTokenValue("refresh_token");
                        var httpClientFactory = cookieCtx.HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>();
                        var httpClient = httpClientFactory.CreateClient(nameof(CookieAuthenticationEvents));
                        var response = await httpClient.RequestRefreshTokenAsync(new RefreshTokenRequest
                        {
                            Address = configuration.GetNonEmptyValue("Keycloak:Authority") + "/protocol/openid-connect/token",
                            ClientId = configuration.GetNonEmptyValue("Keycloak:Client"),
                            ClientSecret = configuration.GetNonEmptyValue("Keycloak:Secret"),
                            RefreshToken = refreshToken
                        });

                        if (response.IsError)
                        {
                            cookieCtx.RejectPrincipal();
                            await cookieCtx.HttpContext.SignOutAsync(CookieAuthenticationDefaults
                                .AuthenticationScheme);
                        }
                        else
                        {
                            var expiresInSeconds = response.ExpiresIn;
                            var updatedExpiresAt = DateTimeOffset.UtcNow.AddSeconds(expiresInSeconds);
                            cookieCtx.Properties.UpdateTokenValue("expires_at", updatedExpiresAt.ToString());
                            cookieCtx.Properties.UpdateTokenValue("refresh_token", response.RefreshToken);

                            // Indicate to the cookie middleware that the cookie should be remade (since we have updated it)
                            cookieCtx.ShouldRenew = true;
                        }
                    }
                };
            }
            )
            .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.Authority = configuration.GetNonEmptyValue("Keycloak:Authority");
                options.ClientId = configuration.GetNonEmptyValue("Keycloak:Client");
                options.ClientSecret = configuration.GetNonEmptyValue("Keycloak:Secret");
                options.RequireHttpsMetadata = true;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.ResponseType = OpenIdConnectResponseType.Code;
                options.UsePkce = true;
                options.SaveTokens = true;
                options.CallbackPath = "/api/auth/signin-oidc";
                options.Scope.Add("groups");
                options.Scope.Add("vc_authn");
                options.Events = new OpenIdConnectEvents
                {
                    OnTicketReceived = context =>
                    {
                        context.Properties.Items.Remove(".Token.id_token");
                        context.Properties.Items.Remove(".Token.access_token");
                        context.Properties.Items[".TokenNames"] = "refresh_token;token_type;expires_at";
                        return Task.CompletedTask;
                    },
#pragma warning disable 1998
                    OnTokenValidated = async context =>
#pragma warning restore 1998
                    {
                        if (!(context.Principal.Identity is ClaimsIdentity identity)) return;

                        //Cleanup keycloak claims, that are unused.
                        foreach (var claim in identity.Claims.WhereToList(c =>
                            !CustomClaimTypes.UsedKeycloakClaimTypes.Contains(c.Type)))
                            identity.RemoveClaim(claim);

                        //Add default claims for PartId, AgencyIdentifier Id
                        var claims = new List<Claim>();
                        claims.AddRange(new[] {
                            new Claim(CustomClaimTypes.JcParticipantId, configuration.GetNonEmptyValue("Request:PartId")),
                            new Claim(CustomClaimTypes.JcAgencyCode, configuration.GetNonEmptyValue("Request:AgencyIdentifierId")),
                        });

                        identity.AddClaims(claims);
                    },
                    OnRedirectToIdentityProvider = context =>
                    {
                        if (context.ProtocolMessage.RequestType == OpenIdConnectRequestType.Authentication)
                        {
                            if (!context.Request.Path.StartsWithSegments("/api/auth/login"))
                            {
                                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                                context.HandleResponse();
                                return Task.CompletedTask;
                            }
                        }

                        context.ProtocolMessage.SetParameter("kc_idp_hint",
                            context.Request.Query["redirectUri"].ToString().Contains("fromA2A=true")
                                ? configuration.GetNonEmptyValue("Keycloak:VcIdpHint")
                                : "idir");

                        context.ProtocolMessage.SetParameter("pres_req_conf_id", configuration.GetNonEmptyValue("Keycloak:PresReqConfId"));
                        if (context.HttpContext.Request.Headers["X-Forwarded-Host"].Count > 0)
                        {
                            var forwardedHost = context.HttpContext.Request.Headers["X-Forwarded-Host"];
                            var forwardedPort = context.HttpContext.Request.Headers["X-Forwarded-Port"];
                            context.ProtocolMessage.RedirectUri =
                                $"{XForwardedForHelper.BuildUrlString(forwardedHost, forwardedPort, baseUrl)}{options.CallbackPath}";
                        }
                        return Task.CompletedTask;
                    }
                };
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                var key = Encoding.ASCII.GetBytes(configuration.GetNonEmptyValue("Keycloak:Secret"));
                options.Authority = configuration.GetNonEmptyValue("Keycloak:Authority");
                options.Audience = configuration.GetNonEmptyValue("Keycloak:Audience");
                options.RequireHttpsMetadata = true;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.FromSeconds(5)
                };
                if (key.Length > 0) options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(key);
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        context.NoResult();
                        context.Response.StatusCode = 401;
                        return Task.CompletedTask;
                    },
                    OnForbidden = context =>
                    {
                        context.NoResult();
                        context.Response.StatusCode = 403;
                        return Task.CompletedTask;
                    }
                };
            })
            .AddScheme<AuthenticationSchemeOptions, SiteMinderAuthenticationHandler>(
                SiteMinderAuthenticationHandler.SiteMinder, null);

            services.AddAuthorization(options =>
            {
                options.AddPolicy(nameof(ProviderAuthorizationHandler), policy => policy.Requirements.Add(new ProviderAuthorizationHandler()));
            });
            return services;
        }
    }
}
