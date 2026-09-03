using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Photobiz.Api.ExceptionHandling;
using Photobiz.Application.Common.Exceptions;

namespace Photobiz.Api.Tests.ExceptionHandling
{
    public class GlobalExceptionHandlerTests
    {
        private static (GlobalExceptionHandler Handler, IProblemDetailsService ProblemDetailsService) CreateHandler(
            string environmentName)
        {
            var environment = Substitute.For<IHostEnvironment>();
            environment.EnvironmentName.Returns(environmentName);
            var problemDetailsService = Substitute.For<IProblemDetailsService>();
            problemDetailsService
                .TryWriteAsync(Arg.Any<ProblemDetailsContext>())
                .Returns(true);

            var handler = new GlobalExceptionHandler(
                NullLogger<GlobalExceptionHandler>.Instance,
                environment,
                problemDetailsService);

            return (handler, problemDetailsService);
        }

        [Fact]
        public async Task TryHandleAsync_WithGenericException_Returns500ProblemDetails()
        {
            var (handler, problemDetailsService) = CreateHandler(Environments.Production);
            var httpContext = new DefaultHttpContext();
            ProblemDetailsContext? captured = null;
            problemDetailsService
                .TryWriteAsync(Arg.Do<ProblemDetailsContext>(ctx => captured = ctx))
                .Returns(true);

            var handled = await handler.TryHandleAsync(httpContext, new InvalidOperationException("boom"), CancellationToken.None);

            Assert.True(handled);
            Assert.Equal(StatusCodes.Status500InternalServerError, httpContext.Response.StatusCode);
            Assert.Equal(StatusCodes.Status500InternalServerError, captured!.ProblemDetails.Status);
        }

        [Fact]
        public async Task TryHandleAsync_InProduction_DoesNotExposeExceptionDetail()
        {
            var (handler, problemDetailsService) = CreateHandler(Environments.Production);
            var httpContext = new DefaultHttpContext();
            ProblemDetailsContext? captured = null;
            problemDetailsService
                .TryWriteAsync(Arg.Do<ProblemDetailsContext>(ctx => captured = ctx))
                .Returns(true);

            await handler.TryHandleAsync(httpContext, new InvalidOperationException("sensitive details"), CancellationToken.None);

            Assert.Null(captured!.ProblemDetails.Detail);
        }

        [Fact]
        public async Task TryHandleAsync_InDevelopment_ExposesExceptionDetail()
        {
            var (handler, problemDetailsService) = CreateHandler(Environments.Development);
            var httpContext = new DefaultHttpContext();
            ProblemDetailsContext? captured = null;
            problemDetailsService
                .TryWriteAsync(Arg.Do<ProblemDetailsContext>(ctx => captured = ctx))
                .Returns(true);

            await handler.TryHandleAsync(httpContext, new InvalidOperationException("visible in dev"), CancellationToken.None);

            Assert.Contains("visible in dev", captured!.ProblemDetails.Detail);
        }

        [Fact]
        public async Task TryHandleAsync_WithValidationException_Returns400WithFieldErrors()
        {
            var (handler, problemDetailsService) = CreateHandler(Environments.Production);
            var httpContext = new DefaultHttpContext();
            ProblemDetailsContext? captured = null;
            problemDetailsService
                .TryWriteAsync(Arg.Do<ProblemDetailsContext>(ctx => captured = ctx))
                .Returns(true);

            var validationException = new ValidationException(
                [new ValidationFailure("Username", "'Username' must not be empty.")]);

            var handled = await handler.TryHandleAsync(httpContext, validationException, CancellationToken.None);

            Assert.True(handled);
            Assert.Equal(StatusCodes.Status400BadRequest, httpContext.Response.StatusCode);
            var validationProblemDetails = Assert.IsType<ValidationProblemDetails>(captured!.ProblemDetails);
            Assert.Equal(["'Username' must not be empty."], validationProblemDetails.Errors["Username"]);
        }

        [Fact]
        public async Task TryHandleAsync_WithAuthenticationFailedException_Returns401()
        {
            var (handler, problemDetailsService) = CreateHandler(Environments.Production);
            var httpContext = new DefaultHttpContext();
            ProblemDetailsContext? captured = null;
            problemDetailsService
                .TryWriteAsync(Arg.Do<ProblemDetailsContext>(ctx => captured = ctx))
                .Returns(true);

            var handled = await handler.TryHandleAsync(httpContext, new AuthenticationFailedException(), CancellationToken.None);

            Assert.True(handled);
            Assert.Equal(StatusCodes.Status401Unauthorized, httpContext.Response.StatusCode);
            Assert.Equal(StatusCodes.Status401Unauthorized, captured!.ProblemDetails.Status);
        }
    }
}
