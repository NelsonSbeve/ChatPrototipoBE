using ChatPrototipo.API.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{

    options.AddPolicy("AllowNetlify", policy =>
    {
        policy.WithOrigins(
            "http://localhost:4200",
            "https://6929915c24b6860008f68e03--chatprototipo.netlify.app",  // Your current preview
            "https://chatprototipo.netlify.app"  // For prod/custom domain later
        )
        .AllowAnyHeader()  // Allows custom headers (e.g., Authorization)
        .AllowAnyMethod()  // GET, POST, OPTIONS for negotiation
        .AllowCredentials();  // For auth cookies if used
    });
});

var app = builder.Build();

app.UseCors();

app.MapHub<ChatHub>("/chat");

app.Run();