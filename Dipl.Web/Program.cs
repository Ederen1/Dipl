using System.Security.Claims;
using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using Dipl.Business;
using Dipl.Business.Services;
using Dipl.Business.Services.Extensions;
using Dipl.Business.Services.Interfaces;
using Dipl.Common.Configs;
using Dipl.Web.Components;
using Dipl.Web.Endpoints;
using Dipl.Web.Extensions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddBlazorise(options => { options.Immediate = true; }).AddBootstrap5Providers().AddFontAwesomeIcons();

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.WebHost.ConfigureKestrel(servecrOptions => { servecrOptions.Limits.MaxRequestBodySize = null; });
builder.Services.Configure<FormOptions>(x => { x.MultipartBodyLengthLimit = long.MaxValue; });

builder.Services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));
builder.Services.Configure<EmailSenderSettings>(configuration.GetSection("EmailSenderSettings"));
builder.Services.Configure<FileStoreServiceConfiguration>(
    configuration.GetSection("FileStoreConfiguration:FileStoreService"));
builder.Services.Configure<FTPFileStoreServiceConfiguration>(
    configuration.GetSection("FileStoreConfiguration:FtpStoreService"));

if (configuration.GetSection("FileStoreConfiguration:FileStoreService").Exists())
    builder.Services.AddScoped<IStoreService, FileStoreService>();
else
    builder.Services.AddScoped<IStoreService, FTPFileStoreService>();


builder.Services.AddAuthentication("Cookies").AddCookie(opt => { opt.Cookie.Name = "AuthCookie"; })
    .AddMicrosoftAccount(opt =>
    {
        opt.SignInScheme = "Cookies";
        opt.ClientId = configuration["Authentication:Microsoft:ClientId"]!;
        opt.ClientSecret = configuration["Authentication:Microsoft:ClientSecret"]!;

        opt.Events.OnTicketReceived = context =>
        {
            var email = context.Principal?.FindFirst(ClaimTypes.Email)?.Value ??
                        context.Principal?.FindFirst("email")?.Value;

            if (string.IsNullOrEmpty(email))
                return Task.CompletedTask;

            var allowedDomains = configuration.GetSection("AllowedDomains").Get<string[]>();
            if (allowedDomains?.Any(x => email.EndsWith(x)) == true)
                return Task.CompletedTask;

            var allowedEmails = configuration.GetSection("AllowedEmails").Get<string[]>();
            if (allowedEmails?.Contains(email) == true)
                return Task.CompletedTask;

            context.Response.Redirect("/failSignin");
            context.HandleResponse();
            return Task.CompletedTask;
        };
    });

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorization();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseLazyLoadingProxies();
    options.UseSqlite(configuration["ConnectionStrings:Db"]!);
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});
builder.Services.AddServiceLayer();
builder.Services.AddWebServiceLayer();

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

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetService<AppDbContext>()!.Database;
    db.EnsureDeleted();
    db.EnsureCreated();
    // await db.MigrateAsync();
}

app.MapLoginEndpoints();
app.MapDownloadEndpoints();
app.MapUploadEncpoints();

app.Run();