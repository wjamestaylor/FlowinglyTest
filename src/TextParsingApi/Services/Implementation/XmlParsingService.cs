using System.Text.RegularExpressions;
using System.Xml;
using TextParsingApi.Models;
using TextParsingApi.Services;
using TextParsingApi.Validation;

namespace TextParsingApi.Services.Implementation;

/// <summary>
/// Service for XML parsing and tag extraction operations
/// </summary>
public class XmlParsingService : IXmlParsingService
{
    private static readonly Regex XmlBlockRegex = new(@"<(\w+)>(.*?)</\1>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
    private static readonly Regex TaggedFieldRegex = new(@"<(\w+)>([^<]+)</\1>", RegexOptions.IgnoreCase);
    private static readonly Regex UnclosedTagRegex = new(@"<(\w+)>(?![^<]*</\1>)", RegexOptions.IgnoreCase);

    /// <summary>
    /// Extracts complete XML blocks from text content
    /// </summary>
    public async Task<List<XmlBlockDto>> ExtractXmlBlocksAsync(string content)
    {
        await Task.CompletedTask; // Async placeholder for future enhancement

        var xmlBlocks = new List<XmlBlockDto>();
        var matches = XmlBlockRegex.Matches(content);

        foreach (Match match in matches)
        {
            var tagName = match.Groups[1].Value;
            var innerContent = match.Groups[2].Value;
            var rawXml = match.Value;

            // Parse inner tags
            var fields = new Dictionary<string, string>();
            var innerMatches = TaggedFieldRegex.Matches(innerContent);

            foreach (Match innerMatch in innerMatches)
            {
                var fieldName = innerMatch.Groups[1].Value;
                var fieldValue = innerMatch.Groups[2].Value.Trim();
                fields[fieldName] = fieldValue;
            }

            xmlBlocks.Add(new XmlBlockDto
            {
                TagName = tagName,
                Fields = fields,
                RawXml = rawXml
            });
        }

        return xmlBlocks;
    }

    /// <summary>
    /// Extracts individual tagged fields from text content (excluding XML blocks)
    /// </summary>
    public async Task<Dictionary<string, string>> ExtractTaggedFieldsAsync(string content)
    {
        await Task.CompletedTask; // Async placeholder for future enhancement

        var taggedFields = new Dictionary<string, string>();

        // Remove XML blocks first to avoid duplicate extraction
        var contentWithoutXmlBlocks = XmlBlockRegex.Replace(content, "");

        // Extract individual tagged fields
        var matches = TaggedFieldRegex.Matches(contentWithoutXmlBlocks);

        foreach (Match match in matches)
        {
            var tagName = match.Groups[1].Value;
            var tagValue = match.Groups[2].Value.Trim();

            // Use the last occurrence if duplicate tags exist
            taggedFields[tagName] = tagValue;
        }

        return taggedFields;
    }

    /// <summary>
    /// Validates XML structure and checks for unclosed tags
    /// </summary>
    public async Task<ValidationResult> ValidateXmlStructureAsync(string content)
    {
        await Task.CompletedTask; // Async placeholder for future enhancement

        var result = new ValidationResult { IsValid = true };

        if (!ValidationRules.IsContentValid(content))
        {
            result.IsValid = false;
            result.Errors.Add(ValidationRules.EMPTY_CONTENT_ERROR);
            return result;
        }

        // Check for unclosed tags
        var unclosedTags = FindUnclosedTags(content);
        if (unclosedTags.Any())
        {
            result.IsValid = false;
            result.Errors.Add($"{ValidationRules.UNCLOSED_TAG_ERROR}: {string.Join(", ", unclosedTags)}");
        }

        // Validate XML structure for XML blocks
        var xmlBlockMatches = XmlBlockRegex.Matches(content);
        foreach (Match match in xmlBlockMatches)
        {
            if (!IsValidXmlStructure(match.Value))
            {
                result.IsValid = false;
                result.Errors.Add($"{ValidationRules.MALFORMED_XML_ERROR}: {match.Groups[1].Value}");
            }
        }

        return result;
    }

    /// <summary>
    /// Finds unclosed tags in the content
    /// </summary>
    private static List<string> FindUnclosedTags(string content)
    {
        var unclosedTags = new List<string>();
        var allOpenTags = new List<string>();
        var allCloseTags = new List<string>();

        // Find all opening tags
        var openTagRegex = new Regex(@"<(\w+)>", RegexOptions.IgnoreCase);
        var openMatches = openTagRegex.Matches(content);
        foreach (Match match in openMatches)
        {
            allOpenTags.Add(match.Groups[1].Value.ToLower());
        }

        // Find all closing tags
        var closeTagRegex = new Regex(@"</(\w+)>", RegexOptions.IgnoreCase);
        var closeMatches = closeTagRegex.Matches(content);
        foreach (Match match in closeMatches)
        {
            allCloseTags.Add(match.Groups[1].Value.ToLower());
        }

        // Check for unmatched opening tags
        var openTagGroups = allOpenTags.GroupBy(t => t).ToDictionary(g => g.Key, g => g.Count());
        var closeTagGroups = allCloseTags.GroupBy(t => t).ToDictionary(g => g.Key, g => g.Count());

        foreach (var openTag in openTagGroups)
        {
            var closeCount = closeTagGroups.GetValueOrDefault(openTag.Key, 0);
            if (openTag.Value > closeCount)
            {
                unclosedTags.Add(openTag.Key);
            }
        }

        return unclosedTags;
    }

    /// <summary>
    /// Validates XML structure using XmlDocument
    /// </summary>
    private static bool IsValidXmlStructure(string xmlContent)
    {
        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(xmlContent);
            return true;
        }
        catch (XmlException)
        {
            return false;
        }
    }
}
