using ChatPrototipo.API.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(b =>
        b.AllowAnyHeader()
         .AllowAnyMethod()
         .WithOrigins("http://localhost:4200")  // Angular
         .AllowCredentials()
    );
});

var app = builder.Build();

app.UseCors();

app.MapHub<ChatHub>("/chat");

app.Run();