using Photobiz.Application.Common.Models;

namespace Photobiz.Application.Tests.Common.Models
{
    public class ResultDtoTests
    {
        [Fact]
        public void Succeeded_WithoutPayload_SetsSuccessAndMessage()
        {
            var result = ResultDto.Succeeded("done");

            Assert.True(result.Success);
            Assert.Equal("done", result.Message);
        }

        [Fact]
        public void Failed_WithoutPayload_SetsFailureAndMessage()
        {
            var result = ResultDto.Failed("nope");

            Assert.False(result.Success);
            Assert.Equal("nope", result.Message);
        }

        [Fact]
        public void Succeeded_WithPayload_CarriesData()
        {
            var result = ResultDto<int>.Succeeded(42, "answer");

            Assert.True(result.Success);
            Assert.Equal(42, result.Data);
            Assert.Equal("answer", result.Message);
        }

        [Fact]
        public void Failed_WithPayload_HasDefaultData()
        {
            var result = ResultDto<string>.Failed("bad");

            Assert.False(result.Success);
            Assert.Null(result.Data);
            Assert.Equal("bad", result.Message);
        }
    }
}
