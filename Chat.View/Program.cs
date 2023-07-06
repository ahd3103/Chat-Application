
using Chat.DL.DbContexts;
using Chat.View.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ChatDbContext>(options => options.UseNpgsql(builder.Configuration["ConnectionStrings:DefaultConnection"]));
builder.Services.AddIdentity<AppUser, IdentityRole>().AddEntityFrameworkStores<ChatDbContext>().AddDefaultTokenProviders();
builder.Services.Configure<DataProtectionTokenProviderOptions>(opts => opts.TokenLifespan = TimeSpan.FromHours(10));



// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication()
          .AddGoogle(opts =>
          {
              opts.ClientId = builder.Configuration["Google:client_id"];
              opts.ClientSecret = builder.Configuration["Google:client_secret"];
              opts.SignInScheme = IdentityConstants.ExternalScheme;
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
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Google}/{action=Index}/{id?}");

app.Run();
