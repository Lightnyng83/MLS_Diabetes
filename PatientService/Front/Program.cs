using System;

var builder = WebApplication.CreateBuilder(args);

// Ajoutez HttpClient
builder.Services.AddHttpClient("ApiGateway", client =>
{
    client.BaseAddress = new Uri("http://localhost:8000"); // URL de l'API Gateway
});

// Pour faciliter l'injection dans les contrôleurs, vous pouvez enregistrer le HttpClient par défaut:
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("ApiGateway"));

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configurez le pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();