using EF_1.Models;
using Microsoft.EntityFrameworkCore;

namespace EF_1.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Student> Students => Set<Student>();
        public DbSet<Teacher> Teachers => Set<Teacher>();
        public DbSet<Course> Courses => Set<Course>();
        public DbSet<Enrollment> Enrollments => Set<Enrollment>();

        // Один файл БД для консоли и окна.
        public static string DatabasePath { get; } = BuildDatabasePath();

        private static string BuildDatabasePath()
        {
            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "EF_1");

            Directory.CreateDirectory(folder);

            return Path.Combine(folder, "school.db");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={DatabasePath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasKey(s => s.Id);

                entity.Property(s => s.FullName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(s => s.Email)
                    .IsRequired()
                    .HasMaxLength(100);

                // Уникальный индекс: два студента не могут иметь один email.
                entity.HasIndex(s => s.Email)
                    .IsUnique()
                    .HasDatabaseName("IX_Students_Email");

                // Обычный индекс ускоряет поиск и сортировку по имени.
                entity.HasIndex(s => s.FullName)
                    .HasDatabaseName("IX_Students_FullName");

                // CHECK-ограничение на уровне БД.
                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_Students_Age", "Age BETWEEN 1 AND 120"));
            });

            modelBuilder.Entity<Teacher>(entity =>
            {
                entity.HasKey(t => t.Id);

                entity.Property(t => t.FullName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(t => t.Email)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasIndex(t => t.Email)
                    .IsUnique()
                    .HasDatabaseName("IX_Teachers_Email");
            });

            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasKey(c => c.Id);

                entity.Property(c => c.Title)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(c => c.DurationHours)
                    .IsRequired();

                entity.HasIndex(c => c.Title)
                    .IsUnique()
                    .HasDatabaseName("IX_Courses_Title");

                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_Courses_Duration", "DurationHours > 0"));
            });

            modelBuilder.Entity<Enrollment>(entity =>
            {
                entity.HasKey(e => new { e.StudentId, e.CourseId });

                // Токен конкуренции: EF добавит Version в WHERE при UPDATE и DELETE.
                entity.Property(e => e.Version)
                    .IsConcurrencyToken();

                entity.HasOne(e => e.Student)
                    .WithMany(s => s.Enrollments)
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Course)
                    .WithMany(c => c.Enrollments)
                    .HasForeignKey(e => e.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.ToTable(t => t.HasCheckConstraint(
                    "CK_Enrollments_Grade", "Grade IS NULL OR Grade BETWEEN 2 AND 5"));
            });

            modelBuilder.Entity<Student>()
                .HasOne(s => s.Teacher)
                .WithMany(t => t.Students)
                .HasForeignKey(s => s.TeacherId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}