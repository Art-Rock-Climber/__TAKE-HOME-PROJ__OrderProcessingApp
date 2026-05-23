using Microsoft.EntityFrameworkCore;
using PaymentService.DataAccess.Postgres;
using MediatR;

using FluentValidation;
using PaymentService.WebApi.UseCases;
using PaymentService.WebApi;

using Confluent.Kafka;

var builder = WebApplication.CreateBuilder(args);

// логи
builder.Logging.ClearProviders();
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
builder.Logging.AddConsole();

// Add services to the container.

builder.Services.AddControllers();


// бд
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Валидатор и медиатор
builder.Services.AddValidatorsFromAssemblyContaining<CreatePaymentCommandValidator>();
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreatePaymentHandler).Assembly);
    // cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});

// Регистрация продюсера
builder.Services.AddSingleton<IProducer<string, string>>(_ =>
{
    var config = new ProducerConfig { BootstrapServers = builder.Configuration["Kafka:BootstrapServers"] };
    return new ProducerBuilder<string, string>(config).Build();
});


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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


// миграции
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        dbContext.Database.Migrate(); 
        Console.WriteLine("Database migration completed successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Database migration failed: {ex.Message}");
        throw;
    }
}
// для тестов dotnet tool install --global dotnet-ef 
// dotnet ef migrations add InitialCreate
// dotnet ef database update
// using (var scope = app.Services.CreateScope())
// {
//     var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//     try
//     {
//         dbContext.Database.EnsureDeleted(); 
//         // Этот метод создаст БД, если её нет, но не применит миграции
//         dbContext.Database.EnsureCreated();
//         Console.WriteLine("Database ensured created successfully");
//     }
//     catch (Exception ex)
//     {
//         Console.WriteLine($"Database creation failed: {ex.Message}");
//         throw;
//     }
// }

app.Run();
