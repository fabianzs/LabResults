using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Labresults.Infrastructure.Persistence
{
    public class LabResultsDbCotextFactory : IDesignTimeDbContextFactory<LabResultsDbCotext>
    {
        public LabResultsDbCotext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<LabResultsDbCotext>();
            optionsBuilder.UseSqlite();

            return new LabResultsDbCotext(optionsBuilder.Options);
        }
    }
}
