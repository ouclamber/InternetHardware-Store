import React from 'react';
import './QuantityControl.css';

const QuantityControl = ({ 
    quantity, 
    onIncrease, 
    onDecrease, 
    onChange, 
    disabled = false, 
    min = 1,
    showInput = true
}) => {
    return (
        <div className="quantity-control">
            <button 
                className="quantity-btn minus"
                onClick={onDecrease}
                disabled={disabled || quantity <= min}
            >
                {showInput ? '-' : <svg width="16" height="16" viewBox="0 0 24 24" fill="none"><path d="M5 12H19" stroke="currentColor" strokeWidth="2" strokeLinecap="round"/></svg>}
            </button>
            
            {showInput ? (
                <input 
                    type="number" 
                    className="quantity-input"
                    value={quantity} 
                    onChange={onChange}
                    min={min}
                    disabled={disabled}
                />
            ) : (
                <span className="quantity-value">{quantity}</span>
            )}
            
            <button 
                className="quantity-btn plus"
                onClick={onIncrease}
                disabled={disabled}
            >
                {showInput ? '+' : <svg width="16" height="16" viewBox="0 0 24 24" fill="none"><path d="M12 5V19M5 12H19" stroke="currentColor" strokeWidth="2" strokeLinecap="round"/></svg>}
            </button>
        </div>
    );
};

export default QuantityControl;