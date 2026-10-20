import React from 'react';
import './CheckoutSteps.css';

const CheckoutSteps = ({ step, paymentMethod, isConfirmationStep, isSuccessStep }) => {
    return (
        <div className="checkout-steps">
            <div className={`step-indicator ${step >= 1 ? 'active' : ''}`}>
                <span className="step-number">1</span>
                <span className="step-label">Информация</span>
            </div>
            <div className="step-line"></div>

            <div className={`step-indicator ${step >= 2 ? 'active' : ''}`}>
                <span className="step-number">2</span>
                <span className="step-label">Доставка</span>
            </div>
            <div className="step-line"></div>

            <div className={`step-indicator ${step >= 3 ? 'active' : ''}`}>
                <span className="step-number">3</span>
                <span className="step-label">Оплата</span>
            </div>
            <div className="step-line"></div>

            {paymentMethod === 'card' && (
                <>
                    <div className={`step-indicator ${step >= 4 ? 'active' : ''}`}>
                        <span className="step-number">4</span>
                        <span className="step-label">Карта</span>
                    </div>
                    <div className="step-line"></div>
                </>
            )}

            <div className={`step-indicator ${isConfirmationStep || isSuccessStep ? 'active' : ''}`}>
                <span className="step-number">{paymentMethod === 'card' ? '5' : '4'}</span>
                <span className="step-label">Подтверждение</span>
            </div>
        </div>
    );
};

export default CheckoutSteps;