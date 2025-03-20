using BusinessLayer.Mapping;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Context;
using AutoMapper;
using FluentValidation.AspNetCore;
using RepositoryLayer.Interface;
using RepositoryLayer.Service;
using BusinessLayer.Interface;
using BusinessLayer.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using BusinessLayer.Helper;
using StackExchange.Redis;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using BusinessLayer.Consumer;

var builder = WebApplication.CreateBuilder(args);

Task.Run(() => UserRegisteredConsumers.Consume());
Task.Run(() => ContactAddedConsumers.Consume());


// ? Redis Connection Setup
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = builder.Configuration.GetValue<string>("Redis:ConnectionString");
    return ConnectionMultiplexer.Connect(configuration);
});

// ? Redis Caching & Session Storage
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379"; // Make sure Redis is running
    options.InstanceName = "AddressBook_";
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ? JWT Configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = "https://localhost:7023",
        ValidAudience = "https://localhost:7023",
        ClockSkew = TimeSpan.Zero
    };
});

// ? FluentValidation
builder.Services.AddControllers().AddFluentValidation(fv =>
{
    fv.RegisterValidatorsFromAssemblyContaining<AddressBookValidator>();
});

builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddAutoMapper(typeof(AddressBookMappingProfile));

// ? Dependency Injection for Business & Repository Layer
builder.Services.AddScoped<IAddressBookRL, AddressBookRL>();
builder.Services.AddScoped<IAddressBookBL, AddressBookBL>();
builder.Services.AddScoped<IUserBL, UserBL>();
builder.Services.AddScoped<IUserRL, UserRL>();
builder.Services.AddScoped<RedisCacheService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddSingleton<RabbitMQService>();

// ? Database Connection
var connectionString = builder.Configuration.GetConnectionString("SqlConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();


app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// ? Enable Session Middleware
app.UseSession();

app.MapControllers();
app.Run();
