using TextParsingApi.Models;

namespace TextParsingApi.Services;

/// <summary>
/// Core service interface for text parsing operations
/// </summary>
public interface ITextParsingService
{
    /// <summary>
    /// Parses text content to extract XML blocks and tagged fields
    /// </summary>
    Task<ParseResultDto> ParseTextAsync(string content);

    /// <summary>
    /// Validates text content according to business rules
    /// </summary>
    Task<ValidationResult> ValidateContentAsync(string content);
}

/// <summary>
/// Service interface for XML-specific parsing operations
/// </summary>
public interface IXmlParsingService
{
    /// <summary>
    /// Extracts complete XML blocks from text content
    /// </summary>
    Task<List<XmlBlockDto>> ExtractXmlBlocksAsync(string content);

    /// <summary>
    /// Extracts individual tagged fields from text content
    /// </summary>
    Task<Dictionary<string, string>> ExtractTaggedFieldsAsync(string content);

    /// <summary>
    /// Validates XML structure and tag completeness
    /// </summary>
    Task<ValidationResult> ValidateXmlStructureAsync(string content);
}

/// <summary>
/// Service interface for tax calculations
/// </summary>
public interface ITaxCalculationService
{
    /// <summary>
    /// Calculates tax from total amount including tax (NZ GST model)
    /// </summary>
    TaxCalculationDto CalculateFromTotalIncludingTax(decimal totalIncludingTax);

    /// <summary>
    /// Validates and extracts total amount from parsed data
    /// </summary>
    decimal? ExtractTotalAmount(List<XmlBlockDto> xmlBlocks, Dictionary<string, string> taggedFields);
}
