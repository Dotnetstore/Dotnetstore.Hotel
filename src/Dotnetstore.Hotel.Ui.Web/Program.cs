using Dotnetstore.Hotel.Shared.Sdk;
using Dotnetstore.Hotel.Ui.Web.Authentication;
using Dotnetstore.Hotel.Ui.Web.Options;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddHotelSdk();
builder.Services.AddUserSdk();
builder.Services.Configure<HotelUiOptions>(builder.Configuration.GetSection("Hotel"));

builder.Services.AddRazorPages();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.LogoutPath = "/Logout";
        options.AccessDeniedPath = "/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.SlidingExpiration = true;
        options.EventsType = typeof(RefreshingCookieEvents);
    });
builder.Services.AddScoped<RefreshingCookieEvents>();
builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultEndpoints();
app.MapRazorPages();

app.Run();
