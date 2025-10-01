# Text Parsing and XML Extraction Challenge

A .NET Core Web API backend with React TypeScript frontend for parsing text content and extracting XML data from email-like messages.

## 📋 Challenge Overview

This application parses text content to extract:
- **XML blocks**: Complete embedded XML islands 
- **Tagged fields**: Individual opening/closing tag pairs
- **Sales tax calculations**: From extracted total amounts
- **Validation**: Error handling for malformed content

## 🏗️ Architecture

```
FlowinglyTest/
├── src/
│   ├── TextParsingApi/          # .NET 8 Web API
│   │   ├── Controllers/         # REST API endpoints
│   │   ├── Models/             # Data models & DTOs
│   │   ├── Services/           # Business logic
│   │   └── Tests/              # Unit tests
│   └── text-parsing-ui/        # React TypeScript frontend
│       ├── src/
│       ├── public/
│       └── package.json
├── TextParsingApp.sln          # Solution file
└── README.md
```

## 🚀 Quick Start

### Prerequisites
- .NET 8.0 SDK
- Node.js 18+ & npm
- Git

### Backend (.NET API)
```bash
# Run the API
cd src/TextParsingApi
dotnet run

# API will be available at: https://localhost:7000
```

### Frontend (React)
```bash
# Install dependencies and start
cd src/text-parsing-ui
npm install
npm start

# UI will be available at: http://localhost:3000
```

### Full Solution
```bash
# Build entire solution
dotnet build

# Run tests
dotnet test
```

## 🧪 Test Data

### Valid Example
```
Hi Patricia,
Please create an expense claim for the below. Relevant details are marked up as requested…

<expense><cost_centre>DEV632</cost_centre><total>35,000</total><payment_method>personal card</payment_method></expense>

From: William Steele
Subject: Reservation Request

Please create a reservation for 10 at the <vendor>Seaside Steakhouse</vendor> for our <description>development team's project end celebration</description> on <date>27 April 2022</date> at 7.30pm.
```

### Expected Output
```json
{
  "xmlBlocks": [
    {
      "expense": {
        "cost_centre": "DEV632",
        "total": "35,000",
        "payment_method": "personal card"
      }
    }
  ],
  "taggedFields": {
    "vendor": "Seaside Steakhouse",
    "description": "development team's project end celebration", 
    "date": "27 April 2022"
  },
  "calculations": {
    "totalIncludingTax": 35000,
    "taxAmount": 5384.62,
    "totalExcludingTax": 29615.38
  },
  "isValid": true
}
```

## 🔌 API Endpoints

### POST /api/textparser/parse
Parses text content and extracts XML/tagged data.

**Request:**
```json
{
  "content": "Your text content here..."
}
```

**Response:**
```json
{
  "xmlBlocks": [...],
  "taggedFields": {...},
  "calculations": {...},
  "isValid": true,
  "errors": []
}
```

## ⚠️ Validation Rules

| Rule | Behavior |
|------|----------|
| **Unclosed tags** | Reject entire message |
| **Missing `<total>`** | Reject entire message |
| **Missing `<cost_centre>`** | Default to "UNKNOWN" |

## 🧮 Tax Calculation

- **Tax Rate**: 15.38% (NZ GST equivalent)
- **Formula**: `Tax Amount = Total × (15.38 ÷ 115.38)`
- **Total Excl. Tax**: `Total Including Tax - Tax Amount`

## 🧪 Testing Strategy

### Unit Tests
- XML parsing logic
- Tag validation
- Tax calculations
- Error handling

### Integration Tests  
- API endpoint testing
- End-to-end request/response

### E2E Tests (Optional)
- UI workflow testing
- Full stack integration

## 🛠️ Development

### Backend Development
```bash
cd src/TextParsingApi

# Watch mode for development
dotnet watch run

# Add new packages
dotnet add package PackageName

# Run specific tests
dotnet test --filter "TestName"
```

### Frontend Development
```bash
cd src/text-parsing-ui

# Development with hot reload
npm start

# Add new packages
npm install package-name

# Build for production
npm run build

# Run tests
npm test
```

## 📦 Dependencies

### Backend
- **ASP.NET Core 8.0**: Web API framework
- **System.Xml**: XML parsing
- **Microsoft.AspNetCore.Cors**: Cross-origin requests
- **Swashbuckle.AspNetCore**: API documentation

### Frontend
- **React 18**: UI framework
- **TypeScript**: Type safety
- **Axios**: HTTP client
- **@types/react**: TypeScript definitions

## 🔧 Configuration

### API Settings (`appsettings.json`)
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "AllowedHosts": "*",
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000"]
  }
}
```

### Frontend Settings
- **Proxy**: Configured to proxy API calls to `https://localhost:7000`
- **TypeScript**: Strict mode enabled

## 🚀 Deployment

### Docker (Future)
```dockerfile
# Multi-stage build for both frontend and backend
# Production-ready containerization
```

### Azure/Cloud Deployment
- Backend: Azure App Service
- Frontend: Azure Static Web Apps
- Database: Azure SQL (if needed)

## 🎯 Success Criteria

- ✅ Parse XML blocks and tagged fields
- ✅ Calculate sales tax correctly  
- ✅ Handle all validation scenarios
- ✅ REST API with proper error handling
- ✅ React UI with clear validation feedback
- ✅ Unit and integration tests
- ✅ Clean, maintainable code structure

## 📄 License

This is a coding challenge solution. Code is for demonstration purposes.

---

**Created for**: Software Development Challenge  
**Technology Stack**: .NET 8, React 18, TypeScript  
**Created**: October 2025