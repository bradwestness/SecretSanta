using Microsoft.AspNetCore.Authentication.Cookies;
using SecretSanta.Utilities;

var builder = WebApplication.CreateBuilder();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = AppSettings.LoginPath;
        o.LogoutPath = AppSettings.LogoutPath;
        o.ExpireTimeSpan = AppSettings.SessionTimeout;
        o.SlidingExpiration = true;
    });

builder.Services.AddSession(o => o.IdleTimeout = AppSettings.SessionTimeout);
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

var app = builder.Build();

AppSettings.Initialize(app.Configuration);
AccountRepository.Initialize(app.Environment.ContentRootPath);
PreviewGenerator.Initialize(app.Environment.WebRootPath, app.Services.GetRequiredService<IHttpClientFactory>());

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler(AppSettings.ErrorPath);
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