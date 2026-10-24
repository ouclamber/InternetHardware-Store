import React from 'react';
import './Notification.css';

const Notification = ({ show, productName, message = 'Товар добавлен в корзину!', type = 'success' }) => {
    if (!show) return null;

    return (
        <div className={`cart-notification ${type === 'error' ? 'cart-notification--error' : ''}`} role="status">
            <div className="cart-notification-content">
                <div className="cart-notification-text">
                    {productName && <strong>{productName}</strong>}
                    <span>{message}</span>
                </div>
            </div>
        </div>
    );
};

export default Notification;
