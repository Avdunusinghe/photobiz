using FluentValidation;
using FluentValidation.Results;
using MediatR;
using NSubstitute;
using Photobiz.Application.Common.Behaviors;

namespace Photobiz.Application.Tests.Common.Behaviors
{
    public class ValidationBehaviorTests
    {
        public record TestRequest(string Value) : IRequest<string>;

        [Fact]
        public async Task Handle_WithNoValidators_CallsNext()
        {
            var behavior = new ValidationBehavior<TestRequest, string>([]);

            var result = await behavior.Handle(
                new TestRequest("anything"),
                _ => Task.FromResult("handled"),
                CancellationToken.None);

            Assert.Equal("handled", result);
        }

        [Fact]
        public async Task Handle_WithFailingValidator_ThrowsValidationExceptionAndDoesNotCallNext()
        {
            var validator = Substitute.For<IValidator<TestRequest>>();
            validator
                .ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
                .Returns(new ValidationResult([new ValidationFailure(nameof(TestRequest.Value), "must not be empty")]));

            var behavior = new ValidationBehavior<TestRequest, string>([validator]);
            var nextCalled = false;

            await Assert.ThrowsAsync<ValidationException>(() =>
                behavior.Handle(
                    new TestRequest(""),
                    _ =>
                    {
                        nextCalled = true;
                        return Task.FromResult("handled");
                    },
                    CancellationToken.None));

            Assert.False(nextCalled);
        }

        [Fact]
        public async Task Handle_WithPassingValidator_CallsNext()
        {
            var validator = Substitute.For<IValidator<TestRequest>>();
            validator
                .ValidateAsync(Arg.Any<ValidationContext<TestRequest>>(), Arg.Any<CancellationToken>())
                .Returns(new ValidationResult());

            var behavior = new ValidationBehavior<TestRequest, string>([validator]);

            var result = await behavior.Handle(
                new TestRequest("valid"),
                _ => Task.FromResult("handled"),
                CancellationToken.None);

            Assert.Equal("handled", result);
        }
    }
}
