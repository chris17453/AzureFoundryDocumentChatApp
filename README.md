# Azure AI Document Chat Platform

Enterprise-grade document processing and conversational AI platform built with Azure AI services, demonstrating production-ready RAG (Retrieval-Augmented Generation) implementation with modern full-stack architecture.

## Executive Summary

This platform provides intelligent document analysis and natural language querying capabilities through Azure AI services integration. The system processes uploaded documents using OCR and machine learning, creates searchable vector embeddings, and enables conversational interaction with document content through an advanced RAG implementation.

## Core Capabilities

**Document Intelligence Pipeline**
- Multi-format document processing (PDF, Word, images, text)
- OCR text extraction with Azure AI Document Intelligence
- Automated metadata extraction and content indexing
- Vector embedding generation for semantic search

**Conversational AI Interface**
- Natural language document querying via GPT-4
- Context-aware responses with source attribution
- Conversation history and session management
- Unified prompt management system with runtime configurability

**Enterprise Architecture**
- React TypeScript frontend with Material-UI components
- ASP.NET Core 8 Web API with clean architecture patterns
- Entity Framework Core with SQL Server persistence
- Comprehensive error handling and logging infrastructure

## Technical Stack

**Frontend**: React 18, TypeScript, Material-UI, Axios
**Backend**: ASP.NET Core 8, C# 12, Entity Framework Core
**Database**: Microsoft SQL Server with Code-First migrations
**Azure Services**: OpenAI, Document Intelligence, AI Search, Blob Storage
**Architecture**: Clean Architecture, Dependency Injection, RESTful APIs

## Solution Architecture

```
├── AzureAiDocumentChat.Api/       # ASP.NET Core Web API
│   ├── Controllers/               # REST API endpoints
│   ├── Services/                  # Business logic and Azure integrations
│   ├── Models/                    # Entity models and DTOs
│   ├── Data/                      # Entity Framework DbContext
│   └── Configuration/             # Service configuration and DI setup
├── azure-ai-chat-frontend/        # React TypeScript SPA
│   ├── src/components/            # Reusable UI components
│   ├── src/api/                   # HTTP client and service interfaces
│   └── src/types/                 # TypeScript type definitions
├── ARCHITECTURE.md                # Detailed technical architecture
└── Documentation/                 # Additional technical documentation
```

## Setup Instructions

### Prerequisites

- .NET 8 SDK
- Node.js 18+
- MSSQL Server (LocalDB works fine)
- Azure subscription with:
  - Azure OpenAI Service
  - Azure AI Document Intelligence
  - Azure AI Search service
  - Azure Storage Account

### 1. Configure Azure Services

Update `AzureAiDocumentChat.Api/appsettings.json` with your Azure service endpoints and keys:

```json
{
  \"AzureAI\": {
    \"OpenAI\": {
      \"Endpoint\": \"https://YOUR_OPENAI_RESOURCE.openai.azure.com/\",
      \"ApiKey\": \"YOUR_OPENAI_API_KEY\",
      \"DeploymentName\": \"gpt-4\"
    },
    \"DocumentIntelligence\": {
      \"Endpoint\": \"https://YOUR_DOCUMENT_INTELLIGENCE_RESOURCE.cognitiveservices.azure.com/\",
      \"ApiKey\": \"YOUR_DOCUMENT_INTELLIGENCE_API_KEY\"
    },
    \"Search\": {
      \"Endpoint\": \"https://YOUR_SEARCH_SERVICE.search.windows.net\",
      \"ApiKey\": \"YOUR_SEARCH_API_KEY\",
      \"IndexName\": \"documents-index\"
    },
    \"Storage\": {
      \"ConnectionString\": \"DefaultEndpointsProtocol=https;AccountName=YOUR_STORAGE_ACCOUNT;AccountKey=YOUR_STORAGE_KEY;EndpointSuffix=core.windows.net\",
      \"ContainerName\": \"documents\"
    }
  }
}
```

### 2. Setup Azure AI Search Index

Create an index in Azure AI Search with the following schema:

```json
{
  \"name\": \"documents-index\",
  \"fields\": [
    {\"name\": \"id\", \"type\": \"Edm.String\", \"key\": true, \"searchable\": false},
    {\"name\": \"fileName\", \"type\": \"Edm.String\", \"searchable\": true, \"filterable\": true},
    {\"name\": \"content\", \"type\": \"Edm.String\", \"searchable\": true},
    {\"name\": \"uploadedAt\", \"type\": \"Edm.DateTimeOffset\", \"filterable\": true, \"sortable\": true},
    {\"name\": \"wordCount\", \"type\": \"Edm.Int32\", \"filterable\": true},
    {\"name\": \"pageCount\", \"type\": \"Edm.Int32\", \"filterable\": true},
    {\"name\": \"contentVector\", \"type\": \"Collection(Edm.Single)\", \"searchable\": true, \"dimensions\": 1536, \"vectorSearchProfile\": \"default\"}
  ]
}
```

### 3. Run the Backend

```bash
cd AzureAiDocumentChat.Api
dotnet restore
dotnet run
```

API will be available at `https://localhost:7067`

### 4. Run the Frontend

```bash
cd azure-ai-chat-frontend
npm install
npm start
```

Frontend will be available at `http://localhost:3000`

## Key Features Demonstrated

### 1. Document Processing Pipeline
- Upload documents via drag-and-drop or file picker
- Azure AI Document Intelligence extracts text and metadata
- Vector embeddings generated using Azure OpenAI
- Documents indexed in Azure AI Search for hybrid search

### 2. RAG (Retrieval-Augmented Generation)
- Semantic search finds relevant document sections
- GPT-4 answers questions using retrieved context
- Source attribution shows which documents informed the response
- Conversation history maintained per chat session

### 3. Unified Prompt Management System
- **Centralized Prompts**: All AI prompts managed in `PromptTemplateService`
- **Template System**: Parameterized prompts with variable substitution
- **Runtime Updates**: Modify prompts without redeployment
- **Prompt Testing**: API endpoints for testing and validating prompts
- **Token Estimation**: Built-in token counting for cost management
- **Multiple Strategies**: Different prompts for different query types

### 4. Modern Architecture Patterns
- Clean separation of concerns with services layer
- Entity Framework Code First with proper relationships
- React hooks with TypeScript for type safety
- Material-UI for professional UI components

## Technical Notes

- **Vector Search**: Uses text-embedding-3-small (1536 dimensions) for semantic similarity
- **Hybrid Search**: Combines traditional text search with vector similarity
- **Context Management**: Truncates long documents to stay within token limits
- **Error Handling**: Comprehensive error handling with user-friendly messages
- **CORS**: Configured for local development

## Unified Prompt System Usage

The new `PromptTemplateService` centralizes all AI prompts and provides several key benefits:

### Available Prompt Templates
- `document_chat_system` - Main system prompt for document Q&A
- `general_chat_system` - General purpose chat without specific documents
- `document_summary` - Document summarization prompts
- `document_comparison` - Multi-document comparison analysis
- `key_phrase_extraction` - Extract key terms and concepts
- `search_enhancement` - Improve user search queries
- `context_validation` - Validate context relevance

### Usage Examples

```csharp
// Basic prompt with parameters
var prompt = _promptService.GetPrompt("document_summary", new Dictionary<string, string>
{
    {"DOCUMENT_NAME", "Research Paper.pdf"},
    {"DOCUMENT_CONTENT", documentText}
});

// Build context-aware chat prompt
var chatPrompt = _promptService.BuildDocumentChatPrompt(relevantDocuments);

// Get prompt with token estimation
var (prompt, tokens) = _promptService.GetPromptWithTokenEstimate("document_chat_system", parameters);
```

### API Endpoints for Prompt Management

- `GET /api/prompts/templates` - List all available templates
- `POST /api/prompts/templates/{name}/render` - Render a template with parameters
- `PUT /api/prompts/templates/{name}` - Update a template at runtime
- `POST /api/prompts/test` - Test prompts with sample data

### Configuration

Add prompt settings to `appsettings.json`:

```json
{
  "PromptSettings": {
    "MaxContextLength": 3000,
    "EnablePromptCaching": true,
    "DefaultTokenLimit": 4000,
    "PromptTemplateOverrides": {
      "document_chat_system": "Your custom prompt here..."
    }
  }
}
```

## Potential Enhancements

- Add user authentication/authorization
- Implement document preview functionality
- Add support for more file types
- Batch document processing
- Real-time chat updates with SignalR
- Document versioning and change tracking
- Advanced search filters and faceting
- **Prompt A/B testing framework**
- **Prompt performance analytics**
- **Multi-language prompt templates**

## Production Deployment Considerations

**Infrastructure Requirements**
- Azure Resource Group with appropriate RBAC policies
- Azure OpenAI Service with GPT-4 and text-embedding-3-small deployments
- Azure AI Search service with semantic search capabilities enabled
- Azure Storage Account with blob container configuration
- Azure SQL Database with appropriate performance tier
- Application insights for monitoring and telemetry

**Security Implementation**
- Azure Key Vault integration for secrets management
- Managed Identity for service-to-service authentication
- Network security groups and private endpoints for Azure services
- HTTPS enforcement with proper certificate management

**Scalability and Performance**
- Horizontal scaling configuration for App Service
- Connection pooling and retry policies for database operations
- Caching strategy implementation for frequently accessed data
- Rate limiting and throttling for Azure AI service consumption

## Documentation

For detailed technical architecture, service integrations, and deployment specifications, see [ARCHITECTURE.md](./ARCHITECTURE.md).

## License

This project demonstrates Azure AI service integration patterns and is intended for educational and proof-of-concept purposes.