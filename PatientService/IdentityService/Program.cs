using IdentityService.Data;
using IdentityService.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configuration de la chaîne de connexion pour Identity
var connectionString = builder.Configuration.GetConnectionString("IdentityConnection")
                       ?? "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=IdentityServiceDB";

// Enregistrer le DbContext d'identité
builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseSqlServer(connectionString));

// Configurer Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<IdentityDbContext>()
    .AddDefaultTokenProviders();

#region JwtBearer
// Configurer l'authentification JWT
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
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
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });
#endregion

// Configuration unique de SwaggerGen
builder.Services.AddSwaggerGen(c =>
{
    // Définir le SwaggerDoc
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Identity API",
        Version = "v1"
    });

    // Ajout de la définition de sécurité
    // Utilisation du schéma HTTP "bearer" (Note : ici le Type est Http et le Scheme est "bearer")
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",           // utilisez "bearer" ici (en minuscule)
        BearerFormat = "JWT",
        Description = "Entrez 'Bearer' suivi d'un espace et du token"
    };

    c.AddSecurityDefinition("Bearer", securityScheme);

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Middleware
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity API v1");
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

#region ----- INITIALIZATION -----

SeedData.Initialize(app.Services);

#endregion ----- INITIALIZATION -----

app.Run();
