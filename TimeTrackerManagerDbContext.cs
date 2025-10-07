using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Model;

namespace TimeTracker.DAL
{
    public class TimeTrackerManagerDbContext : IdentityDbContext<IdentityUser>
    {
        public DbSet<Shift> Shifts { get; set; }
        public DbSet<Salary> Salaries { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<ScheduleAssignment> ScheduleAssignments { get; set; }

        public TimeTrackerManagerDbContext(DbContextOptions<TimeTrackerManagerDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<IdentityUserLogin<string>>().HasKey(l => new { l.LoginProvider, l.ProviderKey });
            modelBuilder.Entity<IdentityUserRole<string>>().HasKey(r => new { r.UserId, r.RoleId });
            modelBuilder.Entity<IdentityUserToken<string>>().HasKey(t => new { t.UserId, t.LoginProvider, t.Name });

            modelBuilder.Entity<Schedule>()
                .HasMany(s => s.Assignments)
                .WithOne(a => a.Schedule)
                .HasForeignKey(a => a.ScheduleId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Salary>()
                .HasOne<IdentityUser>()
                .WithMany()
                .HasForeignKey(s => s.IdentityUserId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<ScheduleAssignment>()
                .Property(a => a.Position)
                .HasConversion<string>();

            modelBuilder.Entity<Salary>()
                .HasIndex(s => s.IdentityUserId);
        }
        }
    }
