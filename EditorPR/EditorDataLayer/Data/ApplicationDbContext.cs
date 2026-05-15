using EditorEntitiesLayer.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EditorDataLayer.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
       : base(options)
        {
        }

        public DbSet<Websites> Websites { get; set; }
        public DbSet<Publication> Publications { get; set; }
        public DbSet<Channel> Channels { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Writer> Writers { get; set; }
        public DbSet<Assistant> Assistants { get; set; }
        public DbSet<WebsiteCustomerCategory> WebsiteCustomerCategories { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Rename default Identity tables
            builder.Entity<ApplicationUser>().ToTable("Users");

            builder.Entity<Websites>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.WebsiteName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.URL).IsRequired().HasMaxLength(500);
                entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
            });

            // ── Client ────────────────────────────────────────────────────
            builder.Entity<Client>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(256);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.Username).IsUnique();

                // One Client → many Assistants
                entity.HasMany(c => c.AssistantList)
                      .WithOne(a => a.Client)
                      .HasForeignKey(a => a.ClientId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ── Assistant ─────────────────────────────────────────────────
            builder.Entity<Assistant>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(256);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.Username).IsUnique();
            });
        }
    }
}
