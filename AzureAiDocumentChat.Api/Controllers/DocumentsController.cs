using Microsoft.AspNetCore.Mvc;
using AzureAiDocumentChat.Api.Services;
using AzureAiDocumentChat.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace AzureAiDocumentChat.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly DocumentProcessingService _documentService;
    private readonly ApplicationDbContext _context;

    public DocumentsController(DocumentProcessingService documentService, ApplicationDbContext context)
    {
        _documentService = documentService;
        _context = context;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadDocument(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            var document = await _documentService.ProcessDocumentAsync(file);
            return Ok(document);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error processing document: {ex.Message}");
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetDocuments()
    {
        try
        {
            var documents = await _context.Documents
                .OrderByDescending(d => d.UploadedAt)
                .Select(d => new
                {
                    d.Id,
                    d.FileName,
                    d.UploadedAt,
                    d.ContentType,
                    d.FileSizeBytes,
                    d.PageCount,
                    d.WordCount
                })
                .ToListAsync();

            return Ok(documents);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error retrieving documents: {ex.Message}");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDocument(Guid id)
    {
        try
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
                return NotFound();

            return Ok(document);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error retrieving document: {ex.Message}");
        }
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchDocuments([FromQuery] string query, [FromQuery] int maxResults = 5)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Query is required");

            var documents = await _documentService.SearchDocumentsAsync(query, maxResults);
            return Ok(documents.Select(d => new
            {
                d.Id,
                d.FileName,
                d.UploadedAt,
                d.ContentType,
                d.FileSizeBytes,
                d.PageCount,
                d.WordCount,
                ContentPreview = d.Content.Length > 200 ? d.Content.Substring(0, 200) + "..." : d.Content
            }));
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error searching documents: {ex.Message}");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDocument(Guid id)
    {
        try
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
                return NotFound();

            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();

            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error deleting document: {ex.Message}");
        }
    }
}