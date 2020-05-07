using Datingapp.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Datingapp.API.Data
{
    public class DataContext : IdentityDbContext<User, Role, int, IdentityUserClaim<int>,
        UserRole,IdentityUserLogin<int>,IdentityRoleClaim<int>,IdentityUserToken<int>>
    {
        public DataContext(DbContextOptions<DataContext> options):base(options) {}

        public DbSet<Value> Values { get; set; }

        public DbSet<Photo> Photos { get; set; }

        public DbSet<Message> Messages { get; set; }

        public DbSet<Like> Likes { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserRole>()
                .HasKey(d => new { d.UserId, d.RoleId });

            builder.Entity<UserRole>()
                .HasOne(d => d.Role)
                .WithMany(d => d.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .IsRequired();


            builder.Entity<UserRole>()
                .HasOne(d => d.User)
                .WithMany(d => d.UserRoles)
                .HasForeignKey(d => d.UserId)
                .IsRequired();

            builder.Entity<Message>()
                .HasOne(d => d.Sender)
                .WithMany(u => u.MessageSent)
                .HasForeignKey(d=>d.SenderId)
                .OnDelete(DeleteBehavior.Restrict);


            builder.Entity<Message>()
                .HasOne(d => d.Recipient)
                .WithMany(u => u.MessageReceived)
                .HasForeignKey(d=>d.RecipientId)
                .OnDelete(DeleteBehavior.Restrict);


            builder.Entity<Like>()
                .HasKey(k => new { k.LikerId, k.LikeeId });

            builder.Entity<Like>()
                .HasOne(d => d.Liker)
                .WithMany(d => d.Likees)
                .HasForeignKey(d=>d.LikerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Like>()
                .HasOne(d => d.Likee)
                .WithMany(d => d.Likers)
                .HasForeignKey(d=>d.LikeeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Photo>().HasQueryFilter(d => d.IsAproved);
               
      }

       
    }
}
