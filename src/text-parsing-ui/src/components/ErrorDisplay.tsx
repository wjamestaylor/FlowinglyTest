import React from 'react';
import './ErrorDisplay.css';

interface ErrorDisplayProps {
  errors: string[];
  title?: string;
  className?: string;
}

/**
 * Component for displaying validation errors and API errors
 */
const ErrorDisplay: React.FC<ErrorDisplayProps> = ({
  errors,
  title = "Validation Errors",
  className = ""
}) => {
  if (!errors || errors.length === 0) {
    return null;
  }

  return (
    <div className={`error-display ${className}`}>
      <div className="error-header">
        <span className="error-icon">⚠️</span>
        <h3 className="error-title">{title}</h3>
      </div>
      <ul className="error-list">
        {errors.map((error, index) => (
          <li key={index} className="error-item">
            {error}
          </li>
        ))}
      </ul>
    </div>
  );
};

export default ErrorDisplay;
