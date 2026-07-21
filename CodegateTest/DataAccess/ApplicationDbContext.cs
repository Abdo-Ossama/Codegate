using CodegateTest.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CodegateTest.DataAccess
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Course> Courses { get; set; }
        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<CourseInstructors> CourseInstructors { get; set; }
        public DbSet<Review> Reviews { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CourseInstructors>(entity =>
            {
                entity.HasOne(ci => ci.Course)
                    .WithMany(c => c.CourseInstructors)
                    .HasForeignKey(ci => ci.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ci => ci.Instructor)
                    .WithMany(i => i.CourseInstructors)
                    .HasForeignKey(ci => ci.InstructorId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}