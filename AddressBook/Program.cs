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


var builder = WebApplication.CreateBuilder(args);

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

//  Register FluentValidation
builder.Services.AddControllers().AddFluentValidation(fv =>
{
    fv.RegisterValidatorsFromAssemblyContaining<AddressBookValidator>();
});

builder.Services.AddScoped<JwtTokenService>(); // ? JWT Service Register Karo

// AutoMapper ko register kar rahe hain
builder.Services.AddAutoMapper(typeof(AddressBookMappingProfile));

// Add services to the container.
//builder.Services.AddControllers();

// Dependency injection Register Business & Repository Layer Services
builder.Services.AddScoped<IAddressBookRL, AddressBookRL>();
builder.Services.AddScoped<IAddressBookBL, AddressBookBL>();

builder.Services.AddScoped<IUserBL, UserBL>();
builder.Services.AddScoped<IUserRL, UserRL>();


builder.Services.AddScoped<IEmailService, EmailService>();



// Database Connection
var connectionString = builder.Configuration.GetConnectionString("SqlConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));




var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
