using Microsoft.AspNetCore.Mvc;
using AzureAiDocumentChat.Api.Services;

namespace AzureAiDocumentChat.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly ChatService _chatService;

    public ChatController(ChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpPost("sessions")]
    public async Task<IActionResult> CreateChatSession([FromBody] CreateChatSessionRequest request)
    {
        try
        {
            var session = await _chatService.CreateChatSessionAsync(request.Title, request.DocumentId);
            return Ok(session);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error creating chat session: {ex.Message}");
        }
    }

    [HttpGet("sessions")]
    public async Task<IActionResult> GetChatSessions()
    {
        try
        {
            var sessions = await _chatService.GetChatSessionsAsync();
            return Ok(sessions.Select(s => new
            {
                s.Id,
                s.Title,
                s.CreatedAt,
                s.LastUpdatedAt,
                Document = s.Document != null ? new { s.Document.Id, s.Document.FileName } : null
            }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error retrieving chat sessions: {ex.Message}");
        }
    }

    [HttpGet("sessions/{sessionId}")]
    public async Task<IActionResult> GetChatSession(Guid sessionId)
    {
        try
        {
            var session = await _chatService.GetChatSessionAsync(sessionId);
            if (session == null)
                return NotFound();

            return Ok(new
            {
                session.Id,
                session.Title,
                session.CreatedAt,
                session.LastUpdatedAt,
                Document = session.Document != null ? new { session.Document.Id, session.Document.FileName } : null,
                Messages = session.Messages.Select(m => new
                {
                    m.Id,
                    m.Role,
                    m.Content,
                    m.Timestamp,
                    m.SourceDocuments
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error retrieving chat session: {ex.Message}");
        }
    }

    [HttpPost("sessions/{sessionId}/messages")]
    public async Task<IActionResult> SendMessage(Guid sessionId, [FromBody] SendMessageRequest request)
    {
        try
        {
            var message = await _chatService.SendMessageAsync(sessionId, request.Content);
            return Ok(new
            {
                message.Id,
                message.Role,
                message.Content,
                message.Timestamp,
                message.SourceDocuments
            });
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error sending message: {ex.Message}");
        }
    }
}

public record CreateChatSessionRequest(string Title, Guid? DocumentId = null);
public record SendMessageRequest(string Content);