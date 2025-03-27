using Microsoft.OpenApi.Models;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using MMLib.SwaggerForOcelot.DependencyInjection;
using MMLib.SwaggerForOcelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Charger la configuration depuis ocelot.json
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Pour .NET 9, ajoutez EndpointsApiExplorer avant SwaggerGen
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Ajouter SwaggerForOcelot
builder.Services.AddSwaggerForOcelot(builder.Configuration);

// Ajouter Ocelot
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

// Mapper explicitement le chemin pour SwaggerForOcelotUI afin que les requêtes ne passent pas par Ocelot
app.UseSwaggerForOcelotUI(opt =>
{
    opt.PathToSwaggerGenerator = "/swagger/docs";
});
await Task.Delay(TimeSpan.FromSeconds(5));

// Pour toutes les autres requêtes, utiliser Ocelot
await app.UseOcelot();

app.Run();

