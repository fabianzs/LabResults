using LabResults.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Labresults.Infrastructure.Persistence
{
    public class LabResultsDbCotext : DbContext
    {
        public LabResultsDbCotext(DbContextOptions<LabResultsDbCotext> options) : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Sample> Samples { get; set; }
        public DbSet<TestResult> TestResults { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Patient>()
                .HasKey(p => p.Id);

            modelBuilder.Entity<Sample>()
                .HasKey(s => s.Id);

            modelBuilder.Entity<TestResult>()
               .HasKey(t => t.Id);

            modelBuilder.Entity<Sample>()
                .HasOne<Patient>()
                .WithMany(p => p.Samples)
                .HasForeignKey(s => s.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TestResult>()
                .HasOne<Sample>()
                .WithMany(s => s.TestResults)
                .HasForeignKey(t => t.SampleId)
                .OnDelete(DeleteBehavior.Restrict);

            //Indexes
            modelBuilder.Entity<Sample>()
               .HasIndex(s => s.Barcode)
               .IsUnique();

            modelBuilder.Entity<TestResult>()
                .HasIndex(t => t.SampleId);

            modelBuilder.Entity<Sample>()
                .HasIndex(s => s.PatientId);
        }
    }
}
