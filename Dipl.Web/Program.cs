using Dipl.Web.Components;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAuthentication("Cookies")
    .AddCookie(opt =>
    {
        opt.Cookie.Name = "AuthCookie";
    })
    .AddMicrosoftAccount(opt =>
    {
        opt.SignInScheme = "Cookies";
        opt.ClientId = configuration["Microsoft:Id"]!;
        opt.ClientSecret = configuration["Microsoft:Secret"]!;
    });

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

app.MapGet("/login/microsoft", (string redirectUri) =>
{
    var props = new AuthenticationProperties
    {
        RedirectUri = redirectUri
    };

    return Results.Challenge(props, new[] { MicrosoftAccountDefaults.AuthenticationScheme });
});

app.MapGet("/login/logout", async (HttpContext httpContext) =>
{
    await httpContext.SignOutAsync();
    return Results.Redirect("/");
});

app.Run();
