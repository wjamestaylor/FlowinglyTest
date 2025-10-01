# TextParsingApp Development Guidelines

## 🎯 Project Context & Mission
**Challenge**: Build a text parsing system that extracts XML blocks and tagged fields from email-like content
**Tech Stack**: .NET 8 Web API + React TypeScript frontend
**Assessment Focus**: Code quality, testing, architecture, and presentation

## 🏗️ Architecture Principles

### **Clean Architecture**
- **Domain Layer**: Business logic for XML/tag parsing, validation rules
- **Application Layer**: Use cases, DTOs, service interfaces  
- **Infrastructure Layer**: External concerns (logging, configuration)
- **Presentation Layer**: Controllers (API) and React components (UI)

### **Separation of Concerns**
```
src/TextParsingApi/           # Backend (.NET 8)
├── Controllers/              # HTTP endpoints
├── Models/                   # DTOs and request/response models
├── Services/                 # Business logic interfaces
├── Services/Implementation/  # Concrete business logic
├── Validation/              # Input validation logic
└── Tests/                   # Unit and integration tests

src/text-parsing-ui/         # Frontend (React TypeScript)
├── src/components/          # Reusable UI components
├── src/services/           # API client logic
├── src/types/              # TypeScript type definitions
├── src/utils/              # Helper functions
└── src/__tests__/          # Component and integration tests
```

## 💻 Development Standards

### **.NET 8 Backend Standards**

#### **Code Style**
- Use PascalCase for public members, camelCase for private fields
- Async/await for all I/O operations
- Repository pattern for data access (if needed)
- Dependency injection for all services

#### **API Design**
```csharp
// Good: RESTful endpoint design
[HttpPost("api/textparser/parse")]
public async Task<ActionResult<ParseResultDto>> ParseText([FromBody] ParseRequestDto request)

// Good: Proper error handling
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T Data { get; set; }
    public List<string> Errors { get; set; } = new();
}
```

#### **Validation Rules**
```csharp
// Business Rules Implementation
public class ValidationRule
{
    public const string MISSING_TOTAL_ERROR = "Missing required <total> tag";
    public const string UNCLOSED_TAG_ERROR = "Unclosed tag detected";
    public const string DEFAULT_COST_CENTRE = "UNKNOWN";
}
```

#### **Testing Approach**
- Unit tests for parsing logic, validation, calculations
- Integration tests for API endpoints
- Test data matching challenge examples exactly
- Arrange-Act-Assert pattern

### **React TypeScript Frontend Standards**

#### **Component Design**
```typescript
// Good: Typed props interface
interface TextInputProps {
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
  error?: string;
}

// Good: Functional components with hooks
const TextInput: React.FC<TextInputProps> = ({ value, onChange, placeholder, error }) => {
  // Component logic
};
```

#### **API Integration**
```typescript
// Good: Typed API responses
interface ParseResult {
  xmlBlocks: XmlBlock[];
  taggedFields: Record<string, string>;
  calculations: TaxCalculation;
  isValid: boolean;
  errors: string[];
}

// Good: Error handling
const parseText = async (content: string): Promise<ParseResult> => {
  try {
    const response = await apiClient.post<ParseResult>('/api/textparser/parse', { content });
    return response.data;
  } catch (error) {
    throw new Error(`Parse failed: ${error.message}`);
  }
};
```

#### **State Management**
- React hooks for local state
- Context for shared state (if needed)
- No external state library needed for this scope

## 🧪 Testing Strategy

### **Test-Driven Development**
1. **Red**: Write failing test for business requirement
2. **Green**: Implement minimum code to pass test  
3. **Refactor**: Clean up implementation while keeping tests green

### **Test Categories**
```csharp
// Unit Tests - Business Logic
[Test]
public void ParseXmlBlock_ValidExpenseXml_ReturnsCorrectStructure()
[Test] 
public void CalculateTax_ValidTotal_ReturnsCorrectTaxAmount()
[Test]
public void ValidateContent_MissingTotal_ReturnsValidationError()

// Integration Tests - API Endpoints
[Test]
public async Task PostParseText_ValidContent_ReturnsSuccessResponse()
[Test]
public async Task PostParseText_InvalidContent_ReturnsBadRequest()
```

### **Frontend Testing**
```typescript
// Component Tests
test('TextInput displays error message when validation fails', () => {});
test('ParseResults renders XML blocks correctly', () => {});

// Integration Tests  
test('Submit button calls API and displays results', async () => {});
test('Clear button resets form state', () => {});
```

## 📐 Business Logic Implementation

### **XML Parsing Strategy**
```csharp
public interface IXmlParser
{
    Task<List<XmlBlock>> ExtractXmlBlocks(string content);
    Task<Dictionary<string, string>> ExtractTaggedFields(string content);
    Task<ValidationResult> ValidateContent(string content);
}
```

### **Tax Calculation (NZ GST)**
```csharp
public class TaxCalculator
{
    private const decimal GST_RATE = 15.0m;
    private const decimal TAX_DIVISOR = 115.0m; // 100 + GST_RATE
    
    public TaxCalculation CalculateFromTotal(decimal totalIncludingTax)
    {
        var taxAmount = totalIncludingTax * (GST_RATE / TAX_DIVISOR);
        var totalExcludingTax = totalIncludingTax - taxAmount;
        
        return new TaxCalculation
        {
            TotalIncludingTax = totalIncludingTax,
            TaxAmount = Math.Round(taxAmount, 2),
            TotalExcludingTax = Math.Round(totalExcludingTax, 2)
        };
    }
}
```

### **Validation Rules**
1. **Unclosed Tags**: Reject entire message
2. **Missing `<total>`**: Reject entire message  
3. **Missing `<cost_centre>`**: Default to "UNKNOWN"
4. **Malformed XML**: Graceful error handling

### **Extensible Validation System**
The application features a fully configurable validation system:

```csharp
// Add new required fields
validationConfig.FieldRules.Add(new FieldValidationRule
{
    FieldName = "currency",
    IsRequired = true,
    CustomErrorMessage = "Currency is required"
});

// Add default values
validationConfig.FieldRules.Add(new FieldValidationRule
{
    FieldName = "department",
    DefaultValue = "GENERAL"
});

// Custom validation logic
validationConfig.FieldRules.Add(new FieldValidationRule
{
    FieldName = "priority",
    CustomValidator = value => int.TryParse(value, out int p) && p >= 1 && p <= 5,
    CustomErrorMessage = "Priority must be 1-5"
});
```

**Key Components:**
- `FieldValidationRule`: Individual field rules with custom logic
- `ValidationConfiguration`: Central rule container
- `ValidationRules`: Runtime rule management service
- **Runtime Modification**: Add/remove rules during application lifetime

## 🚀 Performance Considerations

### **Backend Optimizations**
- Use `StringBuilder` for string manipulation
- Async patterns for I/O operations
- Efficient regex patterns for tag detection
- Memory-conscious XML parsing

### **Frontend Optimizations**  
- Debounced input for real-time validation
- Memoized components for expensive renders
- Efficient re-renders with proper key props

## 🔧 Development Workflow

### **Git Strategy**
- Feature branches for each component
- Descriptive commit messages
- Small, focused commits

### **Code Review Checklist**
- [ ] Business requirements met
- [ ] Tests written and passing
- [ ] Error handling implemented
- [ ] Type safety maintained (TypeScript)
- [ ] Performance considerations addressed
- [ ] Documentation updated

## 📚 Key Resources

### **Challenge Requirements**
- Parse XML blocks and tagged fields from email-like text
- Calculate tax from `<total>` values (15% GST rate)
- Handle validation errors per specification
- REST API with React UI
- Professional code structure and testing

### **Technology Documentation**
- [.NET 8 Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [ASP.NET Core Web API](https://docs.microsoft.com/en-us/aspnet/core/web-api/)
- [React TypeScript](https://react-typescript-cheatsheet.netlify.app/)
- [Testing with xUnit](https://xunit.net/)

---

## 🎯 Success Metrics
- ✅ All challenge requirements implemented
- ✅ Comprehensive test coverage (>80%)
- ✅ Clean, maintainable code structure  
- ✅ Proper error handling and validation
- ✅ Professional documentation
- ✅ Working end-to-end demo

**Remember**: This is an assessment of coding ability, architecture thinking, and professional development practices. Code quality and structure are as important as functionality.
