using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// Load configuration
var configuration = builder.Configuration;

// Configure Kestrel
builder.WebHost.ConfigureKestrel(options =>
{
    // Configure HTTP endpoint
    var httpEndpoint = configuration.GetSection("Kestrel:Endpoints:Http");
    if (httpEndpoint.Exists())
    {
        options.Listen(System.Net.IPAddress.Any, 5000);
    }

    // Configure HTTPS endpoint if enabled
    var httpsConfig = configuration.GetSection("Kestrel:Endpoints:Https");
    if (httpsConfig.GetValue<bool>("Enabled"))
    {
        var certPath = httpsConfig.GetValue<string>("Certificate:Path");
        var certPassword = httpsConfig.GetValue<string>("Certificate:Password");

        if (!string.IsNullOrEmpty(certPath) && File.Exists(certPath))
        {
            options.Listen(System.Net.IPAddress.Any, 5001, listenOptions =>
            {
                listenOptions.UseHttps(new X509Certificate2(certPath, certPassword));
            });
        }
    }
});

// Add services to the container
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure HTTP-to-HTTPS redirection if enabled
if (configuration.GetValue<bool>("RedirectHttpToHttps"))
{
    app.UseHttpsRedirection();
}

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();