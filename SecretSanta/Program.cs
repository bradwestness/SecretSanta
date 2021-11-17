using Microsoft.AspNetCore.Authentication.Cookies;
using SecretSanta.Utilities;

var builder = WebApplication.CreateBuilder();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/Account/LogIn";
        o.LogoutPath = "/Account/LogOut";
        o.ExpireTimeSpan = TimeSpan.FromHours(1);
        o.SlidingExpiration = true;
    });

builder.Services.AddSession(o => o.IdleTimeout = TimeSpan.FromHours(1));
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddSecretSanta(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
app.UseEndpoints(endpoints => endpoints.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"));
app.Run();