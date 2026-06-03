using CleanrApp.Data;
using CleanrApp.Hubs;
using CleanrApp.Models;
using CleanrApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database – SQLite
var dbFolder = Path.Combine(builder.Environment.ContentRootPath, "Data");
Directory.CreateDirectory(dbFolder);
var dbPath = Path.Combine(dbFolder, "cleanrapp.db");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Cookie config
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
});

// SignalR
builder.Services.AddSignalR();

// App services
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<AuditService>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<NotificationHub>("/notificationHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// Seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
    await SeedData.Initialize(scope.ServiceProvider);
}

app.Run();
