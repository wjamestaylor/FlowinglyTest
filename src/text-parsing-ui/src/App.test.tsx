import React from 'react';
import { render, screen, fireEvent, waitFor, act } from '@testing-library/react';
import '@testing-library/jest-dom';
import App from './App';

// Mock the API service
jest.mock('./services/textParsingApi', () => ({
  __esModule: true,
  default: {
    parseText: jest.fn(),
    testConnection: jest.fn(),
  }
}));

import TextParsingApiService from './services/textParsingApi';

const mockApiService = TextParsingApiService as jest.Mocked<typeof TextParsingApiService>;

describe('Text Parsing App', () => {
  // Suppress console.error during tests to avoid expected error messages
  const originalError = console.error;

  beforeEach(() => {
    jest.clearAllMocks();
    // Mock successful connection by default
    mockApiService.testConnection.mockResolvedValue({
      connected: true,
      message: 'Connected to API at https://localhost:7000'
    });

    // Suppress console.error for cleaner test output
    console.error = jest.fn();
  });

  afterEach(() => {
    // Restore console.error after each test
    console.error = originalError;
  });

  test('renders main app components', async () => {
    await act(async () => {
      render(<App />);
    });

    // Check if main elements are present
    expect(screen.getByText('Text Parsing & XML Extraction')).toBeInTheDocument();
    expect(screen.getByText('Extract XML blocks and tagged fields from email-like content with automatic tax calculation')).toBeInTheDocument();
    expect(screen.getByLabelText('Text Input')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /submit/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /clear/i })).toBeInTheDocument();
  });

  test('loads sample data when Load Sample button is clicked', async () => {
    await act(async () => {
      render(<App />);
    });

    const loadSampleButton = screen.getByRole('button', { name: /load sample/i });
    const textArea = screen.getByLabelText('Text Input') as HTMLTextAreaElement;

    fireEvent.click(loadSampleButton);

    expect(textArea.value).toContain('Hi Patricia');
    expect(textArea.value).toContain('<expense>');
    expect(textArea.value).toContain('<vendor>Seaside Steakhouse</vendor>');
  });

  test('clears input when Clear button is clicked', async () => {
    await act(async () => {
      render(<App />);
    });

    const textArea = screen.getByLabelText('Text Input') as HTMLTextAreaElement;
    const clearButton = screen.getByRole('button', { name: /clear/i });

    // Add some text
    fireEvent.change(textArea, { target: { value: 'Some test text' } });
    expect(textArea.value).toBe('Some test text');

    // Clear the text
    fireEvent.click(clearButton);
    expect(textArea.value).toBe('');
  });

  test('submit button is disabled for empty input', async () => {
    await act(async () => {
      render(<App />);
    });

    const submitButton = screen.getByRole('button', { name: /submit/i });

    // Submit button should be disabled when input is empty
    expect(submitButton).toBeDisabled();
  });

  test('calls API and displays results on successful parse', async () => {
    const mockParseResult = {
      xmlBlocks: [
        {
          tagName: 'expense',
          fields: {
            cost_centre: 'DEV632',
            total: '35,000',
            payment_method: 'personal card'
          },
          rawXml: '<expense><cost_centre>DEV632</cost_centre><total>35,000</total><payment_method>personal card</payment_method></expense>'
        }
      ],
      taggedFields: {
        vendor: 'Seaside Steakhouse',
        description: 'development team project end celebration',
        date: '27 April 2022'
      },
      calculations: {
        totalIncludingTax: 35000,
        taxAmount: 4565.22,
        totalExcludingTax: 30434.78,
        taxRate: 15
      },
      isValid: true,
      errors: []
    };

    mockApiService.parseText.mockResolvedValue(mockParseResult);

    await act(async () => {
      render(<App />);
    });

    const textArea = screen.getByLabelText('Text Input');
    const submitButton = screen.getByRole('button', { name: /submit/i });

    // Enter test data
    await act(async () => {
      fireEvent.change(textArea, {
        target: { value: '<expense><total>35000</total></expense>' }
      });
    });

    // Submit
    await act(async () => {
      fireEvent.click(submitButton);
    });

    // Wait for API call and results
    await waitFor(() => {
      expect(screen.getByText('Parse Results')).toBeInTheDocument();
    });

    expect(screen.getByText('✓ Valid')).toBeInTheDocument();
    expect(screen.getByText('XML Blocks (1)')).toBeInTheDocument();
    expect(screen.getByText('Tax Calculations (15% GST)')).toBeInTheDocument();

    // Verify API was called with correct data
    expect(mockApiService.parseText).toHaveBeenCalledWith('<expense><total>35000</total></expense>');
  });

  test('handles API errors gracefully', async () => {
    const mockError = new Error('API Error');
    mockApiService.parseText.mockRejectedValue(mockError);

    await act(async () => {
      render(<App />);
    });

    const textArea = screen.getByLabelText('Text Input');
    const submitButton = screen.getByRole('button', { name: /submit/i });

    // Enter test data
    await act(async () => {
      fireEvent.change(textArea, {
        target: { value: 'Some invalid content' }
      });
    });

    // Submit
    await act(async () => {
      fireEvent.click(submitButton);
    });

    // Wait for error to be displayed
    await waitFor(() => {
      expect(screen.getByText('Processing Errors')).toBeInTheDocument();
    });

    expect(screen.getByText('An unexpected error occurred while parsing the text')).toBeInTheDocument();
  });

  test('supports keyboard shortcut Ctrl+Enter for submit', async () => {
    await act(async () => {
      render(<App />);
    });

    const textArea = screen.getByLabelText('Text Input');

    // Add some text
    fireEvent.change(textArea, { target: { value: 'Test content' } });

    // Trigger Ctrl+Enter
    await act(async () => {
      fireEvent.keyDown(textArea, { key: 'Enter', ctrlKey: true });
    });

    // Should trigger API call (would be verified by mock being called)
    expect(mockApiService.parseText).toHaveBeenCalled();
  });

  test('displays instructions when no results are present', async () => {
    await act(async () => {
      render(<App />);
    });

    expect(screen.getByText('How to Use')).toBeInTheDocument();
    expect(screen.getByText('📝 Input Text')).toBeInTheDocument();
    expect(screen.getByText('🔍 XML Extraction')).toBeInTheDocument();
    expect(screen.getByText('🏷️ Tagged Fields')).toBeInTheDocument();
    expect(screen.getByText('💰 Tax Calculation')).toBeInTheDocument();
  });
});
