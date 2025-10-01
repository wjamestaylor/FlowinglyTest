import React from 'react';
import './LoadingSpinner.css';

interface LoadingSpinnerProps {
    message?: string;
    size?: 'small' | 'medium' | 'large';
}

/**
 * Loading spinner component with optional message
 */
const LoadingSpinner: React.FC<LoadingSpinnerProps> = ({
    message = "Processing...",
    size = 'medium'
}) => {
    return (
        <div className={`loading-spinner ${size}`}>
            <div className="spinner"></div>
            {message && <p className="loading-message">{message}</p>}
        </div>
    );
};

export default LoadingSpinner;
