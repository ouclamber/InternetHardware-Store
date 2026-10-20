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
                    className="retry-btn"
                    style={{ marginLeft: '10px', backgroundColor: '#6c757d' }}
                >
                    Очистить кэш
                </button>
            )}
        </div>
    );
};

export default ErrorMessage;