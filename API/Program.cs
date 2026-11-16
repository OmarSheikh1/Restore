
using API.Abstractions.Data.Repositories;
using API.Abstractions.Services;
using API.Data;
using API.Data.Repositories;
using API.Entities;
using API.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// [ADDED|1.0.0] Configure database context with SQLite
builder.Services.AddDbContext<StoreContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// [ADDED|1.0.0] Register repository abstractions and implementations
// This enables dependency inversion - services depend on interfaces, not concrete implementations
builder.Services.AddScoped<IRepository<Product>, Repository<Product>>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// [ADDED|1.0.0] Register service abstractions and implementations
// Services orchestrate repository operations and implement business logic
builder.Services.AddScoped<IProductService, ProductService>();

// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    Console.WriteLine("✓ Swagger running at: https://localhost/swagger/index.html");
}
else
{
    Console.WriteLine("... Swagger running (not available in production)");
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");
app.UseAuthorization();

app.MapControllers();

DbInitializer.InitDb(app);

app.Run();
