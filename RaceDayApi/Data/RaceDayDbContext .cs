using Microsoft.EntityFrameworkCore;
using RaceDayApi.Models;

namespace RaceDayApi.Data
{
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Organiser> Organisers { get; set; } = null!;
        public DbSet<Participant> Participants { get; set; } = null!;
        public DbSet<Event> Events { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Enrolment> Enrolments { get; set; } = null!;
        public DbSet<Result> Results { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Unique Constraints
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<Enrolment>().HasIndex(e => new { e.ParticipantId, e.EventId, e.CategoryId }).IsUnique();

       
            modelBuilder.Entity<Enrolment>()
                .HasOne(e => e.Event)
                .WithMany(e => e.Enrolments)
                .HasForeignKey(e => e.EventId)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<Enrolment>()
                .HasOne(e => e.Category)
                .WithMany(e => e.Enrolments)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict); 
            modelBuilder.Entity<Organiser>()
                .HasOne(o => o.User)
                .WithOne(u => u.Organiser)
                .HasForeignKey<Organiser>(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<Participant>()
                .HasOne(p => p.User)
                .WithOne(u => u.Participant)
                .HasForeignKey<Participant>(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict); 

        }


    }
}
