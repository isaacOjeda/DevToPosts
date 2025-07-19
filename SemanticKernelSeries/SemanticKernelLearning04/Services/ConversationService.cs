using Microsoft.EntityFrameworkCore;
using SemanticKernelLearning04.Data;
using SemanticKernelLearning04.Models;

namespace SemanticKernelLearning04.Services;

public class ConversationService(SkInvoiceDbContext context)
{
    public async Task<string> CreateConversationAsync()
    {
        var conversation = new Conversation();

        context.Conversations.Add(conversation);
        await context.SaveChangesAsync();

        return conversation.Id;
    }

    public async Task<Conversation?> GetConversationAsync(string conversationId)
    {
        return await context.Conversations
            .Include(c => c.Messages.OrderBy(m => m.Timestamp))
            .FirstOrDefaultAsync(c => c.Id == conversationId);
    }

    public async Task AddMessageAsync(string conversationId, string content, string role)
    {
        var conversation = await context.Conversations.FindAsync(conversationId);
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

        context.ConversationMessages.Add(message);
        await context.SaveChangesAsync();
    }

    public async Task<List<ConversationMessage>> GetMessagesAsync(string conversationId)
    {
        return await context.ConversationMessages
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();
    }
}