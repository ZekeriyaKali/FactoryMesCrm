using FactoryMesCrm.Api.Middlewares;
using FactoryMesCrm.Application;
using FactoryMesCrm.Infrastructure;
using FactoryMesCrm.Persistence;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Core ve Infrastructure Katman Servislerinin Kaydý
builder.Services.AddApplicationServices();
builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);

// 2. Controller & API Explorer Ayarlarý
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 3. Swagger / OpenAPI Konfigürasyonu
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Factory MES & CRM API",
        Version = "v1",
        Description = "Production Grade MES ve CRM Bounded Context Entegrasyon API'si"
    });
});

var app = builder.Build();

// 4. Custom Global Exception Handling Middleware Kaydý
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 5. Swagger UI Entegrasyonu
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Factory MES/CRM API V1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();