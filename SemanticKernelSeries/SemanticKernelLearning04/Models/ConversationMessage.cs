using System.ComponentModel.DataAnnotations;

namespace SemanticKernelLearning04.Models;

public class ConversationMessage
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string ConversationId { get; set; } = string.Empty;
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    [Required]
    public string Role { get; set; } = string.Empty; // "user" or "assistant"
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public virtual Conversation Conversation { get; set; } = null!;
}