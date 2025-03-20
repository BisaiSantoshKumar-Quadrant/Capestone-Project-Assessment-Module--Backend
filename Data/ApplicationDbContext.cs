using Microsoft.EntityFrameworkCore;
using QAssessment_project.Model;

namespace QAssessment_project.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSet properties for each entity
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<AssessmentDescription> Assessments { get; set; }
        public DbSet<AssessmentScore> AssessmentScores { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<EmployeeResponse> EmployeeResponses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Set identity seed for primary keys

            modelBuilder.Entity<Employee>()
     .Property(e => e.EmployeeId)
     .ValueGeneratedOnAdd();

            modelBuilder.Entity<Role>()
                .Property(r => r.RoleID)
                .ValueGeneratedOnAdd();

            // Set identity seed starting from 1
            modelBuilder.Entity<Employee>()
                .HasAnnotation("SqlServer:Identity", "1, 1");

            modelBuilder.Entity<Role>()
                .HasAnnotation("SqlServer:Identity", "1, 1");

             

            modelBuilder.Entity<Role>().HasData(
    new Role { RoleID = 1, RoleName = "User" },
    new Role { RoleID = 2, RoleName = "Admin" },
    new Role { RoleID = 3, RoleName = "Manager" },
    new Role { RoleID=4, RoleName="Guest"}
);
            modelBuilder.Entity<Employee>().HasData(
                new Employee
                {
                    EmployeeId = 1,
                    Username = "Sridevi",
                    Email = "sridevi@gmail.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("Sridevi@12"),
                    RoleID = 3

                });

            modelBuilder.Entity<AssessmentDescription>()
                .Property(a => a.AssessmentID)
                .UseIdentityColumn(1, 1);

            modelBuilder.Entity<Question>()
                .Property(q => q.QuestionID)
                .UseIdentityColumn(1, 1);

            modelBuilder.Entity<EmployeeResponse>()
                .Property(er => er.Id)
                .UseIdentityColumn(1, 1);

            modelBuilder.Entity<AssessmentScore>()
                .Property(ascore => ascore.Id)
                .UseIdentityColumn(1, 1);

            // Employee - Role (Many-to-One)
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Role)
                .WithMany(r => r.Employees)
                .HasForeignKey(e => e.RoleID)
                .OnDelete(DeleteBehavior.Restrict);

            // Question - Assessment (Many-to-One)
            modelBuilder.Entity<Question>()
                .HasOne(q => q.Assessment)
                .WithMany(a => a.Questions)
                .HasForeignKey(q => q.ExamID)
                .OnDelete(DeleteBehavior.Cascade);

            // EmployeeResponse - Employee (Many-to-One)
            modelBuilder.Entity<EmployeeResponse>()
                .HasOne(er => er.Employee)
                .WithMany(e => e.EmployeeResponses)
                .HasForeignKey(er => er.EmployeeID)
                .OnDelete(DeleteBehavior.Cascade);

            // EmployeeResponse - Question (Many-to-One)
            modelBuilder.Entity<EmployeeResponse>()
                .HasOne(er => er.Question)
                .WithMany(q => q.EmployeeResponses)
                .HasForeignKey(er => er.QuestionID)
                .OnDelete(DeleteBehavior.Restrict);

            // EmployeeResponse - Assessment (Many-to-One)
            modelBuilder.Entity<EmployeeResponse>()
                .HasOne(er => er.Assessment)
                .WithMany(a => a.EmployeeResponses)
                .HasForeignKey(er => er.AssessmentID)
                .OnDelete(DeleteBehavior.Restrict);

            // AssessmentScore - Employee (Many-to-One)
            modelBuilder.Entity<AssessmentScore>()
                .HasOne(ascore => ascore.Employee)
                .WithMany(e => e.AssessmentScores)
                .HasForeignKey(ascore => ascore.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // AssessmentScore - Assessment (Many-to-One)
            modelBuilder.Entity<AssessmentScore>()
                .HasOne(ascore => ascore.Assessment)
                .WithMany(a => a.AssessmentScores)
                .HasForeignKey(ascore => ascore.AssessmentID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
