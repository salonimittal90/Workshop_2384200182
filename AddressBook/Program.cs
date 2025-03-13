using BusinessLayer.Mapping;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Context;
using AutoMapper;
using FluentValidation.AspNetCore;
using RepositoryLayer.Interface;
using RepositoryLayer.Service;
using BusinessLayer.Interface;
using BusinessLayer.Service;


var builder = WebApplication.CreateBuilder(args);

//  Register FluentValidation
builder.Services.AddControllers().AddFluentValidation(fv =>
{
    fv.RegisterValidatorsFromAssemblyContaining<AddressBookValidator>();
});

// AutoMapper ko register kar rahe hain
builder.Services.AddAutoMapper(typeof(AddressBookMappingProfile));

// Add services to the container.
//builder.Services.AddControllers();

// Dependency injection Register Business & Repository Layer Services
builder.Services.AddScoped<IAddressBookRL, AddressBookRL>();
builder.Services.AddScoped<IAddressBookBL, AddressBookBL>();

// Database Connection
var connectionString = builder.Configuration.GetConnectionString("SqlConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));




var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
