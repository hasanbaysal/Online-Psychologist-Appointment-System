using AspNetCoreHero.ToastNotification;
using HB.OnlinePsikologMerkezi.Business.DependencyResolver;
using HB.OnlinePsikologMerkezi.Common.Defaults;
using HB.OnlinePsikologMerkezi.Common.Options;
using HB.OnlinePsikologMerkezi.Common.Utilities;
using HB.OnlinePsikologMerkezi.Data.Context;
using HB.OnlinePsikologMerkezi.Data.Interface;
using HB.OnlinePsikologMerkezi.Entities.Entities;
using HB.OnlinePsikologMerkezi.Web.BackgroundService;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);


//senkron operasyon kodu

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.AllowSynchronousIO = true;
});

builder.Services.Configure<IISServerOptions>(options =>
{
    options.AllowSynchronousIO = true;
});

//api call için httpclient


//backgroud service
builder.Services.AddHostedService<Control3DFail>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAnyOrigin",
        builder =>
        {
            builder.AllowAnyOrigin();
        });
});


builder.Services.AddHttpClient();

PaymentStringDefault.IyzicoApiKey = builder.Configuration.GetValue<string>("IyzicoApiKey");
PaymentStringDefault.IyzicoApiSecret = builder.Configuration.GetValue<string>("IyzicoSecretKey");
PaymentStringDefault.IyzicoApiBaseUrl = builder.Configuration.GetValue<string>("IyzicoBaseUrl");

//business dependencies

builder.Services.AddBusinessDependencies(info =>
{
    info.ConStr = builder.Configuration.GetConnectionString("SqlCon");
}, Assembly.GetExecutingAssembly()); ;


//file provider
builder.Services.AddSingleton<IFileProvider>(new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")));

//cache service
builder.Services.AddMemoryCache();


//toaster
builder.Services.AddNotyf(config => { config.DurationInSeconds = 10; config.IsDismissable = true; config.Position = NotyfPosition.TopRight; });

//crypto option
CustomEncryption.LoadKeyAndIv(
    builder.Configuration.GetSection("key").Get<string[]>(),
    builder.Configuration.GetSection("iv").Get<string[]>());

//email option
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("EmailOptions"));




//cookie ayarlarý DAHA SONRA DÜZENLE

builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.LoginPath = new PathString("/Account/Login");
    opt.Cookie = new CookieBuilder()
    {
        Name = "OnlinePsikologMerkezi",
        HttpOnly = false,
      //  Expiration = TimeSpan.FromDays(5)
    };
    opt.ExpireTimeSpan = TimeSpan.FromDays(5);
    opt.SlidingExpiration = true;
    opt.LogoutPath = new PathString("/Account/Logout");
    opt.AccessDeniedPath = new PathString("/Account/AccesDenied");
});


//google ile giriþ
builder.Services.AddAuthentication()
    .AddGoogle(opt =>
    {

        opt.SaveTokens = true;
        opt.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
        opt.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
        //opt.CallbackPath = new PathString("/Account/ExternalLogin");
        //opt.AuthorizationEndpoint
    });

builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbcontext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbcontext.Database.Migrate();

    var rolemanager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();

    var usermanager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

    var result = await rolemanager.CreateAsync(new() { Name = RoleDefaults.psk });

    await rolemanager.CreateAsync(new() { Name = RoleDefaults.member });
    await rolemanager.CreateAsync(new() { Name = RoleDefaults.admin });

    await usermanager.CreateAsync(new() { Email = "adminonlinepsikoloji@admin.com", UserName = "sistemadmin1" , EmailConfirmed=true}, "23Bx77jAQQm23");

    await usermanager.CreateAsync(new() { Email = "adminonlinepsikoloji2@admin.com", UserName = "sistemadmin2", EmailConfirmed = true }, "23Bx77jxjQm22");


    var admin1 = await usermanager.FindByNameAsync("sistemadmin1");
    var admin2 = await usermanager.FindByNameAsync("sistemadmin2");


    await usermanager.AddToRoleAsync(admin1, RoleDefaults.admin);
    await usermanager.AddToRoleAsync(admin2, RoleDefaults.admin);


}


if (!app.Environment.IsDevelopment())
{
    //loglama yapmam lazým
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


app.UseStatusCodePagesWithReExecute("/Home/Status", "?code{0}");
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowAnyOrigin");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "Areadefault",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");



app.Run();
