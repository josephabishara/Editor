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
        public DbSet<PublicationCustomerCategory> PublicationCustomerCategories { get; set; }
        public DbSet<ChannelCustomerCategory> ChannelCustomerCategories { get; set; }
        public DbSet<ClientCategories> ClientCategories { get; set; }
        public DbSet<News> News { get; set; }
        public DbSet<ClientNews> ClientNews { get; set; }
        // ═══════════════════════════════════════════════════════════════════════════
        // ApplicationDbContext — add DbSets
        // ═══════════════════════════════════════════════════════════════════════════
        public DbSet<NewsPaper> NewsPapers { get; set; }
        public DbSet<ClientNewsPaper> ClientNewsPapers { get; set; }
        public DbSet<GeneralArticle> GeneralArticles { get; set; }
        public DbSet<ClientArticle> ClientArticles { get; set; }
        public DbSet<GeneralVideos> GeneralVideos { get; set; }
        public DbSet<ClientVideo> ClientVideos { get; set; }


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

            // ── PublicationCustomerCategory ───────────────────────────────────────
            builder.Entity<PublicationCustomerCategory>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Client)
                      .WithMany(c => c.PublicationCategories)
                      .HasForeignKey(e => e.CustomerId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Publication)
                      .WithMany()
                      .HasForeignKey(e => e.PublicationId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ── ChannelCustomerCategory ───────────────────────────────────────────
            builder.Entity<ChannelCustomerCategory>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Client)
                      .WithMany(c => c.ChannelCategories)
                      .HasForeignKey(e => e.CustomerId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Channel)
                      .WithMany()
                      .HasForeignKey(e => e.ChannelId)
                      .OnDelete(DeleteBehavior.Cascade);
            });


            builder.Entity<ClientCategories>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.CategoryName)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(e => e.CategoryType)
                      .HasMaxLength(100);

                entity.Property(e => e.Status)
                      .HasMaxLength(50);

                // Self-referencing for parent/child categories
                entity.HasOne(e => e.Parent)
                      .WithMany(e => e.Children)
                      .HasForeignKey(e => e.ParentCategory)
                      .OnDelete(DeleteBehavior.Restrict)
                      .IsRequired(false);

                // Belongs to a Client
                entity.HasOne(e => e.Client)
                      .WithMany()
                      .HasForeignKey(e => e.ClientId)
                      .OnDelete(DeleteBehavior.Cascade);
            });


            builder.Entity<News>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
                entity.Property(e => e.SourceType).IsRequired().HasMaxLength(20);
                entity.Property(e => e.PRValue).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ADValue).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ArticleBranding).HasMaxLength(20);
                entity.Property(e => e.HeadlineBranding).HasMaxLength(20);
            });

            builder.Entity<ClientNews>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
                entity.Property(e => e.PRValue).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ADValue).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Height).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Width).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ArticleBranding).HasMaxLength(20);
                entity.Property(e => e.HeadlineBranding).HasMaxLength(20);

                // News master — no cascade on ClientNews side to avoid double-delete
                entity.HasOne(e => e.News)
                      .WithMany(n => n.ClientNewsList)
                      .HasForeignKey(e => e.NewsId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Client)
                      .WithMany()
                      .HasForeignKey(e => e.ClientId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Writer)
                      .WithMany()
                      .HasForeignKey(e => e.WriterId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
            builder.Entity<NewsPaper>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
                entity.Property(e => e.PRValue).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ADValue).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ArticleBranding).HasMaxLength(20);
                entity.Property(e => e.HeadlineBranding).HasMaxLength(20);
            });

            builder.Entity<ClientNewsPaper>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
                entity.Property(e => e.PRValue).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ADValue).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Height).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Width).HasColumnType("decimal(18,2)");

                // NewsPaperId is a reference (not FK) — no constraint
                entity.HasOne(e => e.Client)
                      .WithMany()
                      .HasForeignKey(e => e.ClientId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Writer)
                      .WithMany()
                      .HasForeignKey(e => e.WriterId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<GeneralArticle>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).HasMaxLength(500);
            });

            builder.Entity<ClientArticle>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).HasMaxLength(500);
                entity.Property(e => e.ADValue).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PRValue).HasColumnType("decimal(18,2)");

                // ArticleId is a reference (not FK) — no constraint
                entity.HasOne<Client>()
                      .WithMany()
                      .HasForeignKey(e => e.ClientId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<GeneralVideos>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Duration).HasColumnType("decimal(18,2)");
            });

            builder.Entity<ClientVideo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Duration).HasColumnType("decimal(18,2)");

                // VideoId is a reference (not FK) — no constraint
                entity.HasOne<Client>()
                      .WithMany()
                      .HasForeignKey(e => e.ClientId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<Channel>()
                      .WithMany()
                      .HasForeignKey(e => e.ChannelId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
