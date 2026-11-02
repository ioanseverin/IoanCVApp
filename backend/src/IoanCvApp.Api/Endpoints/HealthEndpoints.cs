using IoanCvApp.Api.Contracts;
using Microsoft.AspNetCore.Http.HttpResults;

namespace IoanCvApp.Api.Endpoints;

public static class HealthEndpoints
{
    public static WebApplication MapHealth(this WebApplication app)
    {
        app.MapGet("/health", Ok<HealthResponse> () => TypedResults.Ok(new HealthResponse("Healthy")))
            .WithName("GetHealth")
            .WithSummary("Check API liveness")
            .WithDescription("Returns 200 OK when the API process is running and able to serve requests.");

        return app;
    }
}
