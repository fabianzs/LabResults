using Labresults.Infrastructure.Persistence;
using Labresults.Infrastructure.Readers;
using Labresults.Infrastructure.Writers;
using LabResults.DataLoader;
using LabResults.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Console.Write("Enter the full path to the data file (e.g., C:\\data\\lab_results.txt): ");
string filePath = Console.ReadLine();

if (string.IsNullOrWhiteSpace(filePath))
{
    Console.WriteLine("Error: File path cannot be empty. Exiting.");
return;
}

// Ensure the path is clean, especially if running on Unix-like systems
filePath = filePath.Trim();


// --- 2. Configure the Host and Services (DI) ---
var host = Host.CreateDefaultBuilder(Array.Empty<string>())
    .ConfigureServices((context, services) =>
    {
        var connectionString = context.Configuration.GetConnectionString("LabResultsDb");
        services.AddDbContext<LabResultsDbCotext>(options =>
            options.UseSqlite(connectionString));

        services.Configure<LabFileSettings>(context.Configuration.GetSection("LabFileSettings"));

        // Add services
        services.AddTransient<IDataReader, LabDataReader>();
        services.AddTransient<IDataWriter, LabDataWriter>();
    })
    .Build();

// --- 3. Execute Logic ---
using (var scope = host.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;

    var context = serviceProvider.GetRequiredService<LabResultsDbCotext>();
    Console.WriteLine("Applying database migrations...");
    context.Database.Migrate();

    var dataReader = serviceProvider.GetRequiredService<IDataReader>();
    var dataWriter = serviceProvider.GetRequiredService<IDataWriter>();

    Console.WriteLine($"Starting data load from: {filePath}");
    try
    {
        // Pass the user-provided path to the loading method
        var results = await dataReader.ReadDataFromFileAsync(filePath);
        await dataWriter.ProcessAndSaveDataAsync(results!.ToList());
        Console.WriteLine("✅ Data loading complete! Changes saved to the database.");
    }
    catch (FileNotFoundException)
    {
        Console.WriteLine($"❌ Error: File not found at the specified path: {filePath}");
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"❌ Data Format Error: {ex.Message}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ An unexpected error occurred: {ex.Message}");
    }
}

// Keep the console window open after completion for the user to read the output
Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();
