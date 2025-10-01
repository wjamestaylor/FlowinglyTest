namespace TextParsingApi.Models;

/// <summary>
/// Request model for text parsing operations
/// </summary>
public class ParseRequestDto
{
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// Response model containing parsed data and validation results
/// </summary>
public class ParseResultDto
{
    public List<XmlBlockDto> XmlBlocks { get; set; } = new();
    public Dictionary<string, string> TaggedFields { get; set; } = new();
    public TaxCalculationDto? Calculations { get; set; }
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Represents an extracted XML block with its content
/// </summary>
public class XmlBlockDto
{
    public string TagName { get; set; } = string.Empty;
    public Dictionary<string, string> Fields { get; set; } = new();
    public string RawXml { get; set; } = string.Empty;
}

/// <summary>
/// Tax calculation results based on extracted total amount
/// </summary>
public class TaxCalculationDto
{
    public decimal TotalIncludingTax { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalExcludingTax { get; set; }
    public decimal TaxRate { get; set; }
}

/// <summary>
/// Validation result for text content
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public Dictionary<string, string> Warnings { get; set; } = new();
}

/// <summary>
/// API response wrapper for consistent error handling
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}
