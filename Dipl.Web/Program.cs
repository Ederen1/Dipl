using Dipl.Business;
using Dipl.Business.Services;
using Dipl.Business.Services.Extensions;
using Dipl.Web.Components;
using Dipl.Web.Endpoints;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAuthentication("Cookies")
    .AddCookie(opt => { opt.Cookie.Name = "AuthCookie"; })
    .AddMicrosoftAccount(opt =>
    {
        opt.SignInScheme = "Cookies";
        opt.ClientId = configuration["Microsoft:Id"]!;
        opt.ClientSecret = configuration["Microsoft:Secret"]!;
    });

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseLazyLoadingProxies();
    options.UseSqlite("Datasource=main.sqlite3");
    options.EnableSensitiveDataLogging();
    options.EnableDetailedErrors();
});
builder.Services.AddServiceLayer();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();

app.UseDomainWhitelist();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

using (var scope = app.Services.CreateScope())
{
    // var db = scope.ServiceProvider.GetService<AppDbContext>()!.Database;
    // await db.EnsureDeletedAsync();
    // await db.EnsureCreatedAsync();
    // await scope.ServiceProvider.GetService<InitializationService>()!.Initialize();
}

app.MapLoginEndpoints();
app.MapDownloadEndpoints();

app.Run();