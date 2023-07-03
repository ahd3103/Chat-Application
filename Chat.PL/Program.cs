using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Chat.BL.Helper;
using Chat.BL.Servies;
using Chat.DL.DbContexts;
using Chat.DL.Models;
using Chat.DL.Repostiory;
using Microsoft.AspNetCore.Identity;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.PostgreSQL;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ChatDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<IRepository<Message>, Repository<Message>>();
builder.Services.AddScoped<IRepository<User>, Repository<User>>();
//builder.Services.AddScoped<IRepository<LogEntry>, Repository<LogEntry>>();
builder.Services.AddTransient<IMessageService, MessageService>();
builder.Services.AddTransient<IUserService, UserRepository>();


// Adding Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateIssuerSigningKey = true,
        ValidateAudience = true,
        ClockSkew = TimeSpan.Zero,
        ValidAudience = builder.Configuration["JWT:Audience"],
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]))
    };
});

builder.Services.AddAuthorization();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Minimal chat application", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        In = ParameterLocation.Header,
        Name = "Authorization",
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Description = "Enter 'Bearer {token}' in the input box below",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Authorization",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });

});

builder.Services.AddSignalR();
builder.Services.AddControllers();

builder.Host.UseSerilog((context, config) =>
{
    config.WriteTo.PostgreSQL(
        connectionString: "User ID=postgres;Password=Ahd@3103;Host=localhost;Port=5432;Database=ChatApplicationDb;Pooling=true;",
        tableName: "logs",
        needAutoCreateTable: true,
         columnOptions: new Dictionary<string, ColumnWriterBase>
         {
             ["message"] = new RenderedMessageColumnWriter(),
             ["message_template"] = new MessageTemplateColumnWriter(),
             ["level"] = new LevelColumnWriter(),
             ["raise_date"] = new TimestampColumnWriter(),
             ["exception"] = new ExceptionColumnWriter(),
             ["properties"] = new LogEventSerializedColumnWriter()
         }
         ).MinimumLevel.Information()
          .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
          .MinimumLevel.Override("System", LogEventLevel.Error);

    if (context.HostingEnvironment.IsProduction() == false)
    {
        config.WriteTo.Console().MinimumLevel.Information();

    }
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<TokenValidationMiddleware>();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<ChatHub>("/chat");
});

app.Run();
