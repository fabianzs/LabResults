using LabResults.Entities;
using Microsoft.EntityFrameworkCore;

namespace LabResults.Persistence
{
    public class LabResultsDbCotext : DbContext
    {
        public LabResultsDbCotext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Sample> Samples { get; set; }
        public DbSet<TestResult> TestResults { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Patient>()
                .HasKey(p => p.Id);


            modelBuilder.Entity<Sample>()
                .HasKey(s => s.Id);

            modelBuilder.Entity<Sample>()
                .HasOne<Patient>()
                .WithMany()
                .HasForeignKey(s => s.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Sample>()
               .HasIndex(s => s.Barcode)
               .IsUnique();

            modelBuilder.Entity<Sample>()
                .HasIndex(s => s.PatientId);


            modelBuilder.Entity<TestResult>()
                .HasKey(t => t.Id);

            modelBuilder.Entity<TestResult>()
                .HasOne<Sample>()
                .WithMany()
                .HasForeignKey(t => t.SampleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TestResult>()
                .HasIndex(t => t.SampleId);
        }
    }
}
