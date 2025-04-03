using Microsoft.AspNetCore.Mvc;
using System;

var builder = WebApplication.CreateBuilder(args);

// Ajoutez HttpClient
builder.Services.AddHttpClient("ApiGateway", client =>
{
    client.BaseAddress = new Uri("http://apigateway:8080");
});

// Pour faciliter l'injection dans les contrôleurs, vous pouvez enregistrer le HttpClient par défaut:
builder.Services.AddScoped<HttpClient>(sp =>
{
    return sp.GetRequiredService<IHttpClientFactory>().CreateClient("ApiGateway");
});
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new IgnoreAntiforgeryTokenAttribute());
});
var app = builder.Build();

// Configurez le pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
//app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapDefaultControllerRoute();


app.Run();