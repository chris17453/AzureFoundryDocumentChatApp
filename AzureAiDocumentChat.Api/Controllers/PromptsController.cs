using Microsoft.AspNetCore.Mvc;
using AzureAiDocumentChat.Api.Services;

namespace AzureAiDocumentChat.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PromptsController : ControllerBase
{
    private readonly PromptTemplateService _promptService;

    public PromptsController(PromptTemplateService promptService)
    {
        _promptService = promptService;
    }

    // Get all available prompt templates (useful for admin/debugging)
    [HttpGet("templates")]
    public IActionResult GetAvailableTemplates()
    {
        try
        {
            var templates = _promptService.GetAvailableTemplates();
            return Ok(templates);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error retrieving templates: {ex.Message}");
        }
    }

    // Get a specific prompt template with optional parameters
    [HttpPost("templates/{templateName}/render")]
    public IActionResult RenderTemplate(string templateName, [FromBody] Dictionary<string, string>? parameters = null)
    {
        try
        {
            var (prompt, estimatedTokens) = _promptService.GetPromptWithTokenEstimate(templateName, parameters);
            
            return Ok(new
            {
                TemplateName = templateName,
                RenderedPrompt = prompt,
                EstimatedTokens = estimatedTokens,
                Parameters = parameters ?? new Dictionary<string, string>()
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error rendering template: {ex.Message}");
        }
    }

    // Update a prompt template at runtime (for testing/experimentation)
    [HttpPut("templates/{templateName}")]
    public IActionResult UpdateTemplate(string templateName, [FromBody] UpdateTemplateRequest request)
    {
        try
        {
            _promptService.UpdatePromptTemplate(templateName, request.NewTemplate);
            return Ok(new { Message = $"Template '{templateName}' updated successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error updating template: {ex.Message}");
        }
    }

    // Test prompt with sample data (useful for prompt engineering)
    [HttpPost("test")]
    public IActionResult TestPrompt([FromBody] TestPromptRequest request)
    {
        try
        {
            var prompt = _promptService.GetPrompt(request.TemplateName, request.Parameters);
            
            // Could add more sophisticated testing here:
            // - Token counting
            // - Prompt validation
            // - Sample response generation
            
            return Ok(new
            {
                TemplateName = request.TemplateName,
                GeneratedPrompt = prompt,
                TokenCount = prompt.Length / 4, // rough estimate
                Status = "Success"
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"Prompt test failed: {ex.Message}");
        }
    }
}

// Request models for the API
public record UpdateTemplateRequest(string NewTemplate);

public record TestPromptRequest(
    string TemplateName, 
    Dictionary<string, string>? Parameters = null
);