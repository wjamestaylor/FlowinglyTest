using TextParsingApi.Models;

namespace TextParsingApi.Validation;

/// <summary>
/// Represents a field validation rule
/// </summary>
public class FieldValidationRule
{
    public string FieldName { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public string? DefaultValue { get; set; }
    public string? CustomErrorMessage { get; set; }
    public Func<string, bool>? CustomValidator { get; set; }
}

/// <summary>
/// Extensible validation configuration for text parsing business rules
/// </summary>
public class ValidationConfiguration
{
    /// <summary>
    /// Field validation rules - easily extensible for new requirements
    /// </summary>
    public List<FieldValidationRule> FieldRules { get; set; } = new()
    {
        // Required fields
        new FieldValidationRule
        {
            FieldName = "total",
            IsRequired = true,
            CustomErrorMessage = "Missing required <total> tag"
        },

        // Optional fields with defaults
        new FieldValidationRule
        {
            FieldName = "cost_centre",
            IsRequired = false,
            DefaultValue = "UNKNOWN"
        }

        // Future extensibility examples:
        // new FieldValidationRule
        // {
        //     FieldName = "currency",
        //     IsRequired = false,
        //     DefaultValue = "NZD"
        // },
        // new FieldValidationRule
        // {
        //     FieldName = "approval_required",
        //     IsRequired = false,
        //     DefaultValue = "true",
        //     CustomValidator = value => bool.TryParse(value, out _)
        // }
    };

    /// <summary>
    /// Global error messages
    /// </summary>
    public ValidationMessages Messages { get; set; } = new();
}

/// <summary>
/// Centralized error messages for easy maintenance
/// </summary>
public class ValidationMessages
{
    public string UnclosedTagError { get; set; } = "Unclosed tag detected";
    public string MalformedXmlError { get; set; } = "Malformed XML structure";
    public string InvalidTotalFormatError { get; set; } = "Invalid total amount format";
    public string EmptyContentError { get; set; } = "Content cannot be empty";
    public string MissingRequiredFieldError { get; set; } = "Missing required field: {0}";
}

/// <summary>
/// Business rules and validation constants for text parsing - now extensible!
/// </summary>
public class ValidationRules
{
    private readonly ValidationConfiguration _configuration;

    // Tax calculation constants (NZ GST) - these remain static as they're regulatory
    public const decimal GST_RATE = 15.0m;
    public const decimal TAX_DIVISOR = 115.0m; // 100 + GST_RATE

    public ValidationRules(ValidationConfiguration? configuration = null)
    {
        _configuration = configuration ?? new ValidationConfiguration();
    }

    /// <summary>
    /// Validates if content is not empty or whitespace
    /// </summary>
    public bool IsContentValid(string content)
    {
        return !string.IsNullOrWhiteSpace(content);
    }

    /// <summary>
    /// Gets all configured required field names
    /// </summary>
    public IEnumerable<string> GetRequiredFieldNames()
    {
        return _configuration.FieldRules
            .Where(rule => rule.IsRequired)
            .Select(rule => rule.FieldName);
    }

    /// <summary>
    /// Gets all configured default values
    /// </summary>
    public Dictionary<string, string> GetDefaultValues()
    {
        return _configuration.FieldRules
            .Where(rule => !string.IsNullOrEmpty(rule.DefaultValue))
            .ToDictionary(rule => rule.FieldName, rule => rule.DefaultValue!);
    }

    /// <summary>
    /// Checks if all required fields are present with extensible validation
    /// </summary>
    public List<string> GetMissingRequiredFields(List<XmlBlockDto> xmlBlocks, Dictionary<string, string> taggedFields)
    {
        var missingFields = new List<string>();
        var allFields = new Dictionary<string, string>(taggedFields, StringComparer.OrdinalIgnoreCase);

        // Add XML block fields to the collection
        foreach (var block in xmlBlocks)
        {
            foreach (var field in block.Fields)
            {
                allFields.TryAdd(field.Key, field.Value);
            }
        }

        // Check each required field rule
        foreach (var rule in _configuration.FieldRules.Where(r => r.IsRequired))
        {
            var fieldExists = allFields.ContainsKey(rule.FieldName) &&
                             !string.IsNullOrWhiteSpace(allFields[rule.FieldName]);

            if (!fieldExists)
            {
                var errorMessage = !string.IsNullOrEmpty(rule.CustomErrorMessage)
                    ? rule.CustomErrorMessage
                    : string.Format(_configuration.Messages.MissingRequiredFieldError, rule.FieldName);
                missingFields.Add(errorMessage);
            }
            // Apply custom validation if provided
            else if (rule.CustomValidator != null && !rule.CustomValidator(allFields[rule.FieldName]))
            {
                var errorMessage = rule.CustomErrorMessage ?? $"Invalid value for field: {rule.FieldName}";
                missingFields.Add(errorMessage);
            }
        }

        return missingFields;
    }

    /// <summary>
    /// Applies default values for missing optional fields - now configurable!
    /// </summary>
    public void ApplyDefaultValues(List<XmlBlockDto> xmlBlocks, Dictionary<string, string> taggedFields)
    {
        var allFields = new HashSet<string>(taggedFields.Keys, StringComparer.OrdinalIgnoreCase);

        // Add XML block field names to check for existence
        foreach (var block in xmlBlocks)
        {
            foreach (var fieldName in block.Fields.Keys)
            {
                allFields.Add(fieldName);
            }
        }

        // Apply defaults from configuration
        foreach (var rule in _configuration.FieldRules.Where(r => !string.IsNullOrEmpty(r.DefaultValue)))
        {
            if (!allFields.Contains(rule.FieldName))
            {
                taggedFields[rule.FieldName] = rule.DefaultValue!;
            }
        }
    }

    /// <summary>
    /// Gets validation messages configuration
    /// </summary>
    public ValidationMessages GetMessages() => _configuration.Messages;

    /// <summary>
    /// Adds a new field validation rule at runtime
    /// </summary>
    public void AddFieldRule(FieldValidationRule rule)
    {
        // Remove existing rule for the same field if it exists
        _configuration.FieldRules.RemoveAll(r => r.FieldName.Equals(rule.FieldName, StringComparison.OrdinalIgnoreCase));
        _configuration.FieldRules.Add(rule);
    }

    /// <summary>
    /// Removes a field validation rule
    /// </summary>
    public void RemoveFieldRule(string fieldName)
    {
        _configuration.FieldRules.RemoveAll(r => r.FieldName.Equals(fieldName, StringComparison.OrdinalIgnoreCase));
    }
}
