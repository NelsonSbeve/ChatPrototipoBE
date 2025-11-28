using ChatPrototipo.API.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(
            "http://localhost:4200",  // Local dev
            "https://6929915c24b6860008f68e03--chatprototipo.netlify.app",  // Netlify preview
            "https://chatprototipo.netlify.app"  // Prod
        )
        .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
        .AllowAnyHeader()
        .AllowCredentials()
        .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
});

var app = builder.Build();

// Custom middleware for SignalR OPTIONS preflight (handles 405)
app.Use(async (context, next) =>
{
    if (context.Request.Method == "OPTIONS")
    {
        // Check if origin is allowed (match your policy)
        var origin = context.Request.Headers["Origin"].FirstOrDefault();
        if (!string.IsNullOrEmpty(origin) && new[] {
            "http://localhost:4200",
            "https://6929915c24b6860008f68e03--chatprototipo.netlify.app",
            "https://chatprototipo.netlify.app"
        }.Any(o => o == origin))
        {
            context.Response.Headers.Append("Access-Control-Allow-Origin", origin);
            context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
            context.Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type, Authorization, *");
            context.Response.Headers.Append("Access-Control-Allow-Credentials", "true");
            context.Response.Headers.Append("Access-Control-Max-Age", "600");  // 10min cache
            context.Response.StatusCode = 200;
            context.Response.ContentLength = 0;
            await context.Response.CompleteAsync();  // Short-circuit response
            return;
        }
    }
    await next();  // Continue to other middleware for non-OPTIONS
});

app.UseCors();  // Global for other requests

app.MapHub<ChatHub>("/chat").RequireCors();  // Endpoint policy

app.Run();