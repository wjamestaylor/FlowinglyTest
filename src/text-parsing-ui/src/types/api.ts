// TypeScript interfaces matching the .NET API models

export interface XmlBlock {
  tagName: string;
  fields: Record<string, string>;
  rawXml: string;
}

export interface TaxCalculation {
  totalIncludingTax: number;
  taxAmount: number;
  totalExcludingTax: number;
  taxRate: number;
}

export interface ParseResult {
  xmlBlocks: XmlBlock[];
  taggedFields: Record<string, string>;
  calculations: TaxCalculation;
  isValid: boolean;
  errors: string[];
}

export interface ParseRequest {
  content: string;
}

export interface ApiResponse<T> {
  success: boolean;
  data: T;
  errors: string[];
}

// Error types for better error handling
export class ApiError extends Error {
  constructor(
    message: string,
    public statusCode?: number,
    public errors?: string[]
  ) {
    super(message);
    this.name = 'ApiError';
  }
}

export class ValidationError extends Error {
  constructor(
    message: string,
    public validationErrors: string[]
  ) {
    super(message);
    this.name = 'ValidationError';
  }
}
