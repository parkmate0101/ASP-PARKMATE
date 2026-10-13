using Microsoft.EntityFrameworkCore;
using ParkMate.Data;
using ParkMate.Middleware;
using ParkMate.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddDbContext<ParkMateDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// OTP / Brevo email services
builder.Services.AddScoped<IEmailService, BrevoEmailService>();
builder.Services.AddScoped<OtpService>();
builder.Services.AddScoped<SimplePdfService>();
builder.Services.AddSingleton<ReceiptAccessService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.UseMiddleware<GlobalExceptionMiddleware>();

app.MapStaticAssets();

// REST API controllers
app.MapControllers();

// Existing MVC routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
