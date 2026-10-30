using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace IoanCvApp.Api.Tests;

public sealed class ApiDocumentationTests
{
    [Fact]
    public async Task Development_serves_openapi_document_and_scalar_ui()
    {
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(b => b.UseEnvironment("Development"));
        using var client = factory.CreateClient();

        var document = await client.GetAsync("/openapi/v1.json");
        var ui = await client.GetAsync("/scalar/v1");

        Assert.Equal(HttpStatusCode.OK, document.StatusCode);
        Assert.Contains("/health", await document.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.OK, ui.StatusCode);
    }

    [Fact]
    public async Task Production_does_not_expose_openapi_document_or_scalar_ui()
    {
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(b => b.UseEnvironment("Production"));
        using var client = factory.CreateClient();

        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/openapi/v1.json")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/scalar/v1")).StatusCode);
    }
}
