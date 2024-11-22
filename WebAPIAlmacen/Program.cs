using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using System.Text.Json.Serialization;
using WebAPIAlmacen.Filters;
using WebAPIAlmacen.Middlewares;
using WebAPIAlmacen.Models;
using WebAPIAlmacen.Services;
using WebAPIAlmacen.Services.Demo;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Esta configuración evita que las consultas multitabla con Include generen errores de ciclos indefinidos
builder.Services.AddControllers(options =>
{
    // Activamos el filtro de excepciones para todos los controladores
    options.Filters.Add(typeof(FiltroDeExcepcion));
}).AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Capturamos del app.settings la cadena de conexión a la base de datos
// Configuration.GetConnectionString va directamente a la propiedad ConnectionStrings y de ahí tomamos el valor de DefaultConnection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// Nuestros servicios resolverán dependencias de otras clases
// Registramos en el sistema de inyección de dependencias de la aplicación el ApplicationDbContext
builder.Services.AddDbContext<MiAlmacenContext>(options =>
            options.UseSqlServer(connectionString));

builder.Services.AddTransient<TransientService>();
builder.Services.AddScoped<ScopedService>();
builder.Services.AddSingleton<SingletonService>();
builder.Services.AddScoped<TestService>();
builder.Services.AddTransient<OperacionesService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<GestorArchivosService>();
builder.Services.AddSingleton<ContadorPeticionesService>();
builder.Services.AddTransient<HashService>();
builder.Services.AddTransient<TokenService>();
//builder.Services.AddHostedService<TareaProgramadaService>();


// Serilog
Serilog.Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger();
builder.Host.UseSerilog();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
         //builder.WithOrigins("https://apirequest.io/").AllowAnyMethod().AllowAnyHeader();
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

builder.Services.AddDataProtection();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
               .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
               {
                   ValidateIssuer = false,
                   ValidateAudience = false,
                   ValidateLifetime = true,
                   ValidateIssuerSigningKey = true,
                   IssuerSigningKey = new SymmetricSecurityKey(
                     Encoding.UTF8.GetBytes(builder.Configuration["ClaveJWT"]))
               });

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[]{}
                    }
                });

});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseMiddleware<LogMiddleware>();
app.UseStaticFiles();
app.UseAuthorization();

app.MapControllers();

app.Run();
