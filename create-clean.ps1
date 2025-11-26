param (
    [string]$name = "ChatPrototipo"
)

dotnet new sln -n $name

dotnet new webapi -n "$name.API"
dotnet new classlib -n "$name.Application"
dotnet new classlib -n "$name.Domain"
dotnet new classlib -n "$name.Infrastructure"

dotnet sln $name.sln add "$name.API/$name.API.csproj"
dotnet sln $name.sln add "$name.Application/$name.Application.csproj"
dotnet sln $name.sln add "$name.Domain/$name.Domain.csproj"
dotnet sln $name.sln add "$name.Infrastructure/$name.Infrastructure.csproj"

dotnet add "$name.Application/$name.Application.csproj" reference "$name.Domain/$name.Domain.csproj"
dotnet add "$name.Infrastructure/$name.Infrastructure.csproj" reference "$name.Application/$name.Application.csproj"
dotnet add "$name.Infrastructure/$name.Infrastructure.csproj" reference "$name.Domain/$name.Domain.csproj"
dotnet add "$name.API/$name.API.csproj" reference "$name.Application/$name.Application.csproj"
dotnet add "$name.API/$name.API.csproj" reference "$name.Infrastructure/$name.Infrastructure.csproj"

Write-Host "`nDone! Clean architecture solution created: $name"