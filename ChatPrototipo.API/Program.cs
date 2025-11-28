using ChatPrototipo.API.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(
            "http://localhost:4200",
            "https://6929915c24b6860008f68e03--chatprototipo.netlify.app",
            "https://chatprototipo.netlify.app"
        )
        .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
        .AllowAnyHeader()
        .AllowCredentials()
        .SetPreflightMaxAge(TimeSpan.FromMinutes(10)));
});

var allowedOrigins = new[]
{
    "http://localhost:4200",
    "https://6929915c24b6860008f68e03--chatprototipo.netlify.app",
    "https://chatprototipo.netlify.app"
};

var app = builder.Build();

// Custom middleware for SignalR OPTIONS preflight (handles 405)
app.Use(async (context, next) =>
{
    if (context.Request.Method == "OPTIONS")
    {
        var origin = context.Request.Headers["Origin"].FirstOrDefault();

        if (!string.IsNullOrEmpty(origin) && allowedOrigins.Contains(origin))
        {
            // Allow origin
            context.Response.Headers.Append("Access-Control-Allow-Origin", origin);

            // Allow credentials
            context.Response.Headers.Append("Access-Control-Allow-Credentials", "true");

            // Allow all common methods
            context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");

            // MIRROR BACK REQUESTED HEADERS (FIX)
            var requestedHeaders = context.Request.Headers["Access-Control-Request-Headers"];
            if (!string.IsNullOrEmpty(requestedHeaders))
            {
                context.Response.Headers.Append("Access-Control-Allow-Headers", requestedHeaders);
            }
            else
            {
                // fallback: allow all
                context.Response.Headers.Append("Access-Control-Allow-Headers", "*");
            }

            context.Response.Headers.Append("Access-Control-Max-Age", "600");
            context.Response.StatusCode = 200;
            await context.Response.CompleteAsync();
            return;
        }
    }

    await next();
});
app.UseCors();  // Global for other requests

app.MapHub<ChatHub>("/chat").RequireCors();  // Endpoint policy

app.Run();