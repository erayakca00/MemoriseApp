using Microsoft.AspNetCore.Components.Server;
using MemoriseApp.Components;
using MemoriseApp.Data;
using MemoriseApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using MemoriseApp.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = 
    builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new
    InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Veritabaný oluþturulmasý sýrasýnda oluþan sorunlarý daha detaylý gösteren servis.
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options =>
    {
        options.DetailedErrors = true; // Hata ayýklama için detaylý hata mesajlarý gösterir.
    });
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// ASP.NET Core Identity servislerini ekleyin
builder.Services.AddDefaultIdentity<IdentityUser>(options => { /* ... */ })
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Blazor'un kimlik doðrulama durumunu yönetmesi için
builder.Services.AddCascadingAuthenticationState(); // Bu, AuthenticationStateProvider'ý kullanýlabilir hale getirir
builder.Services.AddScoped<AuthenticationStateProvider,ServerAuthenticationStateProvider>();  //RevalidatingIdentityAuthenticationStateProvider EKLENECEK    

// Özel Kullanýcý Servisini Kaydet
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<SrsService>(); // SRS servisini ekle

builder.Services.Configure<Microsoft.AspNetCore.HttpsPolicy.HttpsRedirectionOptions>(options =>
{
    // launchSettings.json'daki HTTPS portunu buraya yazýn
    options.HttpsPort = 7105;
});



var app = builder.Build();

app.UseHttpsRedirection();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorPages();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await app.RunAsync();
