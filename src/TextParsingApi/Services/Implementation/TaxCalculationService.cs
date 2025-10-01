using System.Globalization;
using System.Text.RegularExpressions;
using TextParsingApi.Models;
using TextParsingApi.Services;
using TextParsingApi.Validation;

namespace TextParsingApi.Services.Implementation;

/// <summary>
/// Service for tax calculations based on NZ GST model
/// </summary>
public class TaxCalculationService : ITaxCalculationService
{
    /// <summary>
    /// Calculates tax from total amount including tax (15% GST rate)
    /// Formula: Tax Amount = Total × (15 ÷ 115)
    /// </summary>
    public TaxCalculationDto CalculateFromTotalIncludingTax(decimal totalIncludingTax)
    {
        var taxAmount = totalIncludingTax * (ValidationRules.GST_RATE / ValidationRules.TAX_DIVISOR);
        var totalExcludingTax = totalIncludingTax - taxAmount;

        return new TaxCalculationDto
        {
            TotalIncludingTax = totalIncludingTax,
            TaxAmount = Math.Round(taxAmount, 2),
            TotalExcludingTax = Math.Round(totalExcludingTax, 2),
            TaxRate = ValidationRules.GST_RATE
        };
    }

    /// <summary>
    /// Extracts and validates total amount from parsed data
    /// Supports various number formats including commas
    /// </summary>
    public decimal? ExtractTotalAmount(List<XmlBlockDto> xmlBlocks, Dictionary<string, string> taggedFields)
    {
        string? totalValue = null;

        // First check tagged fields
        if (taggedFields.ContainsKey("total"))
        {
            totalValue = taggedFields["total"];
        }

        // Then check XML blocks
        if (string.IsNullOrEmpty(totalValue))
        {
            foreach (var block in xmlBlocks)
            {
                if (block.Fields.ContainsKey("total"))
                {
                    totalValue = block.Fields["total"];
                    break;
                }
            }
        }

        if (string.IsNullOrWhiteSpace(totalValue))
        {
            return null;
        }

        return ParseDecimalValue(totalValue);
    }

    /// <summary>
    /// Parses decimal value from string, handling various formats
    /// </summary>
    private static decimal? ParseDecimalValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        // Remove common formatting characters
        var cleanValue = Regex.Replace(value.Trim(), @"[,\s$]", "");

        // Try parsing as decimal
        if (decimal.TryParse(cleanValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        return null;
    }
}
