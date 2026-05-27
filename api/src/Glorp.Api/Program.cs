using Glorp.Api;
using Scalar.AspNetCore;
using Glorp.Api.Json;
using Glorp.Api.Generator;
using Glorp.Api.Glorpiatr;

if (args.Length >= 1 && args[0] == "generate-client")
{
    var outputDir = Path.GetFullPath(
        args.Length >= 2 ? args[1] : Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "..", "frontend", "src", "glorp"));

    ClientGenerator.Generate(outputDir);

    return;
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolver = new TypeInfoResolver();
    options.SerializerOptions.Converters.Add(new InterfaceConverterFactory());
});

builder.Services.AddGlorp();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapGlorp();

app.Run();
