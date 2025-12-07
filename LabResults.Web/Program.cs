using Labresults.Infrastructure.Persistence;
using Labresults.Infrastructure.Readers;
using LabResults.Domain.Interfaces;
using LabResults.Web;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<LabResultsDbCotext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("LabResultsDb")));

//// Add services to the container.
builder.Services.AddScoped<IPatientReader, PatientReader>();
builder.Services.AddScoped<ITestResultReader, TestResultReader>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseMiddleware<ExceptionHandlerMiddleware>();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<LabResultsDbCotext>();
    Console.WriteLine("Applying database migrations...");
    context.Database.Migrate();
}

app.Run();
