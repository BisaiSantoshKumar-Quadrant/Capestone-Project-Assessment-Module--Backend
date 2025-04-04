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
        public DbSet<Category> Categories { get; set; }

        public DbSet<OTPRecord> OTPRecords { get; set; }

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

            modelBuilder.Entity<Category>()
                .Property(a => a.CategoryId)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<AssessmentDescription>()
                .Property(a => a.AssessmentID)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Question>()
                .Property(q => q.QuestionID)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<EmployeeResponse>()
                .Property(er => er.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<AssessmentScore>()
                .Property(ascore => ascore.Id)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<OTPRecord>()
               .Property(o => o.Id)
               .ValueGeneratedOnAdd();

            // Relationships

            // Employee - Category (Many-to-One)
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Category)
                .WithMany(r => r.Employees)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

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

            // EmployeeResponse - Assessment (Many-to-One) (Now Cascade)
            modelBuilder.Entity<EmployeeResponse>()
                .HasOne(er => er.Assessment)
                .WithMany(a => a.EmployeeResponses)
                .HasForeignKey(er => er.AssessmentID)
                .OnDelete(DeleteBehavior.NoAction);

            // AssessmentScore - Employee (Many-to-One)
            modelBuilder.Entity<AssessmentScore>()
                .HasOne(ascore => ascore.Employee)
                .WithMany(e => e.AssessmentScores)
                .HasForeignKey(ascore => ascore.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // AssessmentScore - Category (Many-to-One)
            modelBuilder.Entity<AssessmentScore>()
                .HasOne(ascore => ascore.Category)
                .WithMany(e => e.AssessmentScores)
                .HasForeignKey(ascore => ascore.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // AssessmentScore - Assessment (Many-to-One) (Now Cascade)
            modelBuilder.Entity<AssessmentScore>()
                .HasOne(ascore => ascore.Assessment)
                .WithMany(a => a.AssessmentScores)
                .HasForeignKey(ascore => ascore.AssessmentID)
                .OnDelete(DeleteBehavior.Cascade);

            // AssessmentDescription - Category (Many-to-One)
            modelBuilder.Entity<AssessmentDescription>()
                .HasOne(ascore => ascore.Category)
                .WithMany(a => a.AssessmentDescriptions)
                .HasForeignKey(ascore => ascore.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
