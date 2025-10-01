# Azure AI Document Chat Platform - Technical Architecture

## System Overview

The Azure AI Document Chat Platform is a full-stack application demonstrating enterprise-grade document processing and conversational AI capabilities using Microsoft Azure AI services. The system employs a modern microservices-oriented architecture with clear separation of concerns between presentation, business logic, and data layers.

## Architecture Diagram

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   React SPA     │    │  ASP.NET Core   │    │   Azure AI      │
│   TypeScript    │◄──►│   Web API       │◄──►│   Services      │
│   Material-UI   │    │   .NET 8        │    │                 │
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         │                       │                       ▼
         │                       │            ┌─────────────────┐
         │                       │            │ Document Intel. │
         │                       │            │ OpenAI Service  │
         │                       │            │ AI Search       │
         │                       │            │ Blob Storage    │
         │                       │            └─────────────────┘
         │                       │
         │                       ▼
         │            ┌─────────────────┐
         │            │ SQL Server DB   │
         │            │ Entity Framework│
         │            └─────────────────┘
```

## Technology Stack

### Frontend Layer
- **Framework**: React 18 with TypeScript
- **UI Components**: Material-UI (MUI) v5
- **State Management**: React Hooks (useState, useEffect)
- **HTTP Client**: Axios with TypeScript interfaces
- **Build Tool**: Create React App with TypeScript template

### Backend Layer
- **Framework**: ASP.NET Core 8 Web API
- **Language**: C# 12
- **Architecture Pattern**: Clean Architecture with Service Layer
- **Dependency Injection**: Built-in ASP.NET Core DI Container
- **API Documentation**: Swagger/OpenAPI 3.0

### Data Layer
- **Primary Database**: Microsoft SQL Server
- **ORM**: Entity Framework Core 9
- **Migration Strategy**: Code-First with automatic migrations
- **Connection Management**: Connection pooling with retry policies

### Azure AI Services Integration
- **Document Intelligence**: Text extraction and document analysis
- **OpenAI Service**: GPT-4 for chat, text-embedding-3-small for vectors
- **AI Search**: Hybrid search (text + semantic vector search)
- **Blob Storage**: Scalable document storage with hierarchical organization

## Core Service Architecture

### Service Layer Design

```csharp
// Primary Services
├── DocumentProcessingService    // Document upload, OCR, indexing
├── ChatService                  // Conversation management, RAG implementation
├── PromptTemplateService       // Centralized prompt management
└── AdvancedChatService         // Extended AI capabilities (optional)

// Cross-Cutting Concerns
├── Configuration Management    // Azure service configuration
├── Error Handling             // Centralized exception management
├── Logging                    // Structured logging with correlation IDs
└── CORS Policy               // Cross-origin resource sharing
```

### Data Models and Relationships

```csharp
Document (1) ──→ (N) ChatSession (1) ──→ (N) ChatMessage

Document:
├── Id (PK, Guid)
├── FileName, ContentType, FileSizeBytes
├── Content (NVARCHAR(MAX))
├── VectorEmbedding (JSON serialized float[])
├── Metadata (PageCount, WordCount, UploadedAt)
└── BlobUrl (Azure Storage reference)

ChatSession:
├── Id (PK, Guid)
├── Title, CreatedAt, LastUpdatedAt
├── DocumentId (FK, nullable)
└── Navigation: Document, Messages

ChatMessage:
├── Id (PK, Guid)
├── Role (user|assistant), Content
├── Timestamp, SourceDocuments (JSON)
├── ChatSessionId (FK)
└── Navigation: ChatSession
```

## Processing Workflows

### Document Upload and Processing Pipeline

1. **File Reception**: Multipart form upload via REST API
2. **Storage**: Upload to Azure Blob Storage with unique naming
3. **Analysis**: Azure AI Document Intelligence extracts text and metadata
4. **Vectorization**: Generate embeddings using Azure OpenAI text-embedding-3-small
5. **Indexing**: Store in Azure AI Search with hybrid search configuration
6. **Persistence**: Save metadata and content to SQL Server database

### RAG (Retrieval-Augmented Generation) Workflow

1. **Query Processing**: User submits natural language question
2. **Context Retrieval**: 
   - Generate query embeddings
   - Perform hybrid search (text + vector similarity)
   - Retrieve top-K relevant document sections
3. **Prompt Construction**: Use PromptTemplateService to build context-aware prompts
4. **LLM Inference**: Submit to Azure OpenAI GPT-4 with conversation history
5. **Response Processing**: Parse response and track source attribution
6. **Persistence**: Store conversation in database with metadata

### Unified Prompt Management System

```csharp
// Centralized prompt templates with parameter substitution
Templates:
├── document_chat_system      // Primary RAG system prompt
├── document_summary          // Document summarization
├── document_comparison       // Multi-document analysis
├── search_enhancement        // Query optimization
├── context_validation        // Relevance assessment
└── key_phrase_extraction     // Term extraction

Features:
├── Runtime template updates
├── Parameter interpolation
├── Token estimation
├── A/B testing support
└── Configuration overrides
```

## API Architecture

### RESTful Endpoint Design

```
Documents API:
POST   /api/documents/upload        // File upload and processing
GET    /api/documents               // List all documents
GET    /api/documents/{id}          // Retrieve specific document
GET    /api/documents/search        // Semantic document search
DELETE /api/documents/{id}          // Remove document

Chat API:
POST   /api/chat/sessions           // Create new chat session
GET    /api/chat/sessions           // List user's chat sessions
GET    /api/chat/sessions/{id}      // Get session with message history
POST   /api/chat/sessions/{id}/messages  // Send message to session

Prompts API:
GET    /api/prompts/templates       // List available prompt templates
POST   /api/prompts/templates/{name}/render  // Render template with params
PUT    /api/prompts/templates/{name}         // Update template at runtime
POST   /api/prompts/test            // Test prompt configuration
```

### Error Handling and Validation

- **Global Exception Middleware**: Centralized error processing with correlation IDs
- **Model Validation**: Data annotation validation with custom error responses
- **Azure Service Failures**: Retry policies with exponential backoff
- **Rate Limiting**: Implementation considerations for Azure service quotas

## Security Architecture

### Authentication and Authorization
- **Ready for Integration**: RBAC placeholder for Azure AD B2C
- **API Keys**: Secure storage of Azure service credentials
- **CORS Configuration**: Restricted origins for production deployment

### Data Security
- **Encryption at Rest**: Azure Storage and SQL Server encryption
- **Encryption in Transit**: HTTPS/TLS 1.3 for all communications
- **Secret Management**: Azure Key Vault integration (configuration ready)
- **Data Isolation**: Tenant-aware data access patterns (prepared)

## Performance and Scalability

### Optimization Strategies
- **Connection Pooling**: Entity Framework connection management
- **Async Operations**: Comprehensive async/await implementation
- **Token Management**: Intelligent context truncation for LLM limits
- **Caching Strategy**: Ready for Redis integration for prompt caching

### Monitoring and Observability
- **Structured Logging**: Comprehensive logging with correlation tracking
- **Health Checks**: Database and Azure service availability monitoring
- **Performance Counters**: Response time and throughput tracking
- **Error Tracking**: Detailed exception logging and alerting

## Deployment Architecture

### Environment Configuration
- **Development**: LocalDB with Azure service integration
- **Staging**: Azure SQL Database with resource group isolation
- **Production**: Scaled Azure resources with high availability

### Infrastructure as Code
```yaml
Azure Resources Required:
├── Resource Group
├── Azure OpenAI Service (GPT-4 + text-embedding-3-small)
├── Azure AI Document Intelligence
├── Azure AI Search (with semantic search enabled)
├── Azure Storage Account (with blob containers)
├── Azure SQL Database
└── Azure App Service (for API hosting)
```

## Development and Maintenance

### Code Quality Standards
- **Clean Architecture**: Clear separation of concerns
- **SOLID Principles**: Dependency injection and interface abstraction
- **Error Handling**: Comprehensive exception management
- **Documentation**: Inline documentation and API specifications

### Testing Strategy
- **Unit Tests**: Service layer and business logic validation
- **Integration Tests**: Azure service interaction verification
- **API Tests**: Endpoint functionality and error handling
- **E2E Tests**: Complete workflow validation

This architecture provides a robust foundation for enterprise document processing and conversational AI applications, with clear scalability paths and maintainable code organization.