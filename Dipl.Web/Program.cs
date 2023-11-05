using System.IO.Compression;
using System.Security.Claims;
using Dipl.Business;
using Dipl.Business.Extensions;
using Dipl.Business.Services;
using Dipl.Business.Services.Extensions;
using Dipl.Web.Components;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
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

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetService<AppDbContext>()!.Database;
    await db.EnsureDeletedAsync();
    await db.EnsureCreatedAsync();
    await scope.ServiceProvider.GetService<InitializationService>()!.Initialize();
}

app.MapGet("/Account/Login", () =>
{
    var props = new AuthenticationProperties
    {
        RedirectUri = "/Account/LoginCallback"
    };

    return Results.Challenge(props, new[] { MicrosoftAccountDefaults.AuthenticationScheme });
});

app.MapGet("/Account/LoginCallback", async (HttpContext context, UsersService _userService) =>
{
    var identity = context.User.Identity as ClaimsIdentity;
    if (identity == null || !identity.IsAuthenticated)
        return Results.Redirect("/");

    var user = identity.MapToUser();
    await _userService.CreateIfNotExists(user);

    return Results.Redirect("/");
});

app.MapGet("/Account/Logout", async (HttpContext httpContext) =>
{
    await httpContext.SignOutAsync();
    return Results.Redirect("/");
});

app.MapGet("/download/{linkId:guid}", async (Guid linkId, LinksService linksService, HttpContext context) =>
{
    var files = await linksService.EnumerateLink(linkId);

    // TODO: maybe estimate final zip size?
    // context.Response.Headers.ContentLength = files.Sum(x => x.Size);

    using var archive = new ZipArchive(context.Response.BodyWriter.AsStream(), ZipArchiveMode.Create, true);
    foreach (var file in files)
    {
        using var fs = File.OpenRead(file.Path);
        var entry = archive.CreateEntry(file.Name, CompressionLevel.NoCompression);
        using var entryStream = entry.Open();
        await fs.CopyToAsync(entryStream);
    }
});

app.MapGet("/download/{linkId:guid}/{fileName}", async (Guid linkId, string fileName, LinksService linksService) =>
{
    var file = await linksService.GetFile(linkId, fileName);
    return Results.File(file, "application/octet-stream");
});

app.Run();