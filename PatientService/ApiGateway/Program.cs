using Microsoft.OpenApi.Models;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using MMLib.SwaggerForOcelot.DependencyInjection;
using MMLib.SwaggerForOcelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Charger la configuration Ocelot depuis ocelot.json
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Ajoute SwaggerGen classique (nécessaire pour SwaggerForOcelot)
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Gateway API", Version = "v1" });
});

// Ajouter SwaggerForOcelot (pour agréger la documentation des microservices)
builder.Services.AddSwaggerForOcelot(builder.Configuration);

// Ajouter Ocelot
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

// Mappe le chemin "/swagger" pour que SwaggerForOcelotUI prenne en charge ces requêtes
app.Map("/swagger", swaggerApp =>
{
    var swaggerSection = builder.Configuration.GetSection("SwaggerEndPoints");
    if (!swaggerSection.Exists())
    {
        throw new Exception("La section SwaggerEndPoints est manquante dans ocelot.json");
    }
});

// Toutes les autres requêtes passent par Ocelot
await app.UseOcelot();

app.Run();

