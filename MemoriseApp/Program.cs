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

// Veritaban� olu�turulmas� s�ras�nda olu�an sorunlar� daha detayl� g�steren servis.
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options =>
    {
        options.DetailedErrors = true; // Hata ay�klama i�in detayl� hata mesajlar� g�sterir.
    });
builder.Services.AddRazorPages();
//builder.Services.AddServerSideBlazor();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// ASP.NET Core Identity servislerini ekleyin
builder.Services.AddDefaultIdentity<IdentityUser>(options => { /* ... */ })
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Blazor'un kimlik do�rulama durumunu y�netmesi i�in
//builder.Services.AddCascadingAuthenticationState(); // Bu, AuthenticationStateProvider'� kullan�labilir hale getirir
//builder.Services.AddScoped<AuthenticationStateProvider,ServerAuthenticationStateProvider>();  //RevalidatingIdentityAuthenticationStateProvider EKLENECEK    

// �zel Kullan�c� Servisini Kaydet
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<SrsService>(); // SRS servisini ekle

builder.Services.Configure<Microsoft.AspNetCore.HttpsPolicy.HttpsRedirectionOptions>(options =>
{
    // launchSettings.json'daki HTTPS portunu buraya yaz�n
    options.HttpsPort = 7105;
});



var app = builder.Build();

app.UseHttpsRedirection(); // Genellikle en ba�larda
app.UseStaticFiles();    // Statik dosyalar i�in

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseRouting();        // Y�nlendirme i�in endpoint'leri bulmadan �nce

app.UseAuthentication(); // Kim oldu�unu belirle
app.UseAuthorization();  // Ne yapabilece�ini belirle

app.UseAntiforgery();    // Antiforgery token'lar� i�in

// Endpoint'leri map etme
app.MapRazorPages();     // Razor Pages i�in
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode(); // Blazor Components i�in

app.MapGet("/hello", () => "Merhaba D�nya!"); // Basit bir test endpoint'i

await app.RunAsync();
