import React from 'react';

const CheckoutSuccess = ({onContinueShopping }) => {
    return (
        <div className="checkout-success">
            <div className="success-icon">✓</div>
            <h2>Заказ успешно оформлен!</h2>

            <button className="continue-shopping-btn" onClick={onContinueShopping}>
                Продолжить покупки
            </button>
        </div>
    );
};

export default CheckoutSuccess;