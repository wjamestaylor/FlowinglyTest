using TextParsingApi.Models;
using TextParsingApi.Services;
using TextParsingApi.Services.Implementation;

namespace TextParsingApi.Tests;

public class TextParsingIntegrationTests
{
    private readonly ITextParsingService _textParsingService;
    private readonly IXmlParsingService _xmlParsingService;
    private readonly ITaxCalculationService _taxCalculationService;

    public TextParsingIntegrationTests()
    {
        _xmlParsingService = new XmlParsingService();
        _taxCalculationService = new TaxCalculationService();
        _textParsingService = new TextParsingService(_xmlParsingService, _taxCalculationService);
    }

    [Fact]
    public async Task ParseTextAsync_ValidChallengeSample_ShouldParseCorrectly()
    {
        // Arrange - Using the exact challenge example
        var content = @"
Hi Patricia,
Please create an expense claim for the below. Relevant details are marked up as requested…

<expense><cost_centre>DEV632</cost_centre><total>35,000</total><payment_method>personal card</payment_method></expense>

From: William Steele
Sent: Friday, 16 June 2022 10:32 AM
To: Maria Washington
Subject: test

Hi Maria,
Please create a reservation for 10 at the <vendor>Seaside Steakhouse</vendor> for our <description>development team's project end celebration</description> on <date>27 April 2022</date> at 7.30pm.

Regards,
William";

        // Act
        var result = await _textParsingService.ParseTextAsync(content);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);

        // Check XML blocks
        Assert.Single(result.XmlBlocks);
        var expenseBlock = result.XmlBlocks.First();
        Assert.Equal("expense", expenseBlock.TagName);
        Assert.Equal("DEV632", expenseBlock.Fields["cost_centre"]);
        Assert.Equal("35,000", expenseBlock.Fields["total"]);
        Assert.Equal("personal card", expenseBlock.Fields["payment_method"]);

        // Check tagged fields
        Assert.Equal("Seaside Steakhouse", result.TaggedFields["vendor"]);
        Assert.Equal("development team's project end celebration", result.TaggedFields["description"]);
        Assert.Equal("27 April 2022", result.TaggedFields["date"]);

        // Check tax calculations
        Assert.NotNull(result.Calculations);
        Assert.Equal(35000m, result.Calculations.TotalIncludingTax);
        Assert.Equal(Math.Round(35000m * (15m / 115m), 2), result.Calculations.TaxAmount);
        Assert.Equal(Math.Round(35000m - (35000m * (15m / 115m)), 2), result.Calculations.TotalExcludingTax);
    }

    [Fact]
    public async Task ParseTextAsync_MissingTotal_ShouldReturnError()
    {
        // Arrange
        var content = @"<expense><cost_centre>DEV632</cost_centre><payment_method>personal card</payment_method></expense>";

        // Act
        var result = await _textParsingService.ParseTextAsync(content);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Missing required <total> tag", result.Errors);
    }

    [Fact]
    public async Task ParseTextAsync_UnclosedTag_ShouldReturnError()
    {
        // Arrange
        var content = @"<expense><cost_centre>DEV632<total>1000</total></expense>";

        // Act
        var result = await _textParsingService.ParseTextAsync(content);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Unclosed tag detected", result.Errors);
    }

    [Fact]
    public async Task ParseTextAsync_MissingCostCentre_ShouldUseDefault()
    {
        // Arrange
        var content = @"<expense><total>1000</total></expense>";

        // Act
        var result = await _textParsingService.ParseTextAsync(content);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal("UNKNOWN", result.TaggedFields["cost_centre"]);
    }
}
