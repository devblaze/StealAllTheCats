using StealAllTheCats.Dtos;
using Xunit;

namespace StealAllTheCats.Tests.Dtos;

public class ResultTests
{
    [Fact]
    public void Ok_SetsSuccessTrue()
    {
        var result = Result<string>.Ok("hello");

        Assert.True(result.Success);
        Assert.Equal("hello", result.Data);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void Fail_SetsSuccessFalse()
    {
        var result = Result<string>.Fail("bad request", 400);

        Assert.False(result.Success);
        Assert.Equal("bad request", result.ErrorMessage);
        Assert.Equal(400, result.ErrorCode);
        Assert.Null(result.Data);
    }

    [Fact]
    public void Fail_WithException_IncludesException()
    {
        var ex = new InvalidOperationException("oops");
        var result = Result<int>.Fail("error", 500, ex);

        Assert.False(result.Success);
        Assert.Equal(ex, result.Exception);
    }

    [Fact]
    public void Fail_DefaultErrorCodeIs500()
    {
        var result = Result<int>.Fail("server error");

        Assert.Equal(500, result.ErrorCode);
    }
}
