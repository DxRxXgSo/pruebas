using BuildingBlocks.Exceptions.Handler;
using Microsoft.Extensions.Options;
using Pdf.API.Endpoints;
using Pdf.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<PdfSettings>(builder.Configuration.GetSection("Pdf"));
var settings = builder.Configuration.GetSection("Pdf").Get<PdfSettings>() ?? new PdfSettings();

builder.Services.AddHttpClient<IOrderingApiClient, OrderingApiClient>(client =>
    client.BaseAddress = new Uri(string.IsNullOrWhiteSpace(settings.OrderingApiBaseUrl)
        ? "http://localhost:8083"
        : settings.OrderingApiBaseUrl));

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddExceptionHandler<CustomExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Pdf.API - Generador de Tickets PDF",
        Version = "v1",
        Description = "Microservicio que genera tickets PDF (recibos de compra) a partir de las órdenes de Ordering.API."
    });
});

var port = Environment.GetEnvironmentVariable("PORT");
if (int.TryParse(port, out var portNumber))
    builder.WebHost.UseUrls($"http://+:{portNumber}");

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowFrontend");

app.UseExceptionHandler(options => { });

TicketEndpoints.MapTicketEndpoints(app);

app.Run();