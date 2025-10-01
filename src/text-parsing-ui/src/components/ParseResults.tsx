import React from 'react';
import { XmlBlock, TaxCalculation } from '../types/api';
import './ParseResults.css';

interface ParseResultsProps {
  xmlBlocks: XmlBlock[];
  taggedFields: Record<string, string>;
  calculations: TaxCalculation;
  isValid: boolean;
}

/**
 * Component for displaying parsed results in a structured format
 */
const ParseResults: React.FC<ParseResultsProps> = ({
  xmlBlocks,
  taggedFields,
  calculations,
  isValid
}) => {
  const formatCurrency = (amount: number): string => {
    return new Intl.NumberFormat('en-NZ', {
      style: 'currency',
      currency: 'NZD'
    }).format(amount);
  };

  const renderJsonSection = (title: string, data: any, className: string = '') => (
    <div className={`json-section ${className}`}>
      <h3 className="section-title">{title}</h3>
      <pre className="json-content">
        {JSON.stringify(data, null, 2)}
      </pre>
    </div>
  );

  const renderXmlBlocks = () => {
    if (!xmlBlocks || xmlBlocks.length === 0) {
      return (
        <div className="json-section">
          <h3 className="section-title">XML Blocks</h3>
          <p className="empty-state">No XML blocks found</p>
        </div>
      );
    }

    return (
      <div className="json-section">
        <h3 className="section-title">XML Blocks ({xmlBlocks.length})</h3>
        {xmlBlocks.map((block, index) => (
          <div key={index} className="xml-block">
            <h4 className="block-title">
              {block.tagName || `Block ${index + 1}`}
            </h4>
            <pre className="json-content">
              {JSON.stringify(block.fields, null, 2)}
            </pre>
          </div>
        ))}
      </div>
    );
  };

  const renderTaggedFields = () => {
    const hasFields = taggedFields && Object.keys(taggedFields).length > 0;

    return (
      <div className="json-section">
        <h3 className="section-title">Tagged Fields</h3>
        {hasFields ? (
          <pre className="json-content">
            {JSON.stringify(taggedFields, null, 2)}
          </pre>
        ) : (
          <p className="empty-state">No tagged fields found</p>
        )}
      </div>
    );
  };

  const renderCalculations = () => {
    if (!calculations) {
      return (
        <div className="json-section">
          <h3 className="section-title">Tax Calculations</h3>
          <p className="empty-state">No calculations available</p>
        </div>
      );
    }

    return (
      <div className="json-section calculations">
        <h3 className="section-title">Tax Calculations (15% GST)</h3>
        <div className="calculation-grid">
          <div className="calculation-item">
            <span className="calculation-label">Total (Including Tax):</span>
            <span className="calculation-value primary">
              {formatCurrency(calculations.totalIncludingTax)}
            </span>
          </div>
          <div className="calculation-item">
            <span className="calculation-label">Tax Amount:</span>
            <span className="calculation-value tax">
              {formatCurrency(calculations.taxAmount)}
            </span>
          </div>
          <div className="calculation-item">
            <span className="calculation-label">Total (Excluding Tax):</span>
            <span className="calculation-value">
              {formatCurrency(calculations.totalExcludingTax)}
            </span>
          </div>
        </div>

        {/* JSON representation */}
        <details className="json-details">
          <summary>View as JSON</summary>
          <pre className="json-content">
            {JSON.stringify(calculations, null, 2)}
          </pre>
        </details>
      </div>
    );
  };

  return (
    <div className={`parse-results ${isValid ? 'valid' : 'invalid'}`}>
      <div className="results-header">
        <h2 className="results-title">
          Parse Results
          <span className={`status-badge ${isValid ? 'success' : 'error'}`}>
            {isValid ? '✓ Valid' : '✗ Invalid'}
          </span>
        </h2>
      </div>

      <div className="results-content">
        {renderXmlBlocks()}
        {renderTaggedFields()}
        {renderCalculations()}
      </div>

      {/* Complete JSON output as specified in requirements */}
      <details className="full-json">
        <summary>View Complete JSON Output</summary>
        <pre className="json-content">
          {JSON.stringify({
            xmlBlocks,
            taggedFields,
            calculations,
            isValid
          }, null, 2)}
        </pre>
      </details>
    </div>
  );
};

export default ParseResults;
