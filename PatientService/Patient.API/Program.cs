using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Patient.Core.Service.PatientService;
using Patient.Data.Data;
using Patient.Data.Repository.PatientRepository; // Add this using directive

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

#if TEST
builder.Services.AddDbContextFactory<PatientDbContext>(o =>
    o.UseInMemoryDatabase("TestPatientDb"));
#else
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContextFactory<PatientDbContext>(o =>
    o.UseSqlServer(connectionString), ServiceLifetime.Scoped);
#endif
builder.Services.AddSwaggerGen();

builder.Services.AddAutoMapper(typeof(PatientMappingProfile));
builder.Services.AddControllers();
// Autres configurations...

#region Jwt Bearer

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "PAtientAPI",         // même valeur que dans IdentityService
            ValidAudience = "Doctor",     // même valeur que dans IdentityService
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Uwd%6vbxQ3qGUt1BR8Nh16@e61E8z3brB9FAkP!M6U$*TJJfSHmPKAsV4*3C2FY8"))
        };
    });

#endregion

#region Service & Repository

builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IPatientRepository, PatientRepository>();

builder.Services.AddScoped<SeedData>();

#endregion

var app = builder.Build();
app.MapControllers();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Patient API v1");
});

app.UseHttpsRedirection();

#region DataSeeder

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var seedData = services.GetRequiredService<SeedData>();
    await seedData.Initialize(services);
}

#endregion



app.Run();


public partial class Program
{
    
}