using ContosoUniversityHW.Models;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversityHW.Data
{
	public class UniversityContextHW:DbContext
	{
		public UniversityContextHW(DbContextOptions<UniversityContextHW> options) : base(options) { }

		public DbSet<Student>Students { get; set; }
		public DbSet<Enrollment>Enrollments { get; set; }
		public DbSet<Course>Courses { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Student>().ToTable("Students");
			modelBuilder.Entity<Enrollment>().ToTable("Enrollments");
			modelBuilder.Entity<Course>().ToTable("Courses");
		}
	}
}
