using Microsoft.EntityFrameworkCore;
using SemanticKernelLearning04.Data;
using SemanticKernelLearning04.Models;

namespace SemanticKernelLearning04.Services;

public class ConversationService
{
    private readonly ConversationDbContext _context;

    public ConversationService(ConversationDbContext context)
    {
        _context = context;
    }

    public async Task<string> CreateConversationAsync()
    {
        var conversation = new Conversation();
        
        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();
        
        return conversation.Id;
    }

    public async Task<Conversation?> GetConversationAsync(string conversationId)
    {
        return await _context.Conversations
            .Include(c => c.Messages.OrderBy(m => m.Timestamp))
            .FirstOrDefaultAsync(c => c.Id == conversationId);
    }

    public async Task AddMessageAsync(string conversationId, string content, string role)
    {
        var conversation = await _context.Conversations.FindAsync(conversationId);
        if (conversation == null)
        {
            throw new ArgumentException($"Conversation with ID {conversationId} not found.");
        }

        var message = new ConversationMessage
        {
            ConversationId = conversationId,
            Content = content,
            Role = role
        };

        conversation.UpdatedAt = DateTime.UtcNow;

        _context.ConversationMessages.Add(message);
        await _context.SaveChangesAsync();
    }

    public async Task<List<ConversationMessage>> GetMessagesAsync(string conversationId)
    {
        return await _context.ConversationMessages
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();
    }
}