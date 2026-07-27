using Basket.API.Data;
using BuildingBlocks.Behaviors;
using BuildingBlocks.Exceptions.Handler;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCarter();
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.Services.AddScoped<IBasketRepository, BasketRepository>();

var redisUrl = builder.Configuration["UPSTASH_REDIS_REST_URL"];
var redisToken = builder.Configuration["UPSTASH_REDIS_REST_TOKEN"];
if (!string.IsNullOrEmpty(redisUrl) && !string.IsNullOrEmpty(redisToken))
{
    var redisHost = new Uri(redisUrl).Host;
    builder.Services.AddStackExchangeRedisCache(options =>
        options.Configuration = $"{redisHost}:6379,password={redisToken},ssl=True,abortConnect=False");
}
else
{
    builder.Services.AddStackExchangeRedisCache(options =>
        options.Configuration = builder.Configuration.GetConnectionString("Redis")!);
}

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

var app = builder.Build();

app.UseCors("AllowFrontend");

app.UsePathBase("/api");
app.MapCarter();
app.UseExceptionHandler(options => { });

app.Run();
