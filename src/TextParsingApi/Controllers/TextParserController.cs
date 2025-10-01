using Microsoft.AspNetCore.Mvc;
using TextParsingApi.Models;
using TextParsingApi.Services;

namespace TextParsingApi.Controllers;

/// <summary>
/// API controller for text parsing operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TextParserController : ControllerBase
{
    private readonly ITextParsingService _textParsingService;
    private readonly ILogger<TextParserController> _logger;

    public TextParserController(
        ITextParsingService textParsingService,
        ILogger<TextParserController> logger)
    {
        _textParsingService = textParsingService;
        _logger = logger;
    }

    /// <summary>
    /// Parses text content to extract XML blocks and tagged fields
    /// </summary>
    /// <param name="request">The parse request containing text content</param>
    /// <returns>Parsed data with XML blocks, tagged fields, and tax calculations</returns>
    [HttpPost("parse")]
    public async Task<ActionResult<ApiResponse<ParseResultDto>>> ParseText([FromBody] ParseRequestDto request)
    {
        try
        {
            _logger.LogInformation("Parsing text content of length: {Length}", request?.Content?.Length ?? 0);

            if (request == null || string.IsNullOrWhiteSpace(request.Content))
            {
                return BadRequest(new ApiResponse<ParseResultDto>
                {
                    Success = false,
                    Errors = new List<string> { "Content is required" },
                    Message = "Invalid request"
                });
            }

            var result = await _textParsingService.ParseTextAsync(request.Content);

            if (result.IsValid)
            {
                _logger.LogInformation("Successfully parsed text. Found {XmlCount} XML blocks and {TagCount} tagged fields",
                    result.XmlBlocks.Count, result.TaggedFields.Count);

                return Ok(new ApiResponse<ParseResultDto>
                {
                    Success = true,
                    Data = result,
                    Message = "Text parsed successfully"
                });
            }
            else
            {
                _logger.LogWarning("Text parsing failed with errors: {Errors}", string.Join(", ", result.Errors));

                return BadRequest(new ApiResponse<ParseResultDto>
                {
                    Success = false,
                    Data = result,
                    Errors = result.Errors,
                    Message = "Text parsing failed validation"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during text parsing");

            return StatusCode(500, new ApiResponse<ParseResultDto>
            {
                Success = false,
                Errors = new List<string> { "An unexpected error occurred" },
                Message = "Internal server error"
            });
        }
    }

    /// <summary>
    /// Validates text content without performing full parsing
    /// </summary>
    /// <param name="request">The validation request containing text content</param>
    /// <returns>Validation result indicating if content is valid</returns>
    [HttpPost("validate")]
    public async Task<ActionResult<ApiResponse<ValidationResult>>> ValidateText([FromBody] ParseRequestDto request)
    {
        try
        {
            _logger.LogInformation("Validating text content of length: {Length}", request?.Content?.Length ?? 0);

            if (request == null || string.IsNullOrWhiteSpace(request.Content))
            {
                return BadRequest(new ApiResponse<ValidationResult>
                {
                    Success = false,
                    Errors = new List<string> { "Content is required" },
                    Message = "Invalid request"
                });
            }

            var result = await _textParsingService.ValidateContentAsync(request.Content);

            _logger.LogInformation("Validation completed. Valid: {IsValid}", result.IsValid);

            return Ok(new ApiResponse<ValidationResult>
            {
                Success = true,
                Data = result,
                Message = result.IsValid ? "Content is valid" : "Content validation failed"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during text validation");

            return StatusCode(500, new ApiResponse<ValidationResult>
            {
                Success = false,
                Errors = new List<string> { "An unexpected error occurred" },
                Message = "Internal server error"
            });
        }
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow });
    }
}
