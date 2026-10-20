import React from 'react';
import './MessageNotification.css';

const MessageNotification = ({ show, message, type = 'success' }) => {
    if (!show) return null;

    return (
        <div className={`message-notification ${type}`}>
            <div className="message-content">
                <span className="message-icon">{type === 'success' ? '✓' : '⚠'}</span>
                <span className="message-text">{message}</span>
            </div>
        </div>
    );
};

export default MessageNotification;