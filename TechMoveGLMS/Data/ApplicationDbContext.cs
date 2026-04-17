using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using TechMoveGLMS.Models;

namespace TechMoveGLMS.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==========================================
            // 1. FLUENT API CONSTRAINTS 
            // ==========================================

            // Ensures decimals don't truncate or crash SQL Server
            modelBuilder.Entity<ServiceRequest>()
                .Property(s => s.OriginalCostUSD)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ServiceRequest>()
                .Property(s => s.ConvertedCostZAR)
                .HasPrecision(18, 2);

            // Enforce One-to-Many Relationships explicitly
            modelBuilder.Entity<Contract>()
                .HasOne(c => c.Client)
                .WithMany(cl => cl.Contracts)
                .HasForeignKey(c => c.ClientId)
                .OnDelete(DeleteBehavior.Restrict); // Prevents deleting a client if they have active contracts

            // ==========================================
            // 2. PRE-LOAD SEED DATA (For Testing)
            // ==========================================

            modelBuilder.Entity<Client>().HasData(
                new Client { ClientId = 1, Name = "Acme Global", ContactEmail = "billing@acme.com", Region = "North America" },
                new Client { ClientId = 2, Name = "SITA Logistics SA", ContactEmail = "admin@sita.co.za", Region = "Africa" }
            );

            modelBuilder.Entity<Contract>().HasData(
                // An ACTIVE contract to test successful Service Requests
                new Contract
                {
                    ContractId = 1,
                    ClientId = 1,
                    StartDate = new DateTime(2025, 1, 1),
                    EndDate = new DateTime(2027, 1, 1),
                    Status = ContractStatus.Active,
                    Level = ServiceLevel.Express
                },
                // An EXPIRED contract to test your Validation Logic (Should block requests)
                new Contract
                {
                    ContractId = 2,
                    ClientId = 2,
                    StartDate = new DateTime(2023, 1, 1),
                    EndDate = new DateTime(2024, 1, 1),
                    Status = ContractStatus.Expired,
                    Level = ServiceLevel.Standard
                }
            );

            modelBuilder.Entity<ServiceRequest>().HasData(
                new ServiceRequest
                {
                    RequestId = 1,
                    ContractId = 1,
                    Description = "Initial Freight Setup",
                    OriginalCostUSD = 1500.00m,
                    ConvertedCostZAR = 28500.00m,
                    Status = "Pending"
                }
            );
        }
    }
}