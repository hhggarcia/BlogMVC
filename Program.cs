using BlogMVC.Config;
using BlogMVC.Data;
using BlogMVC.Entity;
using BlogMVC.Services;
using BlogMVC.Utils;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenAI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<ConfiguracionesIA>()
    .Bind(builder.Configuration.GetSection(ConfiguracionesIA.Seccion))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddScoped(sp =>
{
    var configAI = sp.GetRequiredService<IOptions<ConfiguracionesIA>>();
    return new OpenAIClient(configAI.Value.KeyOpenAI);
});


builder.Services.AddServerSideBlazor();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddTransient<IAlmacenadorArchivo, AlmacenadorArchivosLocal>();
builder.Services.AddTransient<IServicioUsuarios, ServicioUsuarios>();
builder.Services.AddTransient<IServicioChat, ServicioChatOpenAI>();

builder.Services.AddDbContextFactory<ApplicationDbContext>(options => 
options.UseSqlServer("name=DefaultConnection")
.UseSeeding(Seeding.Aplicar)
.UseAsyncSeeding(Seeding.AplicarAsync));

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
}).AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

builder.Services.PostConfigure<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme, options =>
{
    options.LoginPath = "/user/login";
    options.AccessDeniedPath = "/user/login";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapBlazorHub();

app.Run();
