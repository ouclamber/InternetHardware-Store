import React from 'react';
import './Notification.css';

const Notification = ({ show, productName, message = 'Товар добавлен в корзину!' }) => {
    if (!show) return null;

    return (
        <div className="cart-notification">
            <div className="cart-notification-content">
                <div className="cart-notification-text">
                    <strong>{productName}</strong>
                    <span>{message}</span>
                </div>
            </div>
        </div>
    );
};

export default Notification;