using Microsoft.EntityFrameworkCore;
using AzureAiDocumentChat.Api.Models;

namespace AzureAiDocumentChat.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Document> Documents { get; set; }
    public DbSet<ChatSession> ChatSessions { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Document entity
        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasIndex(e => e.FileName);
            entity.HasIndex(e => e.UploadedAt);
            entity.Property(e => e.Content).HasColumnType("NVARCHAR(MAX)");
            entity.Property(e => e.VectorEmbedding).HasColumnType("NVARCHAR(MAX)");
        });

        // Configure ChatSession entity
        modelBuilder.Entity<ChatSession>(entity =>
        {
            entity.HasIndex(e => e.CreatedAt);
            entity.HasOne(e => e.Document)
                  .WithMany(e => e.ChatSessions)
                  .HasForeignKey(e => e.DocumentId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure ChatMessage entity
        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasIndex(e => e.Timestamp);
            entity.Property(e => e.Content).HasColumnType("NVARCHAR(MAX)");
            entity.Property(e => e.SourceDocuments).HasColumnType("NVARCHAR(MAX)");
            entity.HasOne(e => e.ChatSession)
                  .WithMany(e => e.Messages)
                  .HasForeignKey(e => e.ChatSessionId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}