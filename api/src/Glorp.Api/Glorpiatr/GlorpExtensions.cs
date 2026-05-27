using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using Glorp.Api.Json;

namespace Glorp.Api.Glorpiatr;

public static class ConfigurationExtensions
{
    public static IServiceCollection AddGlorp(this IServiceCollection services)
    {
        services.AddSingleton<IGlorpiator, Glorpiator>();
        // services.AddGlorpHandlers();

        return services;
    }

    public static IServiceCollection RegisterGlorpHandlers(this IServiceCollection services)
    {
        var handlerInterface = typeof(IRequestHandler<,>);
        var assembly = Assembly.GetCallingAssembly();

        var handlers = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterface)
                .Select(i => (i, t)));

        foreach (var (service, implementation) in handlers)
        {
            services.AddTransient(service, implementation);
        }

        return services;
    }

    public static IEndpointRouteBuilder MapGlorp(this IEndpointRouteBuilder app, string path = "/glorp")
    {
        app.MapPost(path, async Task<Results<Ok<object>, BadRequest<string>>> (
            HttpContext httpContext,
            IGlorpiator mediator,
            CancellationToken cancellationToken) =>
        {
            var jsonOptions = httpContext.RequestServices
                .GetRequiredService<IOptions<JsonOptions>>()
                .Value.SerializerOptions;

            JsonDocument doc;

            try
            {
                doc = await JsonDocument.ParseAsync(httpContext.Request.Body, cancellationToken: cancellationToken);
            }
            catch (JsonException ex)
            {
                return TypedResults.BadRequest($"Invalid JSON: {ex.Message}");
            }

            using (doc)
            {
                if (!doc.RootElement.TryGetProperty("$type", out var typeProp) ||
                    typeProp.GetString() is not string typeName ||
                    !Configuration.GetTypeRegistry().TryGetValue(typeName, out var concreteType))
                {
                    return TypedResults.BadRequest("Missing or unknown \"$type\" discriminator.");
                }

                var request = JsonSerializer.Deserialize(doc.RootElement.GetRawText(), concreteType, jsonOptions);
                if (request is null)
                {
                    return TypedResults.BadRequest("Request body is empty.");
                }

                var response = await mediator.SendAsync(request, cancellationToken);
                return TypedResults.Ok(response!);
            }
        });

        return app;
    }
}
