using System.ComponentModel.DataAnnotations;

namespace SemanticKernelLearning04.Models;

public class Conversation
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public virtual ICollection<ConversationMessage> Messages { get; set; } = new List<ConversationMessage>();
}