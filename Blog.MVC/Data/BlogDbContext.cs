using BeaverX.Domain.Users;
using BeaverX.EntityFrameworkCore.Contexts;
using Blog.MVC.Models.Blog;
using Blog.MVC.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace Blog.MVC.Data;

public class BlogDbContext : BeaverXDbContext<BlogDbContext>
{
    public BlogDbContext(DbContextOptions<BlogDbContext> options, ICurrentUser currentUser)
        : base(options, currentUser)
    {
    }

    public DbSet<AppUser> Users { get; set; }

    public DbSet<Category> Categories { get; set; }

    public DbSet<Tag> Tags { get; set; }

    public DbSet<Article> Articles { get; set; }

    public DbSet<ArticleTag> ArticleTags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasIndex(x => x.UserName).IsUnique();
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.UserName).HasMaxLength(50);
            entity.Property(x => x.Email).HasMaxLength(100);
            entity.Property(x => x.PasswordHash).HasMaxLength(256);
            entity.Property(x => x.DisplayName).HasMaxLength(50);
            entity.Property(x => x.Avatar).HasMaxLength(500);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(x => x.Slug).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(50);
            entity.Property(x => x.Slug).HasMaxLength(80);
            entity.Property(x => x.Description).HasMaxLength(200);
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasIndex(x => x.Slug).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(30);
            entity.Property(x => x.Slug).HasMaxLength(50);
        });

        modelBuilder.Entity<Article>(entity =>
        {
            entity.HasIndex(x => x.Slug).IsUnique();
            entity.Property(x => x.Title).HasMaxLength(200);
            entity.Property(x => x.Slug).HasMaxLength(220);
            entity.Property(x => x.Summary).HasMaxLength(500);
            entity.Property(x => x.CoverImage).HasMaxLength(500);
            entity.Property(x => x.Content).HasColumnType("longtext");

            entity.HasOne(x => x.Category)
                .WithMany(x => x.Articles)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Author)
                .WithMany()
                .HasForeignKey(x => x.AuthorId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ArticleTag>(entity =>
        {
            entity.HasKey(x => new { x.ArticleId, x.TagId });

            entity.HasOne(x => x.Article)
                .WithMany(x => x.ArticleTags)
                .HasForeignKey(x => x.ArticleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Tag)
                .WithMany(x => x.ArticleTags)
                .HasForeignKey(x => x.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
