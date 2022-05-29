using BulkyBook.DataAccess;
using BulkyBook.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using BulkyBook.Utility;
using Stripe;
using BulkyBookWeb.Config;
using BulkyBook.DataAccess.DbInitializer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<StripeKeys>(builder.Configuration.GetSection(StripeKeys.Section));
builder.Services.Configure<SmtpConfig>(builder.Configuration.GetSection(SmtpConfig.Section));
builder.Services.Configure<SendGridConfig>(builder.Configuration.GetSection(SendGridConfig.Section));
builder.Services.Configure<FacebookConfig>(builder.Configuration.GetSection(FacebookConfig.Section));

builder.Services.AddSingleton<IEmailSender, SendGridEmailSender>();

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("DefaultConnection")
    ));

builder.Services
    .AddIdentity<IdentityUser, IdentityRole>()
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IDbInitializer, DbInitializer>();

builder.Services.AddRazorPages();

builder.Services.AddAuthentication().AddFacebook(
    options =>
    {
        var config = builder.Configuration.GetSection(FacebookConfig.Section).Get<FacebookConfig>();
        options.AppId = config.AppId;
        options.AppSecret = config.AppSecret;
    }
);


builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(100);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

SeedDatabase(app);

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

app.Run();

static void SeedDatabase(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
        dbInitializer.Initialize();
    }
}