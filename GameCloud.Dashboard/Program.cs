using GameCloud.Dashboard.Abstractions;
using GameCloud.Dashboard.Extensions;
using GameCloud.Dashboard.Filters;
using GameCloud.Dashboard.Security;
using Microsoft.Extensions.Caching.Memory;
using NToastNotify;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages()
    .AddMvcOptions(options => { options.Filters.Add<ApiExceptionHandlerFilter>(); })
    .AddRazorRuntimeCompilation()
    .AddNToastNotifyToastr(new ToastrOptions()
    {
        ProgressBar = false,
        PositionClass = ToastPositions.BottomRight
    });


builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = "JwtAuthentication";
        options.DefaultChallengeScheme = "JwtAuthentication";
    })
    .AddCookie(options =>
    {
        options.Cookie.Name = "GameCloud.Dashboard.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.SlidingExpiration = true;
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/access-denied";

        options.Cookie.IsEssential = true;

        options.SessionStore = new MemoryCacheTicketStore(
            builder.Services.BuildServiceProvider().GetRequiredService<IMemoryCache>());
    })
    .AddScheme<JwtAuthenticationOptions, JwtAuthenticationHandler>("JwtAuthentication", options =>
    {
        options.SecretKey = builder.Configuration["Jwt:SecretKey"];
        options.Issuer = builder.Configuration["Jwt:Issuer"];
        options.Audience = builder.Configuration["Jwt:Audience"];
    });


builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.AddSingleton<JwtTokenHandler>();
builder.Services.AddTransient<AuthenticationMessageHandler>();
builder.Services.AddMemoryCache();

builder.Services.AddRefitClient<IDeveloperClient>(builder.Configuration["ApiUrl:GameCloud"] ??
                                                  throw new ArgumentNullException());

builder.Services.AddRefitClient<IGameClient>(builder.Configuration["ApiUrl:GameCloud"] ??
                                             throw new ArgumentNullException());

builder.Services.AddRefitClient<IImagesClient>(builder.Configuration["ApiUrl:GameCloud"] ??
                                               throw new ArgumentNullException());

builder.Services.AddRefitClient<IMatchmakingClient>(builder.Configuration["ApiUrl:GameCloud"] ??
                                               throw new ArgumentNullException());

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseNToastNotify();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();