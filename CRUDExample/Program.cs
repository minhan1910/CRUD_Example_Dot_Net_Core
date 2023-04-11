using Entities;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using Services;
using System;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Add services into IoC Container
builder.Services.AddScoped<ICountriesService, CountriesService>();
builder.Services.AddScoped<IPersonService, PersonService>();

builder.Services.AddDbContext<PersonsDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"));
});

var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();

app.Run();
