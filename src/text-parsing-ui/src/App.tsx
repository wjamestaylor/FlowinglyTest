import React, { useState, useEffect } from 'react';
import './App.css';
import TextParsingApiService from './services/textParsingApi';
import { ParseResult, ApiError, ValidationError } from './types/api';
import ParseResults from './components/ParseResults';
import ErrorDisplay from './components/ErrorDisplay';
import LoadingSpinner from './components/LoadingSpinner';

/**
 * Main Text Parsing Application Component
 * Meets all original challenge requirements:
 * - Text input area for email/text blocks
 * - Submit and Clear buttons
 * - JSON output display
 * - Clear validation error handling
 * - REST API integration
 */
function App() {
  // State management
  const [inputText, setInputText] = useState<string>('');
  const [parseResult, setParseResult] = useState<ParseResult | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(false);
  const [errors, setErrors] = useState<string[]>([]);
  const [apiConnected, setApiConnected] = useState<boolean | null>(null);

  // Sample data for demonstration
  const sampleData = `Hi Patricia,
Please create an expense claim for the below. Relevant details are marked up as requested…

<expense><cost_centre>DEV632</cost_centre><total>35,000</total><payment_method>personal card</payment_method></expense>

From: William Steele
Sent: Friday, 16 June 2022 10:32 AM
To: Maria Washington
Subject: test

Hi Maria,
Please create a reservation for 10 at the <vendor>Seaside Steakhouse</vendor> for our <description>development team's project end celebration</description> on <date>27 April 2022</date> at 7.30pm.

Regards,
William`;

  // Check API connectivity on component mount
  useEffect(() => {
    checkApiConnection();
  }, []);

  const checkApiConnection = async () => {
    try {
      const result = await TextParsingApiService.testConnection();
      setApiConnected(result.connected);
      if (!result.connected) {
        setErrors([result.message]);
      }
    } catch (error) {
      setApiConnected(false);
      setErrors(['Failed to connect to API server']);
    }
  };

  const handleSubmit = async () => {
    if (!inputText.trim()) {
      setErrors(['Please enter some text to parse']);
      return;
    }

    setIsLoading(true);
    setErrors([]);
    setParseResult(null);

    try {
      const result = await TextParsingApiService.parseText(inputText);
      setParseResult(result);

      // If parsing failed, show errors
      if (!result.isValid && result.errors?.length > 0) {
        setErrors(result.errors);
      }
    } catch (error) {
      console.error('Parse failed:', error);

      if (error instanceof ValidationError) {
        setErrors(error.validationErrors);
      } else if (error instanceof ApiError) {
        setErrors(error.errors || [error.message]);
      } else {
        setErrors(['An unexpected error occurred while parsing the text']);
      }
    } finally {
      setIsLoading(false);
    }
  };

  const handleClear = () => {
    setInputText('');
    setParseResult(null);
    setErrors([]);
  };

  const handleLoadSample = () => {
    setInputText(sampleData);
    setErrors([]);
    setParseResult(null);
  };

  const handleKeyPress = (event: React.KeyboardEvent) => {
    if (event.ctrlKey && event.key === 'Enter') {
      handleSubmit();
    }
  };

  return (
    <div className="app">
      <header className="app-header">
        <h1 className="app-title">Text Parsing & XML Extraction</h1>
        <p className="app-description">
          Extract XML blocks and tagged fields from email-like content with automatic tax calculation
        </p>

        {/* API Connection Status */}
        <div className={`api-status ${apiConnected === null ? 'checking' : apiConnected ? 'connected' : 'disconnected'}`}>
          {apiConnected === null && 'Checking API connection...'}
          {apiConnected === true && '✓ Connected to API'}
          {apiConnected === false && '✗ API Disconnected'}
          {apiConnected === false && (
            <button
              onClick={checkApiConnection}
              className="retry-button"
              type="button"
            >
              Retry
            </button>
          )}
        </div>
      </header>

      <main className="app-main">
        <div className="input-section">
          <div className="input-header">
            <label htmlFor="text-input" className="input-label">
              Text Input
            </label>
            <div className="input-actions">
              <button
                onClick={handleLoadSample}
                className="sample-button"
                type="button"
                disabled={isLoading}
              >
                Load Sample
              </button>
            </div>
          </div>

          <textarea
            id="text-input"
            className="text-input"
            value={inputText}
            onChange={(e) => setInputText(e.target.value)}
            onKeyDown={handleKeyPress}
            placeholder="Paste your email or text content here...

Example:
Hi Patricia,
Please create an expense claim for the below.

<expense><cost_centre>DEV632</cost_centre><total>35,000</total></expense>

Tagged fields: <vendor>Seaside Steakhouse</vendor>"
            rows={12}
            disabled={isLoading}
          />

          <div className="button-group">
            <button
              onClick={handleSubmit}
              className="submit-button"
              disabled={isLoading || !inputText.trim() || apiConnected === false}
              type="button"
            >
              {isLoading ? 'Processing...' : 'Submit'}
            </button>

            <button
              onClick={handleClear}
              className="clear-button"
              disabled={isLoading}
              type="button"
            >
              Clear
            </button>
          </div>

          <div className="input-help">
            <p>💡 <strong>Tips:</strong></p>
            <ul>
              <li>Use Ctrl+Enter to submit quickly</li>
              <li>XML blocks: <code>&lt;expense&gt;&lt;total&gt;100&lt;/total&gt;&lt;/expense&gt;</code></li>
              <li>Tagged fields: <code>&lt;vendor&gt;Company Name&lt;/vendor&gt;</code></li>
              <li>Required: <code>&lt;total&gt;</code> tag for tax calculation</li>
            </ul>
          </div>
        </div>

        {/* Loading State */}
        {isLoading && (
          <LoadingSpinner
            message="Parsing text and calculating tax..."
            size="large"
          />
        )}

        {/* Error Display */}
        {errors.length > 0 && !isLoading && (
          <ErrorDisplay
            errors={errors}
            title="Processing Errors"
          />
        )}

        {/* Results Display */}
        {parseResult && !isLoading && (
          <ParseResults
            xmlBlocks={parseResult.xmlBlocks}
            taggedFields={parseResult.taggedFields}
            calculations={parseResult.calculations}
            isValid={parseResult.isValid}
          />
        )}

        {/* Instructions */}
        {!parseResult && !isLoading && errors.length === 0 && (
          <div className="instructions">
            <h2>How to Use</h2>
            <div className="instruction-grid">
              <div className="instruction-item">
                <h3>📝 Input Text</h3>
                <p>Paste email content or text containing XML blocks and tagged fields</p>
              </div>
              <div className="instruction-item">
                <h3>🔍 XML Extraction</h3>
                <p>Extracts complete XML blocks like <code>&lt;expense&gt;...&lt;/expense&gt;</code></p>
              </div>
              <div className="instruction-item">
                <h3>🏷️ Tagged Fields</h3>
                <p>Finds individual tagged fields like <code>&lt;vendor&gt;Name&lt;/vendor&gt;</code></p>
              </div>
              <div className="instruction-item">
                <h3>💰 Tax Calculation</h3>
                <p>Automatically calculates 15% GST from extracted totals</p>
              </div>
            </div>
          </div>
        )}
      </main>

      <footer className="app-footer">
        <p>Text Parsing Challenge - React TypeScript Frontend with .NET 8 Web API</p>
      </footer>
    </div>
  );
}

export default App;
