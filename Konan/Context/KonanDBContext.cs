using Konan.Models;
using System.Data.Entity;

namespace Konan.Context
{
    public class KonanDBContext : DbContext
    {
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Friend> Friends { get; set; }
        public DbSet<Solution> Solutions { get; set; }
        public DbSet<FriendRequest> FriendRequests { get; set; }
        public DbSet<Chat> Messages { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>()
                   .HasMany(u => u.Solutions)
                   .WithRequired()
                   .HasForeignKey(h => h.Id_A);

            modelBuilder.Entity<Account>()
                   .HasMany(u => u.Posts)
                   .WithRequired()
                   .HasForeignKey(h => h.Id_A);

            modelBuilder.Entity<Account>()
                   .HasMany(u => u.Likes)
                   .WithRequired()
                   .HasForeignKey(h => h.Id_A);

            modelBuilder.Entity<Post>()
                   .HasMany(u => u.Likes)
                   .WithRequired()
                   .HasForeignKey(h => h.Id_P);

            modelBuilder.Entity<Post>()
                   .HasMany(u => u.Solutions)
                   .WithRequired()
                   .HasForeignKey(h => h.Id_P);

            modelBuilder.Entity<Friend>()
                .HasRequired(p => p.Account)
                .WithMany()
                .HasForeignKey(p => p.Id_A)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Friend>()
                        .HasRequired(p => p.A_Friend)
                        .WithMany()
                        .HasForeignKey(p => p.Id_F)
                        .WillCascadeOnDelete(false);

        }
    }
}