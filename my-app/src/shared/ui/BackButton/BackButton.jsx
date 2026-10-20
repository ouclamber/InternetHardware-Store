import React from 'react';
import './BackButton.css';

const BackButton = ({ onClick, text = 'Назад', className = '' }) => {
    return (
        <button className={`back-button ${className}`} onClick={onClick}>
            {text}
        </button>
    );
};

export default BackButton;