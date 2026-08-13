using BuildingBlocks.Behaviors;
using BuildingBlocks.Exceptions.Handler;
using FluentValidation;
using Ordering.API.Application.Contracts;
using Ordering.API.Application.Integration;
using Ordering.API.Endpoints;
using Ordering.API.Infrastructure.Configuration;
using Ordering.API.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.Services.Configure<OrderingSettings>(builder.Configuration.GetSection("Ordering"));
var settings = builder.Configuration.GetSection("Ordering").Get<OrderingSettings>() ?? new OrderingSettings();

if (string.IsNullOrWhiteSpace(settings.MongoDbConnectionString))
{
    var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger("Program");
    logger.LogError(
        "No se encontró la cadena de conexión de MongoDB Atlas. Configure la variable de entorno " +
        "Ordering__MongoDbConnectionString (o MONGODB_CONNECTION_STRING). El servicio arranca pero las " +
        "operaciones de órdenes devolverán error hasta que se configure.");
}

var port = Environment.GetEnvironmentVariable("PORT");
if (int.TryParse(port, out var portNumber))
    builder.WebHost.UseUrls($"http://+:{portNumber}");

builder.Services.AddSingleton(new OrderingDbContext(settings.MongoDbConnectionString, settings.MongoDbDatabaseName));
builder.Services.AddScoped<IOrderRepository, MongoDbOrderRepository>();

builder.Services.AddHttpClient<IBasketApiClient, BasketApiClient>(client =>
    client.BaseAddress = new Uri(string.IsNullOrWhiteSpace(settings.BasketApiBaseUrl)
        ? "http://localhost:8082"
        : settings.BasketApiBaseUrl));

builder.Services.AddHttpClient<ICatalogApiClient, CatalogApiClient>(client =>
    client.BaseAddress = new Uri(string.IsNullOrWhiteSpace(settings.CatalogApiBaseUrl)
        ? "http://localhost:8080"
        : settings.CatalogApiBaseUrl));

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
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Ordering.API - Microservicio de Órdenes de Compra",
        Version = "v1",
        Description = "Microservicio de órdenes de compra con ASP.NET Core Minimal API y MongoDB Atlas. " +
                       "Usa el header Idempotency-Key para evitar órdenes duplicadas."
    });
});

var app = builder.Build();

var dbContext = app.Services.GetRequiredService<OrderingDbContext>();
_ = Task.Run(async () =>
{
    try
    {
        await dbContext.EnsureIndexesAsync();
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "No se pudieron garantizar los índices de MongoDB al iniciar. " +
            "Se reintentará en la siguiente operación; el servicio continúa iniciando.");
    }
});

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowFrontend");

app.UseExceptionHandler(options => { });

app.MapOrdersEndpoints();

app.Run();