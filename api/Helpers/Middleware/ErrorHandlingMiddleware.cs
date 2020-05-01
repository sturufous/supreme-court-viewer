using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Scv.Api.Helpers.Exceptions;

namespace Scv.Api.Helpers.Middleware 
{
    /// <summary>
    /// ErrorHandlingMiddleware class, provides a way to catch and handle unhandled errors in a generic way.
    /// </summary>
    public class ErrorHandlingMiddleware
    {
        #region Variables
        private readonly RequestDelegate _next;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly JsonOptions _options;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new instance of an ErrorHandlingMiddleware class, and initializes it with the specified arguments.
        /// </summary>
        /// <param name="next"></param>
        /// <param name="env"></param>
        /// <param name="logger"></param>
        /// <param name="options"></param>
        public ErrorHandlingMiddleware(RequestDelegate next, IWebHostEnvironment env, ILogger<ErrorHandlingMiddleware> logger, IOptions<JsonOptions> options)
        {
            _next = next;
            _env = env;
            _logger = logger;
            _options = options.Value;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Handle the exception if one occurs.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// Handle the exception by returning an appropriate error message depending on type and environment.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var code = HttpStatusCode.InternalServerError;
            var message = "An unhandled error has occurred.";
            string details = null;

            switch (ex)
            {
                case SecurityTokenException _:
                    code = HttpStatusCode.Unauthorized;
                    message = "The authentication token is invalid.";
                    break;
                case KeyNotFoundException _:
                    code = HttpStatusCode.BadRequest;
                    message = "Item does not exist.";

                    _logger.LogDebug(ex, "Middleware caught unhandled exception.");
                    break;
                case NotAuthorizedException _:
                    code = HttpStatusCode.Forbidden;
                    message = "User is not authorized to perform this action.";

                    _logger.LogWarning(ex, ex.Message);
                    break;
                case ConfigurationException _:
                    code = HttpStatusCode.InternalServerError;
                    message = "Application configuration details invalid or missing.";

                    _logger.LogError(ex, ex.Message);
                    break;
                case BadRequestException _:
                case InvalidOperationException _:
                    code = HttpStatusCode.BadRequest;
                    message = ex.Message;
                    break;
                case ApiHttpRequestException exception:
                    code = exception.StatusCode;
                    message = exception.Message;

                    try
                    {
                        await using var responseStream = await exception?.Response.Content.ReadAsStreamAsync();
                        responseStream.Position = 0;
                        using var readStream = new StreamReader(responseStream, Encoding.UTF8);
                        details = await readStream.ReadToEndAsync(); // TODO: Rewrite this logic.
                        _logger.LogError(exception, details);
                    }
                    catch (Exception streamEx)
                    {
                        // Ignore for now.
                        _logger.LogError(streamEx, $"Failed to read the {nameof(ApiHttpRequestException)} error stream.");
                    }

                    break;
                default:
                    _logger.LogError(ex, "Middleware caught unhandled exception.");
                    break;
            }

            if (!context.Response.HasStarted)
            {
                var isDev = _env.IsDevelopment();
                var result = JsonSerializer.Serialize(new Models.ErrorResponseModel(_env, ex, message, details), _options.JsonSerializerOptions);
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)code;
                await context.Response.WriteAsync(result);
            }
            else
            {
                // Had to do this because odd errors were occurring when bearer tokens were failing.
                await context.Response.WriteAsync(string.Empty);
            }
        }
        #endregion
    }
}