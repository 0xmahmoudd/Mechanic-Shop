using System.Text.Json.Serialization;
using MechanicShop.Application.Interfaces;
using MechanicShop.Application.Services;
using MechanicShop.Domain.Enums;
using MechanicShop.Domain.Interfaces;
using MechanicShop.Infrastructure.Data;
using MechanicShop.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });


// Configure Database with Postgres enum mappings
var dataSourceBuilder = new Npgsql.NpgsqlDataSourceBuilder(
    builder.Configuration.GetConnectionString("DefaultConnection"));
dataSourceBuilder.MapEnum<WorkOrderState>("workorder_state", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<PaymentStatus>("payment_status", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<UserRole>("user_role", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<MechanicShopDbContext>(options =>
    options.UseNpgsql(dataSource, o => 
    {
        o.MapEnum<WorkOrderState>("workorder_state");
        o.MapEnum<PaymentStatus>("payment_status");
        o.MapEnum<UserRole>("user_role");
    }));


// Register Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register Repositories
builder.Services.AddScoped<IPartRepository, PartRepository>();
builder.Services.AddScoped<IRepairTaskRepository, RepairTaskRepository>();
// Add other repositories as needed
builder.Services.AddScoped<IWorkOrderRepository, WorkOrderRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();

// Register Application Services
builder.Services.AddScoped<IPartService, PartService>();
builder.Services.AddScoped<IRepairTaskService, RepairTaskService>();
// Add other services as needed
builder.Services.AddScoped<IWorkOrderService, WorkOrderService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Mechanic Shop API",
        Version = "v1",
        Description = "API for managing mechanic shop operations including parts, work orders, and invoices",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Mechanic Shop Support",
            Email = "support@mechanicshop.com"
        }
    });
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Mechanic Shop API v1");
        options.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();
