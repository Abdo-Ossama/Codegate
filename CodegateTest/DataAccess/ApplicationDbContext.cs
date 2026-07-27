using CodegateTest.Models;
using CodegateTest.Models.CodegateTest.Models;
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
                entity.HasOne(e => e.Course)
                    .WithMany(e => e.CourseInstructors)
                    .HasForeignKey(e => e.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Instructor)
                    .WithMany(e => e.CourseInstructors)
                    .HasForeignKey(e => e.InstructorId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Review>()
       .HasOne(e => e.Student)
       .WithMany()
       .HasForeignKey(e => e.StudentId)
       .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasOne(e => e.Course)
                .WithMany()
                .HasForeignKey( e=> e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
            // Student Can Make feedback on course only ony time
            modelBuilder.Entity<Review>()
                .HasIndex(e => new
                {
                    e.StudentId,
                    e.CourseId
                })
                .IsUnique();
        }

    }
}