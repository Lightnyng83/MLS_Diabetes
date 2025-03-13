using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
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

builder.Services.AddAutoMapper(typeof(PatientMappingProfile));
builder.Services.AddControllers();
// Autres configurations...

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

public partial class Program { }

