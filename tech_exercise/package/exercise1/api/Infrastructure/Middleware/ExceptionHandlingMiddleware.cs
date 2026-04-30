using Microsoft.AspNetCore.Mvc;
using StargateAPI.Controllers;
using StargateAPI.Infrastructure.Exceptions;
using StargateAPI.Infrastructure.Logging;
using System.Net;

namespace StargateAPI.Infrastructure.Middleware
{
    public class ExceptionHandlingMiddleware : IMiddleware
    {
        private readonly IProcessLogWriter _processLogWriter;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(IProcessLogWriter processLogWriter, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _processLogWriter = processLogWriter;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);

                var (level, message) = context.Response.StatusCode switch
                {
                    >= 500 => ("Error", "Request completed with server error."),
                    >= 400 => ("Warning", "Request completed with client error."),
                    _ => ("Information", "Request succeeded.")
                };

                await _processLogWriter.LogAsync(
                    level,
                    message,
                    context.Request.Path,
                    context.Request.Method,
                    context.Response.StatusCode,
                    context.RequestAborted);
            }
            catch (Exception exception)
            {
                var statusCode = exception is ClientInputException
                    ? (int)HttpStatusCode.BadRequest
                    : (int)HttpStatusCode.InternalServerError;

                var response = new BaseResponse
                {
                    Success = false,
                    Message = exception is ClientInputException ? exception.Message : "An unexpected error occurred.",
                    ResponseCode = statusCode
                };

                _logger.LogError(exception, "Request failed.");
                await _processLogWriter.LogAsync(
                    "Error",
                    exception.Message,
                    context.Request.Path,
                    context.Request.Method,
                    statusCode,
                    context.RequestAborted);

                context.Response.StatusCode = statusCode;
                await context.Response.WriteAsJsonAsync(response);
            }
        }
    }
}
