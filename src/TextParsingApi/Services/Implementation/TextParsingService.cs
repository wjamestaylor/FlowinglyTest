using TextParsingApi.Models;
using TextParsingApi.Services;
using TextParsingApi.Validation;

namespace TextParsingApi.Services.Implementation;

/// <summary>
/// Main orchestration service for text parsing operations
/// </summary>
public class TextParsingService : ITextParsingService
{
    private readonly IXmlParsingService _xmlParsingService;
    private readonly ITaxCalculationService _taxCalculationService;
    private readonly ValidationRules _validationRules;

    public TextParsingService(
        IXmlParsingService xmlParsingService,
        ITaxCalculationService taxCalculationService,
        ValidationRules validationRules)
    {
        _xmlParsingService = xmlParsingService;
        _taxCalculationService = taxCalculationService;
        _validationRules = validationRules;
    }

    /// <summary>
    /// Main parsing method that coordinates all parsing operations
    /// </summary>
    public async Task<ParseResultDto> ParseTextAsync(string content)
    {
        var result = new ParseResultDto();

        // Step 1: Validate content structure
        var validationResult = await ValidateContentAsync(content);
        if (!validationResult.IsValid)
        {
            result.IsValid = false;
            result.Errors = validationResult.Errors;
            return result;
        }

        try
        {
            // Step 2: Extract XML blocks and tagged fields
            result.XmlBlocks = await _xmlParsingService.ExtractXmlBlocksAsync(content);
            result.TaggedFields = await _xmlParsingService.ExtractTaggedFieldsAsync(content);

            // Step 3: Apply validation rules
            var missingRequiredFields = _validationRules.GetMissingRequiredFields(result.XmlBlocks, result.TaggedFields);
            if (missingRequiredFields.Any())
            {
                result.IsValid = false;
                result.Errors.AddRange(missingRequiredFields);
                return result;
            }

            // Step 4: Apply default values for optional fields
            _validationRules.ApplyDefaultValues(result.XmlBlocks, result.TaggedFields);

            // Step 5: Calculate tax if total amount is available
            var totalAmount = _taxCalculationService.ExtractTotalAmount(result.XmlBlocks, result.TaggedFields);
            if (totalAmount.HasValue)
            {
                result.Calculations = _taxCalculationService.CalculateFromTotalIncludingTax(totalAmount.Value);
            }
            else
            {
                result.IsValid = false;
                result.Errors.Add(_validationRules.GetMessages().InvalidTotalFormatError);
                return result;
            }

            result.IsValid = true;
        }
        catch (Exception ex)
        {
            result.IsValid = false;
            result.Errors.Add($"Parsing error: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// Validates content according to all business rules
    /// </summary>
    public async Task<ValidationResult> ValidateContentAsync(string content)
    {
        var result = new ValidationResult { IsValid = true };

        // Basic content validation
        if (!_validationRules.IsContentValid(content))
        {
            result.IsValid = false;
            result.Errors.Add(_validationRules.GetMessages().EmptyContentError);
            return result;
        }

        // XML structure validation
        var xmlValidation = await _xmlParsingService.ValidateXmlStructureAsync(content);
        if (!xmlValidation.IsValid)
        {
            result.IsValid = false;
            result.Errors.AddRange(xmlValidation.Errors);
            return result;
        }

        return result;
    }
}
