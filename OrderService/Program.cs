using Microsoft.EntityFrameworkCore;
using OrderService.DataAccess.Postgres;
using MediatR;

using FluentValidation;
using OrderService.WebApi.UseCases;
using OrderService.WebApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Контекст БД
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Валидатор и медиатор
builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderCommandValidator>();
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateOrderHandler).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


// // миграции
// using (var scope = app.Services.CreateScope())
// {
//     var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//     try
//     {
//         dbContext.Database.Migrate(); 
//         Console.WriteLine("Database migration completed successfully");
//     }
//     catch (Exception ex)
//     {
//         Console.WriteLine($"Database migration failed: {ex.Message}");
//         throw;
//     }
// }
// миграции
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        dbContext.Database.EnsureDeleted(); 
        // Этот метод создаст БД, если её нет, но не применит миграции
        dbContext.Database.EnsureCreated();
        Console.WriteLine("Database ensured created successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database creation failed: {ex.Message}");
        throw;
    }
}

app.Run();
