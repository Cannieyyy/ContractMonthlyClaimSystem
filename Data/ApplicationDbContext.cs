
using ContractMonthlyClaimSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace ContractMonthlyClaimSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Department> Departments { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<UserAccount> UserAccounts { get; set; }
        public DbSet<Claim> Claims { get; set; }
        public DbSet<SupportingDocument> SupportingDocuments { get; set; }

        public DbSet<Verification> Verifications { get; set; }

        public DbSet<Approval> Approvals { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Table mappings
            modelBuilder.Entity<Department>().ToTable("Departments");
            modelBuilder.Entity<Employee>().ToTable("Employees");
            modelBuilder.Entity<UserAccount>().ToTable("UserAccounts");
            modelBuilder.Entity<Claim>().ToTable("Claims");
            modelBuilder.Entity<SupportingDocument>().ToTable("SupportingDocuments");
            modelBuilder.Entity<Verification>().ToTable("Verification");
            modelBuilder.Entity<Approval>().ToTable("Approval");

            modelBuilder.Entity<Claim>()
    .HasOne(c => c.Employee)
    .WithMany(e => e.Claims)
    .HasForeignKey(c => c.EmployeeID)
    .OnDelete(DeleteBehavior.NoAction);


            // Seed departments
            modelBuilder.Entity<Department>().HasData(
                new Department { DepartmentID = 1, Name = "Diploma in Software Development", HourlyRate = 367.50m },
                new Department { DepartmentID = 2, Name = "Bachelor in Information Technology", HourlyRate = 422.00m },
                new Department { DepartmentID = 3, Name = "Higher Certificate In Networking", HourlyRate = 423.55m },
                new Department { DepartmentID = 4, Name = "Diploma in Web Development", HourlyRate = 369.42m },
                new Department { DepartmentID = 5, Name = "Human Resources", HourlyRate = 0 }
            );

            // Force all FKs to NoAction (use with caution)
            foreach (var fk in modelBuilder.Model.GetEntityTypes().SelectMany(t => t.GetForeignKeys()))
            {
                fk.DeleteBehavior = DeleteBehavior.NoAction;
            }
        }

    }
}
