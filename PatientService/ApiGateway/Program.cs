using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using MMLib.SwaggerForOcelot.DependencyInjection;
using MMLib.SwaggerForOcelot.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// Charger la configuration depuis ocelot.json
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
#region Jwt Bearer

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // qui est "Bearer"
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "PAtientAPI",
            ValidAudience = "Doctor",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Uwd%6vbxQ3qGUt1BR8Nh16@e61E8z3brB9FAkP!M6U$*TJJfSHmPKAsV4*3C2FY8"))
        };
    });


#endregion

// Pour .NET 9, ajoutez EndpointsApiExplorer avant SwaggerGen
builder.Services.AddEndpointsApiExplorer();


// Ajouter Ocelot
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();


// Pour toutes les autres requêtes, utiliser Ocelot
await app.UseOcelot();


app.Run();

