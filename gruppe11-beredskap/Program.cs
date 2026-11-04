using System.Globalization;
using gruppe11_beredskap.DataAccess;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using gruppe11_beredskap.DataAccess.Repositories;
using gruppe11_beredskap.Models.Entities;
using gruppe11_beredskap.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Tvinger punktum som desimaltegn (en-US). Nødvendig fordi kartet (JavaScript)
// sender koordinater med punktum, og norsk kultur ellers feiler på Latitude/Longitude.
var supportedCultures = new[] { new CultureInfo("en-US") };
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("en-US");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

// Kobler appen til MariaDB. Connection string kommer fra appsettings.Development.json
// (dotnet run) eller fra miljøvariabelen i compose.yaml (Docker).
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Mangler ConnectionStrings:DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        connectionString,
        new MariaDbServerVersion(new Version(11, 4, 0)),
        mySqlOptions => mySqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null)));
builder.Services.AddScoped<IRessursRepository, RessursRepository>();
builder.Services.AddScoped<IBehovRepository, BehovRepository>();
builder.Services.AddScoped<IMatchingRepository, MatchingRepository>();

// Innlogging med ASP.NET Core Identity. Brukere og roller lagres i MariaDB via ApplicationDbContext.
builder.Services.AddSingleton<IEmailSender, ConsoleEmailSender>();
builder.Services
    .AddIdentity<Bruker, IdentityRole>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.User.RequireUniqueEmail = true;
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 10;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddErrorDescriber<NorskIdentityErrorDescriber>()
    .AddDefaultTokenProviders();

var app = builder.Build();

// Kjører migrasjonene ved oppstart, slik at tabellene opprettes første gang appen starter.
// Deretter opprettes rollene, og i utvikling også testbrukerne (én per rolle).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await IdentitySeeder.SeedRollerAsync(roleManager);

    var testpassord = app.Configuration["Testbrukere:Passord"];
    if (app.Environment.IsDevelopment() && !string.IsNullOrEmpty(testpassord))
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Bruker>>();
        await IdentitySeeder.SeedTestbrukereAsync(userManager, testpassord);
    }
}

app.UseRequestLocalization();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

// Først finner vi ut hvem brukeren er (innlogging), så hva hen har lov til (roller)
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

// Kobling-siden får adressen /Kobling. Controlleren heter fortsatt MatchingController,
// og den gamle adressen /Matching virker fortsatt via standardruten under.
app.MapControllerRoute(
        name: "kobling",
        pattern: "Kobling/{action=Index}/{id?}",
        defaults: new { controller = "Matching" })
    .WithStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
