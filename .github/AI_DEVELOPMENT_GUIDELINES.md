# AI Agent Development Guidelines for TextParsingApp

## 🤖 Agent Behavior & Interaction Standards

### **Core Principles**
- **Context Awareness**: Always understand the business domain (text parsing, XML extraction, tax calculation)
- **Code Quality Focus**: Prioritize clean, testable, maintainable code over quick fixes
- **Challenge Alignment**: Every decision should align with assessment criteria
- **Professional Standards**: Write production-ready code, not prototype code

### **Communication Style**
- **Concise Explanations**: Explain reasoning behind technical decisions
- **Show, Don't Tell**: Provide code examples rather than abstract descriptions
- **Error Transparency**: When something fails, explain why and how to fix it
- **Progress Tracking**: Update status clearly when completing tasks

## 🎯 Project-Specific Context

### **Business Domain Understanding**
```yaml
Domain: Text Processing & Data Extraction
Input: Email-like text with embedded XML and tagged fields
Output: Structured data with tax calculations
Validation: Strict rules for malformed content
```

### **Key Business Rules (Critical to Implementation)**
1. **XML Parsing**: Extract complete `<tag>content</tag>` blocks
2. **Tax Calculation**: 15% GST using formula: `tax = total × (15 ÷ 115)`
3. **Validation Hierarchy**:
   - Missing `<total>` → Reject entire message
   - Unclosed tags → Reject entire message  
   - Missing `<cost_centre>` → Default to "UNKNOWN"

### **Success Metrics for AI Implementation**
- ✅ **Functional**: All challenge requirements working correctly
- ✅ **Testable**: Unit tests for core logic, integration tests for API
- ✅ **Maintainable**: Clean separation of concerns, typed interfaces
- ✅ **Professional**: Error handling, logging, documentation
- ✅ **Demonstrable**: Working end-to-end demo with UI

## 🔧 Technical Implementation Guidelines

### **When Writing .NET Code**
```csharp
// ✅ Good: Clean, testable service interface
public interface ITextParsingService
{
    Task<ParseResult> ParseTextAsync(string content);
    Task<ValidationResult> ValidateContentAsync(string content);
}

// ✅ Good: Proper error handling
public class ParseResult
{
    public List<XmlBlock> XmlBlocks { get; set; } = new();
    public Dictionary<string, string> TaggedFields { get; set; } = new();
    public TaxCalculation Calculations { get; set; }
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}

// ❌ Avoid: Mixing concerns in controllers
public class BadController : ControllerBase
{
    public IActionResult Parse(string text)
    {
        // Don't put business logic directly in controller
        var result = text.Contains("<total>") ? "valid" : "invalid";
        return Ok(result);
    }
}
```

### **When Writing React TypeScript Code**
```typescript
// ✅ Good: Typed component with proper error handling
interface ParseResultsProps {
  result: ParseResult | null;
  isLoading: boolean;
  error: string | null;
}

const ParseResults: React.FC<ParseResultsProps> = ({ result, isLoading, error }) => {
  if (isLoading) return <LoadingSpinner />;
  if (error) return <ErrorMessage message={error} />;
  if (!result) return <EmptyState />;
  
  return (
    <div className="parse-results">
      <XmlBlocksDisplay blocks={result.xmlBlocks} />
      <TaggedFieldsDisplay fields={result.taggedFields} />
      <TaxCalculationDisplay calculation={result.calculations} />
    </div>
  );
};

// ❌ Avoid: Untyped props and poor error handling
const BadComponent = ({ data }) => {
  return <div>{data.something}</div>; // No type safety, no error handling
};
```

### **Testing Standards**
```csharp
// ✅ Good: Comprehensive test with clear naming and assertions
[Test]
public async Task ParseTextAsync_ValidExpenseXml_ExtractsCorrectData()
{
    // Arrange
    var service = new TextParsingService();
    var content = @"
        <expense>
            <cost_centre>DEV632</cost_centre>
            <total>35,000</total>
            <payment_method>personal card</payment_method>
        </expense>";

    // Act
    var result = await service.ParseTextAsync(content);

    // Assert
    Assert.That(result.IsValid, Is.True);
    Assert.That(result.XmlBlocks, Has.Count.EqualTo(1));
    Assert.That(result.Calculations.TotalIncludingTax, Is.EqualTo(35000));
    Assert.That(result.Calculations.TaxAmount, Is.EqualTo(4565.22).Within(0.01));
}
```

## 🚀 AI Development Workflow

### **Step-by-Step Implementation Approach**
1. **Domain Models First**: Create DTOs and interfaces before implementation
2. **Core Logic**: Implement parsing and validation services with tests
3. **API Layer**: Create controllers that use services
4. **Frontend Components**: Build UI components with proper typing
5. **Integration**: Connect frontend to API with error handling
6. **Testing**: Add comprehensive test coverage
7. **Documentation**: Update README with examples

### **Code Review Simulation**
Before implementing any feature, AI should consider:
- **Requirements Alignment**: Does this meet the challenge specification?
- **Error Handling**: What happens when input is malformed?
- **Testing**: How would I test this functionality?
- **Maintainability**: Can another developer easily understand and modify this?
- **Performance**: Are there any obvious bottlenecks?

### **Decision-Making Framework**
```yaml
When choosing between options:
1. Challenge Requirements (highest priority)
2. Code Quality & Maintainability
3. Testing & Debugging Ease
4. Performance (within reason)
5. Developer Experience

Example Decision: "Should I use Regex or XDocument for XML parsing?"
- Requirements: Need to handle malformed XML gracefully
- Quality: XDocument is more robust but stricter
- Testing: Both are testable
- Performance: Similar for small texts
- Decision: Use XDocument with try-catch for validation
```

## 📋 Implementation Checklist

### **Backend (.NET 8 API)**
- [ ] **Models**: DTOs for requests/responses
- [ ] **Services**: Business logic interfaces and implementations
- [ ] **Controllers**: RESTful API endpoints
- [ ] **Validation**: Input validation and business rules
- [ ] **Testing**: Unit tests for services, integration tests for controllers
- [ ] **Configuration**: CORS, logging, error handling middleware

### **Frontend (React TypeScript)**
- [ ] **Types**: TypeScript interfaces matching API contracts
- [ ] **Components**: Reusable UI components with proper props
- [ ] **Services**: API client with error handling
- [ ] **State Management**: Local state for form and results
- [ ] **Testing**: Component tests and integration tests
- [ ] **Styling**: Clean, professional appearance

### **Integration & Demo**
- [ ] **API Documentation**: Clear endpoint documentation
- [ ] **Error Scenarios**: Proper error messages in UI
- [ ] **Example Data**: Working demo with challenge examples
- [ ] **Build Process**: Both frontend and backend build successfully
- [ ] **Documentation**: Updated README with setup instructions

## 🎯 Quality Gates

### **Before Considering Complete**
1. **All challenge requirements implemented and tested**
2. **Clean architecture with separation of concerns**
3. **Comprehensive error handling**
4. **Professional UI with proper validation feedback**
5. **Working end-to-end demo**
6. **Updated documentation**

### **Red Flags to Avoid**
- ❌ Business logic in controllers or UI components
- ❌ Unhandled errors or exceptions
- ❌ Missing type safety in TypeScript
- ❌ Hardcoded values instead of configuration
- ❌ No tests for critical business logic
- ❌ Poor user experience with unclear error messages

---

**Remember**: This is a technical assessment. The code should demonstrate professional software development practices, not just functionality.
