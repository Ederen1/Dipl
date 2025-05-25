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
var conf = builder.Configuration;

builder.Services.AddBlazorise(options => { options.Immediate = true; }).AddBootstrap5Providers().AddFontAwesomeIcons();

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.WebHost.ConfigureKestrel(servecrOptions =>
{
    servecrOptions.Limits.MaxRequestBodySize = null;
    servecrOptions.Limits.MinRequestBodyDataRate = null;
});
builder.Services.Configure<FormOptions>(x =>
{
    x.MultipartBodyLengthLimit = long.MaxValue;
    x.ValueLengthLimit = int.MaxValue;
    x.MultipartHeadersLengthLimit = int.MaxValue;
});

builder.Services.Configure<SmtpSettings>(conf.GetSection("SmtpSettings"));
builder.Services.Configure<EmailSenderSettings>(conf.GetSection("EmailSenderSettings"));
builder.Services.Configure<FileStoreServiceConfiguration>(
    conf.GetSection("FileStoreConfiguration:FileStoreService"));
builder.Services.Configure<FTPFileStoreServiceConfiguration>(
    conf.GetSection("FileStoreConfiguration:FtpStoreService"));

if (conf.GetSection("FileStoreConfiguration:FileStoreService").Exists())
    builder.Services.AddScoped<IStoreService, FileStoreService>();
else
    builder.Services.AddScoped<IStoreService, FTPFileStoreService>();


builder.Services.AddAuthentication("Cookies").AddCookie(opt => { opt.Cookie.Name = "AuthCookie"; })
    .AddMicrosoftAccount(opt =>
    {
        opt.SignInScheme = "Cookies";
        opt.ClientId = conf["Authentication:Microsoft:ClientId"]!;
        opt.ClientSecret = conf["Authentication:Microsoft:ClientSecret"]!;

        opt.Events.OnTicketReceived = ctx =>
        {
            var email = ctx.Principal?.FindFirst(ClaimTypes.Email)?.Value;
            email ??= ctx.Principal?.FindFirst("email")?.Value;

            if (string.IsNullOrEmpty(email))
            {
                ctx.Response.Redirect("/failSignin");
                return Task.CompletedTask;
            }

            var domains = conf.GetSection("AllowedDomains").Get<string[]>();
            if (domains?.Any(x => email.EndsWith(x)) == true)
                return Task.CompletedTask;

            var emails = conf.GetSection("AllowedEmails").Get<string[]>();
            if (emails?.Contains(email) == true)
                return Task.CompletedTask;

            ctx.Response.Redirect("/failSignin");
            ctx.HandleResponse();
            return Task.CompletedTask;
        };
    });

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorization();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseLazyLoadingProxies();
    options.UseSqlite(conf["ConnectionStrings:Db"]!);
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
    await db.MigrateAsync();
}

app.MapLoginEndpoints();
app.MapDownloadEndpoints();
app.MapUploadEncpoints();

app.Run();