import React from 'react';

const Step4Card = ({
    formData,
    fieldErrors,
    onCardNumberChange,
    onCardExpiryChange,
    onCardCvvChange
}) => {
    return (
        <div className="checkout-step">
            <h2 className="step-title">Данные банковской карты</h2>

            <div className="card-payment-form">
                <div className="form-group full-width">
                    <label>Номер карты</label>
                    <input
                        type="text"
                        name="cardNumber"
                        value={formData.cardNumber}
                        onChange={onCardNumberChange}
                        className={`form-input card-input ${fieldErrors.cardNumber ? 'error' : ''}`}
                        placeholder="1234 5678 9012 3456"
                        maxLength="19"
                    />
                    {fieldErrors.cardNumber && (
                        <span className="error-message">{fieldErrors.cardNumber}</span>
                    )}
                </div>

                <div className="form-row">
                    <div className="form-group">
                        <label>Срок действия (ММ/ГГ)</label>
                        <input
                            type="text"
                            name="cardExpiry"
                            value={formData.cardExpiry}
                            onChange={onCardExpiryChange}
                            className={`form-input ${fieldErrors.cardExpiry ? 'error' : ''}`}
                            placeholder="ММ/ГГ"
                            maxLength="5"
                        />
                        {fieldErrors.cardExpiry && (
                            <span className="error-message">{fieldErrors.cardExpiry}</span>
                        )}
                    </div>

                    <div className="form-group">
                        <label>CVV код</label>
                        <input
                            type="password"
                            name="cardCvv"
                            value={formData.cardCvv}
                            onChange={onCardCvvChange}
                            className={`form-input cvv-input ${fieldErrors.cardCvv ? 'error' : ''}`}
                            placeholder="123"
                            maxLength="3"
                        />
                        {fieldErrors.cardCvv && (
                            <span className="error-message">{fieldErrors.cardCvv}</span>
                        )}
                    </div>
                </div>
            </div>
        </div>
    );
};

export default Step4Card;