using TextParsingApi.Models;
using TextParsingApi.Services;
using TextParsingApi.Services.Implementation;
using TextParsingApi.Validation;

namespace TextParsingApi.Tests;

/// <summary>
/// Tests demonstrating the extensible validation system
/// </summary>
public class ExtensibleValidationTests
{
    [Fact]
    public async Task ParseTextAsync_WithCustomRequiredField_ShouldValidateCorrectly()
    {
        // Arrange - Create custom validation configuration
        var validationConfig = new ValidationConfiguration();

        // Add a new required field: currency
        validationConfig.FieldRules.Add(new FieldValidationRule
        {
            FieldName = "currency",
            IsRequired = true,
            CustomErrorMessage = "Currency is required for all transactions"
        });

        var validationRules = new ValidationRules(validationConfig);
        var xmlParsingService = new XmlParsingService(validationRules);
        var taxCalculationService = new TaxCalculationService();
        var textParsingService = new TextParsingService(xmlParsingService, taxCalculationService, validationRules);

        var contentWithoutCurrency = @"<expense><cost_centre>DEV632</cost_centre><total>100</total></expense>";

        // Act
        var result = await textParsingService.ParseTextAsync(contentWithoutCurrency);

        // Assert - Should fail because currency is now required
        Assert.False(result.IsValid);
        Assert.Contains("Currency is required for all transactions", result.Errors);
    }

    [Fact]
    public async Task ParseTextAsync_WithCustomDefaultValues_ShouldApplyCorrectly()
    {
        // Arrange - Create custom validation configuration with new defaults
        var validationConfig = new ValidationConfiguration();

        // Add new default values
        validationConfig.FieldRules.Add(new FieldValidationRule
        {
            FieldName = "currency",
            IsRequired = false,
            DefaultValue = "NZD"
        });

        validationConfig.FieldRules.Add(new FieldValidationRule
        {
            FieldName = "department",
            IsRequired = false,
            DefaultValue = "GENERAL"
        });

        var validationRules = new ValidationRules(validationConfig);
        var xmlParsingService = new XmlParsingService(validationRules);
        var taxCalculationService = new TaxCalculationService();
        var textParsingService = new TextParsingService(xmlParsingService, taxCalculationService, validationRules);

        var content = @"<expense><total>100</total></expense>";

        // Act
        var result = await textParsingService.ParseTextAsync(content);

        // Assert - Should apply all default values
        Assert.True(result.IsValid);
        Assert.Equal("UNKNOWN", result.TaggedFields["cost_centre"]); // Original default
        Assert.Equal("NZD", result.TaggedFields["currency"]); // New default
        Assert.Equal("GENERAL", result.TaggedFields["department"]); // New default
    }

    [Fact]
    public async Task ParseTextAsync_WithCustomValidator_ShouldValidateCorrectly()
    {
        // Arrange - Create custom validation with validator function
        var validationConfig = new ValidationConfiguration();

        // Add currency validation with allowed values
        validationConfig.FieldRules.Add(new FieldValidationRule
        {
            FieldName = "currency",
            IsRequired = true,
            CustomValidator = value => new[] { "NZD", "USD", "AUD", "EUR", "GBP" }.Contains(value.ToUpper()),
            CustomErrorMessage = "Currency must be one of: NZD, USD, AUD, EUR, GBP"
        });

        var validationRules = new ValidationRules(validationConfig);
        var xmlParsingService = new XmlParsingService(validationRules);
        var taxCalculationService = new TaxCalculationService();
        var textParsingService = new TextParsingService(xmlParsingService, taxCalculationService, validationRules);

        var contentWithInvalidCurrency = @"<expense><total>100</total><currency>XXX</currency></expense>";

        // Act
        var result = await textParsingService.ParseTextAsync(contentWithInvalidCurrency);

        // Assert - Should fail custom validation
        Assert.False(result.IsValid);
        Assert.Contains("Currency must be one of: NZD, USD, AUD, EUR, GBP", result.Errors);
    }

    [Fact]
    public void ValidationRules_AddAndRemoveRules_ShouldWorkCorrectly()
    {
        // Arrange
        var validationRules = new ValidationRules();

        // Act - Add a new rule
        validationRules.AddFieldRule(new FieldValidationRule
        {
            FieldName = "priority",
            IsRequired = true,
            CustomErrorMessage = "Priority is required"
        });

        // Assert - Rule should be added
        Assert.Contains("priority", validationRules.GetRequiredFieldNames());

        // Act - Remove the rule
        validationRules.RemoveFieldRule("priority");

        // Assert - Rule should be removed
        Assert.DoesNotContain("priority", validationRules.GetRequiredFieldNames());
    }

    [Fact]
    public void ValidationRules_GetDefaultValues_ShouldReturnConfiguredDefaults()
    {
        // Arrange - Create config with multiple defaults
        var config = new ValidationConfiguration();
        config.FieldRules.Add(new FieldValidationRule { FieldName = "currency", DefaultValue = "NZD" });
        config.FieldRules.Add(new FieldValidationRule { FieldName = "department", DefaultValue = "IT" });
        config.FieldRules.Add(new FieldValidationRule { FieldName = "approval_level", DefaultValue = "1" });

        var validationRules = new ValidationRules(config);

        // Act
        var defaults = validationRules.GetDefaultValues();

        // Assert
        Assert.Equal("NZD", defaults["currency"]);
        Assert.Equal("IT", defaults["department"]);
        Assert.Equal("1", defaults["approval_level"]);
        Assert.Equal("UNKNOWN", defaults["cost_centre"]); // Original default should still be there
    }
}
