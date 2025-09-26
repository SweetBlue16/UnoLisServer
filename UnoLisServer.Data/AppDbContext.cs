using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using UnoLisServer.Data.Entities;
using static System.Collections.Specialized.BitVector32;

namespace UnoLisServer.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() : base("AppDbContext") { }

        public DbSet<Player> Players { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<PlayerStatistics> PlayerStatistics { get; set; }
        public DbSet<Avatar> Avatars { get; set; }
        public DbSet<AvatarsUnlocked> AvatarsUnlocked { get; set; }
        public DbSet<Sanction> Sanctions { get; set; }
        public DbSet<SocialNetwork> SocialNetworks { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Achievement> Achievements { get; set; }
        public DbSet<FriendList> FriendLists { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Player ↔ Account (1:1)
            modelBuilder.Entity<Player>()
                .HasOptional(p => p.Account)
                .WithRequired(a => a.Player);

            // Player ↔ PlayerStatistics (1:1)
            modelBuilder.Entity<Player>()
                .HasOptional(p => p.Statistics)
                .WithRequired(s => s.Player);

            // FriendList (auto-referencia N:N)
            modelBuilder.Entity<FriendList>()
                .HasRequired(f => f.Player)
                .WithMany(p => p.Friends)
                .HasForeignKey(f => f.PlayerId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<FriendList>()
                .HasRequired(f => f.Friend)
                .WithMany()
                .HasForeignKey(f => f.FriendId)
                .WillCascadeOnDelete(false);

            base.OnModelCreating(modelBuilder);
        }

        static AppDbContext()
        {
            Database.SetInitializer<AppDbContext>(null);
        }
    }
}
