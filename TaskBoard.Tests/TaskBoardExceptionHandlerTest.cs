using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

[TestFixture]
public class ExceptionHandlingMiddlewareTests
{
    [Test]
    public async Task Invoke_WhenExceptionThrown_Returns500WithErrorMessage()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var responseStream = new MemoryStream();
        context.Response.Body = responseStream;

        var loggerMock = new Mock<ILogger<ExceptionHandlingMiddleware>>();

        // This delegate simulates the "next" middleware throwing an exception
        RequestDelegate next = (ctx) => throw new InvalidOperationException("Test exception");

        var middleware = new ExceptionHandlingMiddleware(next, loggerMock.Object);

        // Act
        await middleware.Invoke(context);

        // Assert
        Assert.That(context.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.InternalServerError));
        Assert.That(context.Response.ContentType, Is.EqualTo("application/json; charset=utf-8"));

        responseStream.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(responseStream).ReadToEndAsync();

        var json = JsonSerializer.Deserialize<JsonElement>(responseBody);

        Assert.That(json.GetProperty("success").GetBoolean(), Is.False);
        Assert.That(json.GetProperty("message").GetString(), Is.EqualTo("Something went wrong. Please try again later."));
        Assert.That(json.GetProperty("detail").GetString(), Is.EqualTo("Test exception"));
    }
}
