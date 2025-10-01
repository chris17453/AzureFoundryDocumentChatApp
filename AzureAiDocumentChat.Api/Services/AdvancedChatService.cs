using Azure.AI.OpenAI;
using AzureAiDocumentChat.Api.Models;
using AzureAiDocumentChat.Api.Data;
using Microsoft.EntityFrameworkCore;
using OpenAI.Chat;
using System.Text.Json;

namespace AzureAiDocumentChat.Api.Services;

// Extended chat service with more advanced prompt capabilities
public class AdvancedChatService : ChatService
{
    private readonly PromptTemplateService _promptService;
    private readonly OpenAIClient _openAIClient;
    private readonly IConfiguration _configuration;

    public AdvancedChatService(
        IConfiguration configuration,
        ApplicationDbContext context,
        DocumentProcessingService documentService,
        PromptTemplateService promptService)
        : base(configuration, context, documentService, promptService)
    {
        _promptService = promptService;
        _configuration = configuration;
        
        var endpoint = configuration["AzureAI:OpenAI:Endpoint"]!;
        var apiKey = configuration["AzureAI:OpenAI:ApiKey"]!;
        _openAIClient = new OpenAIClient(new Uri(endpoint), new Azure.AzureKeyCredential(apiKey));
    }

    // Enhanced document summarization
    public async Task<string> SummarizeDocumentAsync(Guid documentId)
    {
        var document = await GetDocumentAsync(documentId);
        if (document == null) throw new ArgumentException("Document not found");

        var prompt = _promptService.GetPrompt("document_summary", new Dictionary<string, string>
        {
            { "DOCUMENT_NAME", document.FileName },
            { "DOCUMENT_CONTENT", document.Content }
        });

        var chatClient = _openAIClient.GetChatClient(_configuration["AzureAI:OpenAI:DeploymentName"]!);
        var response = await chatClient.CompleteChatAsync([
            ChatMessage.CreateSystemMessage("You are a document analysis expert."),
            ChatMessage.CreateUserMessage(prompt)
        ]);

        return response.Value.Content[0].Text;
    }

    // Compare multiple documents
    public async Task<string> CompareDocumentsAsync(List<Guid> documentIds, string comparisonFocus = "general analysis")
    {
        var documents = new List<Document>();
        foreach (var id in documentIds)
        {
            var doc = await GetDocumentAsync(id);
            if (doc != null) documents.Add(doc);
        }

        if (documents.Count < 2)
            throw new ArgumentException("At least 2 documents required for comparison");

        var prompt = _promptService.BuildComparisonPrompt(documents, comparisonFocus);

        var chatClient = _openAIClient.GetChatClient(_configuration["AzureAI:OpenAI:DeploymentName"]!);
        var response = await chatClient.CompleteChatAsync([
            ChatMessage.CreateSystemMessage("You are a document comparison specialist."),
            ChatMessage.CreateUserMessage(prompt)
        ]);

        return response.Value.Content[0].Text;
    }

    // Enhanced search with query improvement
    public async Task<string> EnhanceSearchQueryAsync(string originalQuery)
    {
        var prompt = _promptService.BuildSearchEnhancementPrompt(originalQuery);

        var chatClient = _openAIClient.GetChatClient(_configuration["AzureAI:OpenAI:DeploymentName"]!);
        var response = await chatClient.CompleteChatAsync([
            ChatMessage.CreateSystemMessage("You are a search optimization expert."),
            ChatMessage.CreateUserMessage(prompt)
        ]);

        return response.Value.Content[0].Text;
    }

    // Validate context relevance before responding
    public async Task<(int relevanceScore, string explanation)> ValidateContextRelevanceAsync(
        string userQuestion, string context)
    {
        var prompt = _promptService.BuildContextValidationPrompt(userQuestion, context);

        var chatClient = _openAIClient.GetChatClient(_configuration["AzureAI:OpenAI:DeploymentName"]!);
        var response = await chatClient.CompleteChatAsync([
            ChatMessage.CreateSystemMessage("You are a relevance assessment expert."),
            ChatMessage.CreateUserMessage(prompt)
        ]);

        var result = response.Value.Content[0].Text;
        
        // Parse the response to extract score and explanation
        var lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length > 0 && int.TryParse(lines[0].Trim(), out int score))
        {
            var explanation = lines.Length > 1 ? string.Join(" ", lines.Skip(1)) : "No explanation provided";
            return (score, explanation);
        }

        return (3, "Could not parse relevance assessment"); // Default moderate relevance
    }

    // Smart prompt selection based on query type
    public async Task<ChatMessage> SendSmartMessageAsync(Guid sessionId, string userMessage)
    {
        // First, analyze the type of query to select the best prompt strategy
        var queryType = await AnalyzeQueryTypeAsync(userMessage);
        
        // Then proceed with context-aware response
        return await SendMessageAsync(sessionId, userMessage);
    }

    private async Task<string> AnalyzeQueryTypeAsync(string query)
    {
        var analysisPrompt = $@"
Analyze this user query and classify it into one of these categories:
1. FACTUAL - asking for specific facts or data
2. SUMMARY - requesting a summary or overview
3. COMPARISON - comparing multiple things
4. ANALYSIS - requiring deeper analysis or interpretation
5. SEARCH - looking for documents or information
6. GENERAL - general conversation or unclear intent

Query: {query}

Respond with just the category name.";

        var chatClient = _openAIClient.GetChatClient(_configuration["AzureAI:OpenAI:DeploymentName"]!);
        var response = await chatClient.CompleteChatAsync([
            ChatMessage.CreateSystemMessage("You are a query classification expert."),
            ChatMessage.CreateUserMessage(analysisPrompt)
        ]);

        return response.Value.Content[0].Text.Trim();
    }

    // Helper method to get document
    private async Task<Document?> GetDocumentAsync(Guid documentId)
    {
        // This would need to be implemented with proper context access
        // For now, returning null - would need to refactor to access the context
        return null; // TODO: Implement proper document retrieval
    }
}