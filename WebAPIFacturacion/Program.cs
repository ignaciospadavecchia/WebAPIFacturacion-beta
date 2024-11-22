// Creates a new instance of the WebApplicationBuilder class with preconfigured defaults.
// It takes an array of strings (args) as a parameter, which represents the command-line arguments passed to the application.
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using WebAPIFacturacion.Filters;
using WebAPIFacturacion.Models;

var builder = WebApplication.CreateBuilder(args);

// Configure the application's services and settings
builder.Services.AddControllers(options =>
{
    // Filtro de excepciones para todos los controladores
    options.Filters.Add(typeof(FiltroDeExcepcion));
}).AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);


// Configure Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Conection to database 
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<MiFacturacionContext>(options => options.UseSqlServer(connectionString));

// Add services to the container

// Build the web application.
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
