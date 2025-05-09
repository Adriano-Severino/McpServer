using Livros.Repository;
using Livros.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.  
builder.Services.AddMemoryCache();
builder.Services.AddDbContext<DefaultContext>((serviceProvider, options) =>
{
    var memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
    options.UseInMemoryDatabase("LivrosDb")
           .UseMemoryCache(memoryCache);
});

builder.Services.AddTransient<ILivroServices, LivroServices>();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Livros API",
        Version = "v1"
    });
});

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi  
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.  
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SLRpg API v1"));
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
