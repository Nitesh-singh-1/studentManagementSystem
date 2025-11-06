using EmployeeManagementSystem.Context;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection("ApiSettings"));
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpClient();
builder.Services.AddSingleton<ApiService>();

var app = builder.Build();

var config = app.Services.GetRequiredService<IConfiguration>();
app.Use(async (context, next) =>
{
    var apiBaseUrl = config["ApiSettings:BaseUrl"];
    context.Items["ApiBaseUrl"] = apiBaseUrl;
    await next();
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Session must be before authorization and endpoint mapping
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();
