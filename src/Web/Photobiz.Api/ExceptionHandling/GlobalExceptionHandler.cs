using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Photobiz.Application.Common.Exceptions;

namespace Photobiz.Api.ExceptionHandling
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IHostEnvironment _environment;
        private readonly IProblemDetailsService _problemDetailsService;

        public GlobalExceptionHandler(
            ILogger<GlobalExceptionHandler> logger,
            IHostEnvironment environment,
            IProblemDetailsService problemDetailsService)
        {
            _logger = logger;
            _environment = environment;
            _problemDetailsService = problemDetailsService;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is ValidationException validationException)
            {
                return await HandleValidationExceptionAsync(httpContext, validationException);
            }

            if (exception is AuthenticationFailedException authenticationFailedException)
            {
                return await HandleAuthenticationFailedExceptionAsync(httpContext, authenticationFailedException);
            }

            if (exception is NotFoundException notFoundException)
            {
                return await HandleNotFoundExceptionAsync(httpContext, notFoundException);
            }

            if (exception is ConflictException conflictException)
            {
                return await HandleConflictExceptionAsync(httpContext, conflictException);
            }

            if (exception is TenantNotFoundException tenantNotFoundException)
            {
                return await HandleTenantNotFoundExceptionAsync(httpContext, tenantNotFoundException);
            }

            _logger.LogError(exception, "Unhandled exception processing {Method} {Path}",
                httpContext.Request.Method, httpContext.Request.Path);

            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred.",
                Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
                Instance = httpContext.Request.Path
            };

            if (_environment.IsDevelopment())
            {
                problemDetails.Detail = exception.ToString();
            }

            return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = problemDetails
            });
        }

        private async ValueTask<bool> HandleValidationExceptionAsync(
            HttpContext httpContext,
            ValidationException validationException)
        {
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

            var errors = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            var problemDetails = new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred.",
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                Instance = httpContext.Request.Path
            };

            return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = validationException,
                ProblemDetails = problemDetails
            });
        }

        private async ValueTask<bool> HandleAuthenticationFailedExceptionAsync(
            HttpContext httpContext,
            AuthenticationFailedException authenticationFailedException)
        {
            httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Authentication failed.",
                Detail = authenticationFailedException.Message,
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.2",
                Instance = httpContext.Request.Path
            };

            return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = authenticationFailedException,
                ProblemDetails = problemDetails
            });
        }

        private async ValueTask<bool> HandleNotFoundExceptionAsync(
            HttpContext httpContext,
            NotFoundException notFoundException)
        {
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Resource not found.",
                Detail = notFoundException.Message,
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                Instance = httpContext.Request.Path
            };

            return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = notFoundException,
                ProblemDetails = problemDetails
            });
        }

        private async ValueTask<bool> HandleConflictExceptionAsync(
            HttpContext httpContext,
            ConflictException conflictException)
        {
            httpContext.Response.StatusCode = StatusCodes.Status409Conflict;

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Request conflicts with the current state of the resource.",
                Detail = conflictException.Message,
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                Instance = httpContext.Request.Path
            };

            return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = conflictException,
                ProblemDetails = problemDetails
            });
        }

        private async ValueTask<bool> HandleTenantNotFoundExceptionAsync(
            HttpContext httpContext,
            TenantNotFoundException tenantNotFoundException)
        {
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Unknown tenant.",
                Detail = tenantNotFoundException.Message,
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                Instance = httpContext.Request.Path
            };

            return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = tenantNotFoundException,
                ProblemDetails = problemDetails
            });
        }
    }
}
