
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
            opts.ClientId = "117798141181-s4o63n2nqiv0kqt5k8ub9rt34u4bbaqf.apps.googleusercontent.com";
            opts.ClientSecret = "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";
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
