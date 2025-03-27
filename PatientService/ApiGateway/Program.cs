using Microsoft.OpenApi.Models;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using MMLib.SwaggerForOcelot.DependencyInjection;
using MMLib.SwaggerForOcelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Charger la configuration Ocelot
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Ajouter EndpointsApiExplorer et SwaggerGen (nécessaires pour SwaggerForOcelot)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Gateway API", Version = "v1" });
});

// Ajouter SwaggerForOcelot (agrège la doc des microservices)
builder.Services.AddSwaggerForOcelot(builder.Configuration);

// Ajouter Ocelot
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

// Mapper explicitement le chemin "/swagger" pour SwaggerForOcelotUI
app.Map("/swagger", swaggerApp =>
{
    swaggerApp.UseSwaggerForOcelotUI(opt =>
    {
        opt.PathToSwaggerGenerator = "/swagger/docs";
    });
});

// Toutes les autres requêtes passent par Ocelot
await app.UseOcelot();

app.Run();

public partial class Program { }