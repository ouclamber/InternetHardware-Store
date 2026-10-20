import React from 'react';
import './LoadingSpinner.css';

const LoadingSpinner = ({ text = 'Загрузка...' }) => {
    return (
        <div className="loading-spinner">
            <div className="spinner"></div>
            <p>{text}</p>
        </div>
    );
};

export default LoadingSpinner;