using FluentValidation;
using Npgsql;
using System.Text.Encodings.Web;
using System.Text.Json;
using TestovoeJob.API.Models;
using TestovoeJob.API.Services;

var builder = WebApplication.CreateBuilder(args);

JsonSerializerOptions jsonOptions = CreateJsonOptions();

builder.Services.AddSingleton(
    NpgsqlDataSource.Create(builder.Configuration.GetConnectionString("Postgres")!));

builder.Services.AddScoped<IValidator<ProcessingRequest>, ProcessingRequestValidator>();
builder.Services.AddScoped<ProcessingService>();
builder.Services.AddScoped<ElementRepository>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = jsonOptions.PropertyNamingPolicy;
    options.SerializerOptions.DictionaryKeyPolicy = jsonOptions.DictionaryKeyPolicy;
    options.SerializerOptions.PropertyNameCaseInsensitive = jsonOptions.PropertyNameCaseInsensitive;
    options.SerializerOptions.WriteIndented = jsonOptions.WriteIndented;
    options.SerializerOptions.Encoder = jsonOptions.Encoder;
});

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = jsonOptions.PropertyNamingPolicy;
        options.JsonSerializerOptions.DictionaryKeyPolicy = jsonOptions.DictionaryKeyPolicy;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = jsonOptions.PropertyNameCaseInsensitive;
        options.JsonSerializerOptions.WriteIndented = jsonOptions.WriteIndented;
        options.JsonSerializerOptions.Encoder = jsonOptions.Encoder;
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.RoutePrefix = "api/swagger";
    options.SwaggerEndpoint("/openapi/v1.json", "TestovoeJob API v1");
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


static JsonSerializerOptions CreateJsonOptions() => new(JsonSerializerDefaults.Web)
{
    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower,
    PropertyNameCaseInsensitive = false,
    WriteIndented = true,
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
};
