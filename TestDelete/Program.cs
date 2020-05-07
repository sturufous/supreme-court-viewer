using System;
using System.Net.Http;
using JCCommon.Clients.FileServices;
using JCCommon.Clients.LookupServices;
using JCCommon.Framework;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Scv.Api.Helpers.Exceptions;
using Scv.Api.Helpers.ContractResolver;
namespace TestDelete
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            //Load in secrets, this uses a separate secrets file for the tests project. 
            var builder = new ConfigurationBuilder();
            builder.AddUserSecrets<Program>();
            IConfiguration _configuration = builder.Build();

            //Create HTTP client, usually done by Startup.cs - which handles the life cycle of HttpClient nicely. 
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new BasicAuthenticationHeaderValue(
                _configuration.GetValue<string>("FileServicesClient:Username") ?? throw new ConfigurationException("FileServicesClient:Username was not found in secrets."),
                _configuration.GetValue<string>("FileServicesClient:Password") ?? throw new ConfigurationException("FileServicesClient:Password was not found in secrets."));

            var _fsClient = new LookupServiceClient(client);
            _fsClient.JsonSerializerSettings.ContractResolver = new SafeContractResolver();
            _fsClient.BaseUrl = _configuration.GetValue<string>("FileServicesClient:Url") ?? throw new ConfigurationException($"Configuration 'FileServicesClient:Url' is invalid or missing.");


            var codesDocument = JsonConvert.SerializeObject(_fsClient.CodesDocumentsAsync().Result);

            for (int i = 0;
                i < 5000;
                i++)
            {
                var codesDocument2 = _fsClient.CodesDocumentsDocumentIdAsync(i.ToString()).Result;

                if (codesDocument2 != null)
                    codesDocument2 = codesDocument2;
            }

        }
    }
}
