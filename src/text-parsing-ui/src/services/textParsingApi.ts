import axios, { AxiosResponse, AxiosError } from 'axios';
import { ParseResult, ParseRequest, ApiError, ValidationError, ApiResponse } from '../types/api';

// API Configuration
// In production, React app is served from the same origin as the API
// So we can use relative URLs or detect the current origin
const API_BASE_URL = process.env.REACT_APP_API_URL ||
  (process.env.NODE_ENV === 'production' ? '' : 'https://localhost:7000');
const API_TIMEOUT = 10000; // 10 seconds

// Create axios instance with default configuration
const apiClient = axios.create({
  baseURL: API_BASE_URL,
  timeout: API_TIMEOUT,
  headers: {
    'Content-Type': 'application/json',
    'Accept': 'application/json',
  },
});

// Request interceptor for logging
apiClient.interceptors.request.use(
  (config) => {
    console.log(`API Request: ${config.method?.toUpperCase()} ${config.url}`);
    return config;
  },
  (error) => {
    console.error('API Request Error:', error);
    return Promise.reject(error);
  }
);

// Response interceptor for error handling
apiClient.interceptors.response.use(
  (response) => {
    console.log(`API Response: ${response.status} ${response.statusText}`);
    return response;
  },
  (error) => {
    console.error('API Response Error:', error);

    if (error.response) {
      // Server responded with error status
      const { status, data } = error.response;

      if (status === 400 && data?.errors) {
        // Validation errors
        throw new ValidationError(
          'Validation failed',
          Array.isArray(data.errors) ? data.errors : [data.errors]
        );
      }

      throw new ApiError(
        data?.message || `HTTP ${status}: ${error.response.statusText}`,
        status,
        data?.errors
      );
    } else if (error.request) {
      // Network error
      throw new ApiError(
        'Network error: Please check your internet connection and ensure the API server is running.',
        0
      );
    } else {
      // Other error
      throw new ApiError(error.message || 'An unexpected error occurred');
    }
  }
);

/**
 * Text Parsing API Service
 */
export class TextParsingApiService {
  /**
   * Parse text content and extract XML blocks, tagged fields, and calculate tax
   */
  static async parseText(content: string): Promise<ParseResult> {
    try {
      const request: ParseRequest = { content };

      const response: AxiosResponse<ApiResponse<ParseResult>> = await apiClient.post(
        '/api/textparser/parse',
        request
      );

      // Extract data from the API response wrapper
      if (response.data.success && response.data.data) {
        return response.data.data;
      } else {
        // Handle API-level errors
        throw new ValidationError(
          response.data.errors?.[0] || 'API returned an error',
          response.data.errors || []
        );
      }
    } catch (error) {
      console.error('Parse text failed:', error);
      throw error;
    }
  }

  /**
   * Health check endpoint to verify API connectivity
   */
  static async healthCheck(): Promise<boolean> {
    try {
      const response = await apiClient.get('/health');
      return response.status === 200;
    } catch (error) {
      console.warn('Health check failed:', error);
      return false;
    }
  }

  /**
   * Test connection to API
   */
  static async testConnection(): Promise<{ connected: boolean; message: string }> {
    try {
      await this.healthCheck();
      return {
        connected: true,
        message: `Connected to API at ${API_BASE_URL}`
      };
    } catch (error) {
      return {
        connected: false,
        message: `Failed to connect to API at ${API_BASE_URL}`
      };
    }
  }
}

export default TextParsingApiService;
