using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using TextParsingApi.Models;
using TextParsingApi.Services;
using TextParsingApi.Validation;

namespace TextParsingApi.Services.Implementation;

/// <summary>
/// Service for XML parsing and tag extraction operations using System.Xml.Linq
/// </summary>
public class XmlParsingService : IXmlParsingService
{
    private static readonly Regex TaggedFieldRegex = new(@"<(\w+)>([^<]+)</\1>", RegexOptions.IgnoreCase);
    private static readonly Regex XmlBlockFinderRegex = new(@"<(\w+)>.*?</\1>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
    private readonly ValidationRules _validationRules;

    public XmlParsingService(ValidationRules validationRules)
    {
        _validationRules = validationRules;
    }

    /// <summary>
    /// Extracts complete XML blocks from text content using XDocument.
    /// Only multi-element structures are considered XML blocks; single tags are tagged fields.
    /// </summary>
    public async Task<List<XmlBlockDto>> ExtractXmlBlocksAsync(string content)
    {
        await Task.CompletedTask; // Async placeholder for future enhancement

        var xmlBlocks = new List<XmlBlockDto>();
        var xmlBlockMatches = XmlBlockFinderRegex.Matches(content);

        foreach (Match match in xmlBlockMatches)
        {
            try
            {
                // Parse the XML content using XDocument for robust parsing
                var xmlContent = match.Value;
                var xDoc = XDocument.Parse(xmlContent);
                var rootElement = xDoc.Root;

                if (rootElement != null)
                {
                    // Only consider this an XML block if it has child elements (not just text content)
                    var hasChildElements = rootElement.Elements().Any();

                    if (hasChildElements && IsTopLevelXmlBlock(content, match))
                    {
                        var xmlBlock = new XmlBlockDto
                        {
                            TagName = rootElement.Name.LocalName,
                            Fields = ExtractFieldsFromElement(rootElement),
                            RawXml = xmlContent
                        };

                        xmlBlocks.Add(xmlBlock);
                    }
                }
            }
            catch (XmlException)
            {
                // Skip malformed XML blocks - they'll be caught in validation
                continue;
            }
        }

        return xmlBlocks;
    }

    /// <summary>
    /// Extracts fields from an XElement into a dictionary
    /// </summary>
    private static Dictionary<string, string> ExtractFieldsFromElement(XElement element)
    {
        var fields = new Dictionary<string, string>();

        foreach (var child in element.Elements())
        {
            // Only include elements that have text content (no nested elements)
            if (!child.HasElements && !string.IsNullOrWhiteSpace(child.Value))
            {
                fields[child.Name.LocalName] = child.Value.Trim();
            }
        }

        return fields;
    }

    /// <summary>
    /// Determines if an XML block is a top-level block (not nested inside another)
    /// </summary>
    private static bool IsTopLevelXmlBlock(string content, Match xmlMatch)
    {
        var beforeMatch = content.Substring(0, xmlMatch.Index);

        // Count opening and closing tags before this match
        var openTagsBefore = Regex.Matches(beforeMatch, @"<(\w+)>").Count;
        var closeTagsBefore = Regex.Matches(beforeMatch, @"</(\w+)>").Count;

        // If we have more opening tags than closing tags before this match,
        // then this block is nested inside another
        return openTagsBefore <= closeTagsBefore;
    }

    /// <summary>
    /// Extracts individual tagged fields from text content (excluding actual XML blocks)
    /// </summary>
    public async Task<Dictionary<string, string>> ExtractTaggedFieldsAsync(string content)
    {
        await Task.CompletedTask; // Async placeholder for future enhancement

        var taggedFields = new Dictionary<string, string>();

        // Get actual XML blocks first
        var xmlBlocks = await ExtractXmlBlocksAsync(content);

        // Remove only the actual XML blocks from content
        var contentWithoutXmlBlocks = content;
        foreach (var xmlBlock in xmlBlocks)
        {
            contentWithoutXmlBlocks = contentWithoutXmlBlocks.Replace(xmlBlock.RawXml, "");
        }

        // Extract individual tagged fields from remaining content
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
    /// Validates XML structure and checks for unclosed tags using XDocument
    /// </summary>
    public async Task<ValidationResult> ValidateXmlStructureAsync(string content)
    {
        await Task.CompletedTask; // Async placeholder for future enhancement

        var result = new ValidationResult { IsValid = true };

        if (!_validationRules.IsContentValid(content))
        {
            result.IsValid = false;
            result.Errors.Add(_validationRules.GetMessages().EmptyContentError);
            return result;
        }

        // Check for unclosed tags using a more robust approach
        var unclosedTags = FindUnclosedTags(content);
        if (unclosedTags.Any())
        {
            result.IsValid = false;
            result.Errors.Add(_validationRules.GetMessages().UnclosedTagError);
        }

        // Validate XML structure for XML blocks using XDocument
        var xmlBlockMatches = XmlBlockFinderRegex.Matches(content);
        foreach (Match match in xmlBlockMatches)
        {
            try
            {
                XDocument.Parse(match.Value);
            }
            catch (XmlException)
            {
                result.IsValid = false;
                // Extract the root tag name from the match for the error
                var rootTagMatch = Regex.Match(match.Value, @"<(\w+)");
                var rootTagName = rootTagMatch.Success ? rootTagMatch.Groups[1].Value : "unknown";
                result.Errors.Add($"{_validationRules.GetMessages().MalformedXmlError}: {rootTagName}");
            }
        }

        return result;
    }

    /// <summary>
    /// Finds unclosed tags in the content using improved logic
    /// </summary>
    private static List<string> FindUnclosedTags(string content)
    {
        var unclosedTags = new List<string>();
        var tagStack = new Stack<string>();

        // Find all opening and closing tags in order
        var allTagMatches = Regex.Matches(content, @"<(/?)(\w+)>", RegexOptions.IgnoreCase);

        foreach (Match match in allTagMatches)
        {
            var isClosing = !string.IsNullOrEmpty(match.Groups[1].Value);
            var tagName = match.Groups[2].Value.ToLower();

            if (isClosing)
            {
                // Closing tag
                if (tagStack.Count > 0 && tagStack.Peek() == tagName)
                {
                    tagStack.Pop(); // Properly closed
                }
                else
                {
                    // Mismatched closing tag - could indicate unclosed tag
                    unclosedTags.Add(tagName);
                }
            }
            else
            {
                // Opening tag
                tagStack.Push(tagName);
            }
        }

        // Any remaining tags in the stack are unclosed
        while (tagStack.Count > 0)
        {
            unclosedTags.Add(tagStack.Pop());
        }

        return unclosedTags.Distinct().ToList();
    }
}
