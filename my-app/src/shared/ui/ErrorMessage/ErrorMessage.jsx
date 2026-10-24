import React from 'react';
import './ErrorMessage.css';

const ErrorMessage = ({ message, onRetry, onClearCache }) => {
    return (
        <div className="error-message">
            <p>{message}</p>
            {onRetry && (
                <button onClick={onRetry} className="retry-btn">
                    Попробовать снова
                </button>
            )}
            {onClearCache && (
                <button
                    onClick={onClearCache}
                    className="retry-btn retry-btn--secondary"
                >
                    Очистить кэш
                </button>
            )}
        </div>
    );
};

export default ErrorMessage;