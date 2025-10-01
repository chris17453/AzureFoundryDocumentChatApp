using System.Text;
using AzureAiDocumentChat.Api.Models;

namespace AzureAiDocumentChat.Api.Services;

public class PromptTemplateService
{
    private readonly IConfiguration _configuration;

    // Store all prompt templates here - much easier to manage than scattered strings
    private readonly Dictionary<string, string> _promptTemplates;

    public PromptTemplateService(IConfiguration configuration)
    {
        _configuration = configuration;
        _promptTemplates = LoadPromptTemplates();
    }

    private Dictionary<string, string> LoadPromptTemplates()
    {
        var templates = new Dictionary<string, string>();

        // Main system prompt for document chat
        templates["document_chat_system"] = @"
You are an AI assistant specialized in document analysis and question answering. You help users understand and extract information from their uploaded documents.

## Your Capabilities:
- Analyze document content and structure
- Answer questions based on provided context
- Provide accurate citations and references
- Compare information across multiple documents
- Summarize key points and findings

## Instructions:
1. **Context-Based Responses**: Base your answers strictly on the provided document context
2. **Source Attribution**: Always cite which document(s) you're referencing
3. **Accuracy**: If information isn't in the context, clearly state that
4. **Clarity**: Provide clear, well-structured responses
5. **Completeness**: Address all parts of the user's question when possible

## Response Format:
- Start with a direct answer to the question
- Provide supporting details from the documents
- Include relevant quotes when helpful
- End with source attribution

## Context Information:
{CONTEXT}

Remember: Your knowledge is limited to the documents provided in the context above. Do not use external knowledge unless specifically asked to supplement the document information.";

        // Prompt for when no specific document is selected (general chat)
        templates["general_chat_system"] = @"
You are an AI assistant helping users with document-related queries. The user may ask general questions about their document collection or request searches across multiple documents.

## Available Documents:
{CONTEXT}

## Instructions:
- Help users understand their document collection
- Suggest relevant documents based on their questions
- Provide summaries across multiple documents when requested
- Guide users on how to better utilize their document library

If the user's question requires specific document content, suggest they start a focused chat session with the relevant document(s).";

        // Prompt for document summarization
        templates["document_summary"] = @"
Please provide a comprehensive summary of the following document:

Document: {DOCUMENT_NAME}
Content: {DOCUMENT_CONTENT}

Include:
1. Main topics covered
2. Key findings or conclusions
3. Important data points or statistics
4. Document structure and organization
5. Notable sections or chapters

Keep the summary concise but informative (3-5 paragraphs).";

        // Prompt for comparative analysis
        templates["document_comparison"] = @"
Compare and analyze the following documents:

{DOCUMENT_LIST}

Instructions:
1. Identify common themes and topics
2. Highlight key differences in approach or findings
3. Note any contradictions or disagreements
4. Summarize unique contributions from each document
5. Provide an overall synthesis

Focus on: {COMPARISON_FOCUS}";

        // Prompt for key phrase extraction
        templates["key_phrase_extraction"] = @"
Extract the most important key phrases and concepts from this document:

{DOCUMENT_CONTENT}

Provide:
1. Top 10 key phrases/terms
2. Main concepts and themes
3. Important people, places, or organizations mentioned
4. Technical terms or jargon (if applicable)
5. Action items or recommendations (if present)

Format as a structured list for easy reference.";

        // Search query enhancement prompt
        templates["search_enhancement"] = @"
The user asked: '{USER_QUERY}'

Based on this query, generate 3-5 alternative search terms or phrases that would help find relevant documents. Consider:
- Synonyms and related terms
- Technical vs. common language variations
- Broader and narrower concepts
- Different ways to express the same idea

Return only the search terms, one per line.";

        // Context validation prompt (to check if context is relevant)
        templates["context_validation"] = @"
User Question: {USER_QUESTION}
Available Context: {CONTEXT}

Rate the relevance of the provided context to the user's question on a scale of 1-5:
1 = Not relevant at all
2 = Slightly relevant
3 = Moderately relevant
4 = Highly relevant
5 = Perfectly relevant

Respond with just the number and a brief explanation.";

        return templates;
    }

    public string GetPrompt(string templateName, Dictionary<string, string>? parameters = null)
    {
        if (!_promptTemplates.ContainsKey(templateName))
        {
            // Fallback to a basic prompt if template not found
            return "You are a helpful AI assistant. Please answer the user's question based on the available information.";
        }

        var template = _promptTemplates[templateName];
        
        // Replace parameter placeholders if provided
        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                template = template.Replace($"{{{param.Key}}}", param.Value);
            }
        }

        return template;
    }

    // Specialized method for building document chat prompts
    public string BuildDocumentChatPrompt(List<Document> contextDocuments, string? specificFocus = null)
    {
        var contextBuilder = new StringBuilder();
        
        if (!contextDocuments.Any())
        {
            contextBuilder.AppendLine("No documents are currently available in the context.");
        }
        else
        {
            contextBuilder.AppendLine("## Available Documents:");
            
            foreach (var doc in contextDocuments)
            {
                contextBuilder.AppendLine($"\n### Document: {doc.FileName}");
                contextBuilder.AppendLine($"**Upload Date:** {doc.UploadedAt:yyyy-MM-dd}");
                contextBuilder.AppendLine($"**Pages:** {doc.PageCount} | **Words:** {doc.WordCount}");
                
                // Truncate content for token management - might need to make this configurable
                var maxLength = 3000; // TODO: make this configurable in appsettings
                var content = doc.Content.Length > maxLength 
                    ? doc.Content.Substring(0, maxLength) + "...[Content truncated]" 
                    : doc.Content;
                
                contextBuilder.AppendLine($"**Content:**\n{content}");
                contextBuilder.AppendLine(); // blank line between documents
            }
        }

        var parameters = new Dictionary<string, string>
        {
            { "CONTEXT", contextBuilder.ToString() }
        };

        if (!string.IsNullOrEmpty(specificFocus))
        {
            parameters["FOCUS"] = specificFocus;
        }

        return GetPrompt("document_chat_system", parameters);
    }

    // Method for building search enhancement prompts
    public string BuildSearchEnhancementPrompt(string userQuery)
    {
        var parameters = new Dictionary<string, string>
        {
            { "USER_QUERY", userQuery }
        };

        return GetPrompt("search_enhancement", parameters);
    }

    // Method for document comparison
    public string BuildComparisonPrompt(List<Document> documents, string comparisonFocus = "general analysis")
    {
        var docListBuilder = new StringBuilder();
        
        for (int i = 0; i < documents.Count; i++)
        {
            var doc = documents[i];
            docListBuilder.AppendLine($"\n## Document {i + 1}: {doc.FileName}");
            
            // For comparison, use more content but still manage tokens
            var maxLength = 2000;
            var content = doc.Content.Length > maxLength 
                ? doc.Content.Substring(0, maxLength) + "..." 
                : doc.Content;
            
            docListBuilder.AppendLine(content);
            docListBuilder.AppendLine();
        }

        var parameters = new Dictionary<string, string>
        {
            { "DOCUMENT_LIST", docListBuilder.ToString() },
            { "COMPARISON_FOCUS", comparisonFocus }
        };

        return GetPrompt("document_comparison", parameters);
    }

    // Method to validate if context is relevant to the user's question
    public string BuildContextValidationPrompt(string userQuestion, string context)
    {
        var parameters = new Dictionary<string, string>
        {
            { "USER_QUESTION", userQuestion },
            { "CONTEXT", context }
        };

        return GetPrompt("context_validation", parameters);
    }

    // Allow runtime prompt updates (useful for A/B testing or quick tweaks)
    public void UpdatePromptTemplate(string templateName, string newTemplate)
    {
        _promptTemplates[templateName] = newTemplate;
        // TODO: Consider persisting changes to database or config file
    }

    // Get all available template names (useful for admin interfaces)
    public IEnumerable<string> GetAvailableTemplates()
    {
        return _promptTemplates.Keys;
    }

    // Method to get prompt with token count estimation
    public (string prompt, int estimatedTokens) GetPromptWithTokenEstimate(string templateName, Dictionary<string, string>? parameters = null)
    {
        var prompt = GetPrompt(templateName, parameters);
        
        // Rough token estimation (1 token ≈ 4 characters for English text)
        var estimatedTokens = prompt.Length / 4;
        
        return (prompt, estimatedTokens);
    }
}