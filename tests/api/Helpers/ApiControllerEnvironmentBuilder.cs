using System;
using System.Net.Http;
using JCCommon.Framework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Scv.Api.Helpers.Exceptions;

namespace tests.api.Helpers
{
    /// <summary>
    /// This builds out our environment before injecting into controllers. 
    /// </summary>
    public class ApiControllerEnvironmentBuilder
    {
        public ILoggerFactory LogFactory;
        public HttpClient HttpClient;
        public IConfiguration Configuration;

        public ApiControllerEnvironmentBuilder(string usernameKey, string passwordKey, Type type)
        {
            //Load in secrets, this uses the secrets file as the API. 
            var builder = new ConfigurationBuilder();
            builder.AddUserSecrets<FilesControllerTests>();
            Configuration = builder.Build();

            //Create HTTP client, usually done by Startup.cs - which handles the life cycle of HttpClient nicely. 
            HttpClient = new HttpClient();
            HttpClient.DefaultRequestHeaders.Authorization = new BasicAuthenticationHeaderValue(
                Configuration.GetValue<string>(usernameKey) ?? throw new ConfigurationException($"{usernameKey} was not found in secrets."),
                Configuration.GetValue<string>(passwordKey) ?? throw new ConfigurationException($"{passwordKey} was not found in secrets."));

            //Create logger.
            LogFactory = LoggerFactory.Create(loggingBuilder =>
            {
                loggingBuilder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("LoggingConsoleApp.Program", LogLevel.Debug)
                    .AddConsole();
            });
        }
    }
}
