using Labresults.Infrastructure.Persistence;
using LabResults;
using LabResults.DataLoader;
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
// We pass empty args here since we already consumed the file path input.
var host = Host.CreateDefaultBuilder(Array.Empty<string>())
    .ConfigureServices((context, services) =>
    {
        // Configure the DbContext (using SQLite In-Memory for this example)
        var connectionString = context.Configuration.GetConnectionString("LabResultsDb");

        services.AddDbContext<LabResultsDbCotext>(options =>
            options.UseSqlite(connectionString));

        // Add the Data Loader service
        services.AddTransient<IDataReader, LabDataReader>();
        services.AddTransient<IDataReader, LabDataReader>();
    })
    .Build();

// --- 3. Execute the Loading Logic ---
using (var scope = host.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;

    // Perform necessary setup for In-Memory DB (or apply migrations for a persistent DB)
    var context = serviceProvider.GetRequiredService<LabResultsDbCotext>();
    context.Database.EnsureCreated(); // Use Migrate() for persistent DBs

    // Get the loader and run the import
    var dataLoader = serviceProvider.GetRequiredService<LabDataReader>();

    Console.WriteLine($"Starting data load from: {filePath}");
    try
    {
        // Pass the user-provided path to the loading method
        await dataLoader.ReadDataFromFileAsync(filePath);
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
