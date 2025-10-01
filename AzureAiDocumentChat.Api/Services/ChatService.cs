using Azure.AI.OpenAI;
using AzureAiDocumentChat.Api.Models;
using AzureAiDocumentChat.Api.Data;
using Microsoft.EntityFrameworkCore;
using OpenAI.Chat;
using System.Text.Json;

namespace AzureAiDocumentChat.Api.Services;

public class ChatService
{
    private readonly OpenAIClient _openAIClient;
    private readonly ApplicationDbContext _context;
    private readonly DocumentProcessingService _documentService;
    private readonly PromptTemplateService _promptService;
    private readonly IConfiguration _configuration;

    public ChatService(
        IConfiguration configuration,
        ApplicationDbContext context,
        DocumentProcessingService documentService,
        PromptTemplateService promptService)
    {
        _configuration = configuration;
        _context = context;
        _documentService = documentService;
        _promptService = promptService;

        var endpoint = configuration["AzureAI:OpenAI:Endpoint"]!;
        var apiKey = configuration["AzureAI:OpenAI:ApiKey"]!;
        _openAIClient = new OpenAIClient(new Uri(endpoint), new Azure.AzureKeyCredential(apiKey));
    }

    public async Task<ChatSession> CreateChatSessionAsync(string title, Guid? documentId = null)
    {
        var session = new ChatSession
        {
            Title = title,
            DocumentId = documentId
        };

        _context.ChatSessions.Add(session);
        await _context.SaveChangesAsync();

        return session;
    }

    public async Task<ChatMessage> SendMessageAsync(Guid sessionId, string userMessage)
    {
        var session = await _context.ChatSessions
            .Include(s => s.Document)
            .Include(s => s.Messages.OrderBy(m => m.Timestamp))
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session == null)
            throw new ArgumentException("Chat session not found");

        // Save user message
        var userMsg = new ChatMessage
        {
            ChatSessionId = sessionId,
            Role = "user",
            Content = userMessage
        };
        _context.ChatMessages.Add(userMsg);

        // Get relevant context using RAG
        var contextDocuments = await GetRelevantContextAsync(userMessage, session.DocumentId);
        
        // Use the unified prompt system instead of hardcoded prompts
        var systemPrompt = _promptService.BuildDocumentChatPrompt(contextDocuments);
        
        // Build conversation history
        var messages = new List<ChatMessage>
        {
            new() { Role = "system", Content = systemPrompt }
        };

        // Add conversation history (last 10 messages to stay within token limits)
        messages.AddRange(session.Messages.TakeLast(10));
        messages.Add(userMsg);

        // Get response from Azure OpenAI
        var chatClient = _openAIClient.GetChatClient(_configuration["AzureAI:OpenAI:DeploymentName"]!);
        var chatMessages = messages.Select(m => 
            m.Role == "user" ? ChatMessage.CreateUserMessage(m.Content) : 
            m.Role == "assistant" ? ChatMessage.CreateAssistantMessage(m.Content) :
            ChatMessage.CreateSystemMessage(m.Content)
        ).ToList();

        var response = await chatClient.CompleteChatAsync(chatMessages);
        var assistantResponse = response.Value.Content[0].Text;

        // Save assistant response
        var assistantMsg = new ChatMessage
        {
            ChatSessionId = sessionId,
            Role = "assistant",
            Content = assistantResponse,
            SourceDocuments = JsonSerializer.Serialize(contextDocuments.Select(d => new { d.Id, d.FileName }).ToList())
        };

        _context.ChatMessages.Add(assistantMsg);
        session.LastUpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();

        return assistantMsg;
    }

    private async Task<List<Document>> GetRelevantContextAsync(string query, Guid? specificDocumentId = null)
    {
        if (specificDocumentId.HasValue)
        {
            // If chatting with a specific document, use that document as context
            var document = await _context.Documents.FindAsync(specificDocumentId.Value);
            return document != null ? new List<Document> { document } : new List<Document>();
        }

        // Otherwise, search across all documents
        return await _documentService.SearchDocumentsAsync(query, maxResults: 3);
    }

    // Removed old prompt methods - now using unified PromptTemplateService
    // private string BuildContextFromDocuments(List<Document> documents) - DEPRECATED
    // private string BuildSystemPrompt(string context) - DEPRECATED
    
    // TODO: Consider adding prompt validation/testing methods here
    // Maybe add prompt A/B testing capabilities in the future

    public async Task<List<ChatSession>> GetChatSessionsAsync()
    {
        return await _context.ChatSessions
            .Include(s => s.Document)
            .OrderByDescending(s => s.LastUpdatedAt)
            .ToListAsync();
    }

    public async Task<ChatSession?> GetChatSessionAsync(Guid sessionId)
    {
        return await _context.ChatSessions
            .Include(s => s.Document)
            .Include(s => s.Messages.OrderBy(m => m.Timestamp))
            .FirstOrDefaultAsync(s => s.Id == sessionId);
    }
}