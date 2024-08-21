
using DocumentPlus.Data.Models;
using Microsoft.EntityFrameworkCore;


namespace DocumentPlus.Data.AppContextDB
{
    public class AppContextDB : DbContext
    {
        public AppContextDB(DbContextOptions<AppContextDB> options) : base(options) { }
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Group> Groups { get; set; } = null!;
        public DbSet<Document> Documents { get; set; } = null!;
        public DbSet<UserGroup> UserGroups { get; set; } = null!;
        public DbSet<DocumentAccess> DocumentAccesses { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Group>()
                .HasMany(g => g.Users)
                .WithMany(u => u.Groups)
                .UsingEntity<UserGroup>(
                   j => j
                    .HasOne(ug => ug.User)
                    .WithMany(t => t.UserGroups)
                    .HasForeignKey(ug => ug.User_ID),
                j => j
                    .HasOne(ug => ug.Group)
                    .WithMany(p => p.UserGroups)
                    .HasForeignKey(ug => ug.Group_ID));
            //j =>
            //{
            //    j.HasKey(t => new { t.Group_ID, t.User_ID });
            //    j.ToTable("UserGroups");
            //});
        }
    }
}
