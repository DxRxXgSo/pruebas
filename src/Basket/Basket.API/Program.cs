using Basket.API.Data;
using Basket.API.Models;
using BuildingBlocks.Behaviors;
using BuildingBlocks.Exceptions.Handler;
using FluentValidation;
using Marten;
using Microsoft.Extensions.Caching.Distributed;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCarter();
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);


builder.Services.AddMarten(opts =>
{
    opts.Connection(builder.Configuration.GetConnectionString("Database")!);
    opts.Schema.For<ShoppingCart>().Identity(x => x.UserName);
}).UseLightweightSessions();

builder.Services.AddScoped<IBasketRepository, BasketRepository>();

//aqui agregams el patron de dise�o decorador mediante la libreria scrutor, configuramos redis para cache distri
builder.Services.Decorate<IBasketRepository, CacheBasketRepository>();
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "https://lambent-torrone-969915.netlify.app"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddExceptionHandler<CustomExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseCors("AllowFrontend");

app.MapCarter();
app.UseExceptionHandler(options => { });

app.Run();
