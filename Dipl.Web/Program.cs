using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using Dipl.Business;
using Dipl.Business.Services.Extensions;
using Dipl.Common.Configs;
using Dipl.Web.Components;
using Dipl.Web.Endpoints;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services
    .AddBlazorise(options => { options.Immediate = true; })
    .AddBootstrap5Providers()
    .AddFontAwesomeIcons();

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));
builder.Services.Configure<EmailSenderSettings>(configuration.GetSection("EmailSenderSettings"));
builder.Services.Configure<FileStoreConfiguration>(configuration.GetSection("FileStoreConfiguration"));

builder.Services
    .AddAuthentication("Cookies")
    .AddCookie(opt => { opt.Cookie.Name = "AuthCookie"; })
    .AddMicrosoftAccount(opt =>
    {
        opt.SignInScheme = "Cookies";
        opt.ClientId = configuration["Authentication:Microsoft:ClientId"]!;
        opt.ClientSecret = configuration["Authentication:Microsoft:ClientSecret"]!;
    });

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseLazyLoadingProxies();
    options.UseSqlite(configuration["ConnectionStrings:DiplDb"]!);
    options.EnableSensitiveDataLogging();
    options.EnableDetailedErrors();
});
builder.Services.AddServiceLayer();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();

app.UseDomainWhitelist();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetService<AppDbContext>()!.Database;
    await db.MigrateAsync();
}

app.MapLoginEndpoints();
app.MapDownloadEndpoints();

app.Run();