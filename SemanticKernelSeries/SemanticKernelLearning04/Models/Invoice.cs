using System.ComponentModel.DataAnnotations;

namespace SemanticKernelLearning04.Models;

public enum InvoiceStatus
{
    Draft = 0,
    Sent = 1,
    Paid = 2,
    Overdue = 3,
    Cancelled = 4
}

public class Invoice
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string InvoiceNumber { get; set; } = string.Empty;
    
    [Required]
    public int CustomerId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public decimal Amount { get; set; }
    
    [Required]
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    
    [Required]
    public DateTime IssueDate { get; set; } = DateTime.UtcNow;
    
    [Required]
    public DateTime DueDate { get; set; }
    
    public DateTime? PaidDate { get; set; }
    
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Customer Customer { get; set; } = null!;
    
    // Computed properties
    public bool IsOverdue => Status != InvoiceStatus.Paid && DueDate < DateTime.UtcNow;
    
    public int DaysOverdue => IsOverdue ? (DateTime.UtcNow - DueDate).Days : 0;
}