using System.ComponentModel.DataAnnotations;

namespace AzureAiDocumentChat.Api.Models;

public class Document
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    [Required]
    public string BlobUrl { get; set; } = string.Empty;
    
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    
    [MaxLength(50)]
    public string ContentType { get; set; } = string.Empty;
    
    public long FileSizeBytes { get; set; }
    
    public int PageCount { get; set; }
    
    public int WordCount { get; set; }
    
    // Vector embedding for semantic search
    public string? VectorEmbedding { get; set; }
    
    // Navigation property
    public virtual ICollection<ChatSession> ChatSessions { get; set; } = new List<ChatSession>();
}

public class ChatSession
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Foreign key
    public Guid? DocumentId { get; set; }
    public virtual Document? Document { get; set; }
    
    // Navigation property
    public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}

public class ChatMessage
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [Required]
    [MaxLength(10)]
    public string Role { get; set; } = string.Empty; // "user" or "assistant"
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    // Foreign key
    public Guid ChatSessionId { get; set; }
    public virtual ChatSession ChatSession { get; set; } = null!;
    
    // For RAG responses, store the source documents used
    public string? SourceDocuments { get; set; }
}