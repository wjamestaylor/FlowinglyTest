using TextParsingApi.Models;

namespace TextParsingApi.Validation;

/// <summary>
/// Business rules and validation constants for text parsing
/// </summary>
public static class ValidationRules
{
    // Error messages
    public const string MISSING_TOTAL_ERROR = "Missing required <total> tag";
    public const string UNCLOSED_TAG_ERROR = "Unclosed tag detected";
    public const string MALFORMED_XML_ERROR = "Malformed XML structure";
    public const string INVALID_TOTAL_FORMAT_ERROR = "Invalid total amount format";
    public const string EMPTY_CONTENT_ERROR = "Content cannot be empty";

    // Default values
    public const string DEFAULT_COST_CENTRE = "UNKNOWN";

    // Tax calculation constants (NZ GST)
    public const decimal GST_RATE = 15.0m;
    public const decimal TAX_DIVISOR = 115.0m; // 100 + GST_RATE

    // Required tags
    public static readonly string[] REQUIRED_TAGS = { "total" };

    // Tags with default values
    public static readonly Dictionary<string, string> DEFAULT_TAG_VALUES = new()
    {
        { "cost_centre", DEFAULT_COST_CENTRE }
    };

    /// <summary>
    /// Validates if content is not empty or whitespace
    /// </summary>
    public static bool IsContentValid(string content)
    {
        return !string.IsNullOrWhiteSpace(content);
    }

    /// <summary>
    /// Checks if all required tags are present
    /// </summary>
    public static List<string> GetMissingRequiredTags(List<XmlBlockDto> xmlBlocks, Dictionary<string, string> taggedFields)
    {
        var missingTags = new List<string>();
        var allFields = new Dictionary<string, string>(taggedFields);

        // Add XML block fields to the collection
        foreach (var block in xmlBlocks)
        {
            foreach (var field in block.Fields)
            {
                allFields.TryAdd(field.Key, field.Value);
            }
        }

        foreach (var requiredTag in REQUIRED_TAGS)
        {
            if (!allFields.ContainsKey(requiredTag) || string.IsNullOrWhiteSpace(allFields[requiredTag]))
            {
                missingTags.Add(requiredTag);
            }
        }

        return missingTags;
    }

    /// <summary>
    /// Applies default values for missing optional tags
    /// </summary>
    public static void ApplyDefaultValues(List<XmlBlockDto> xmlBlocks, Dictionary<string, string> taggedFields)
    {
        foreach (var defaultTag in DEFAULT_TAG_VALUES)
        {
            var tagExists = taggedFields.ContainsKey(defaultTag.Key) ||
                           xmlBlocks.Any(b => b.Fields.ContainsKey(defaultTag.Key));

            if (!tagExists)
            {
                taggedFields[defaultTag.Key] = defaultTag.Value;
            }
        }
    }
}
