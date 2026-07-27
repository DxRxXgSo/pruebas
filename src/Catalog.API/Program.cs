using Catalog.API.Exceptions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddCarter();

var databaseConnection = builder.Configuration.GetConnectionString("Database")!;
databaseConnection = ConvertPostgresUriToNpgsql(databaseConnection);
builder.Services.AddMarten(opts =>
{
    opts.Connection(databaseConnection);
}).UseLightweightSessions();

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

app.MapCarter();
app.UseExceptionHandler(options => { });

app.Run();

static string ConvertPostgresUriToNpgsql(string uri)
{
    if (!uri.StartsWith("postgresql://")) return uri;
    var parsed = new Uri(uri);
    var userInfo = parsed.UserInfo.Split(':');
    var user = Uri.UnescapeDataString(userInfo[0]);
    var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";
    var host = parsed.Host;
    var port = parsed.Port > 0 ? parsed.Port : 5432;
    var db = parsed.AbsolutePath.TrimStart('/');
    var result = $"Host={host};Port={port};Database={db};User Id={user};Password={password};";

    var query = parsed.Query.TrimStart('?');
    if (!string.IsNullOrEmpty(query))
        foreach (var param in query.Split('&'))
        {
            var parts = param.Split('=');
            if (parts.Length == 2)
            {
                var key = parts[0] == "sslmode" ? "SSL Mode" : parts[0];
                var val = parts[0] == "sslmode"
                    ? char.ToUpper(parts[1][0]) + parts[1][1..]
                    : parts[1];
                result += $"{key}={val};";
            }
        }
    return result;
}
