import React from 'react';

const Step3Payment = ({ formData, onInputChange }) => {
    return (
        <div className="checkout-step">
            <h2 className="step-title">Способ оплаты</h2>

            <div className="payment-methods">
                <label className="payment-option">
                    <input
                        type="radio"
                        name="paymentMethod"
                        value="card"
                        checked={formData.paymentMethod === 'card'}
                        onChange={onInputChange}
                    />
                    <div className="payment-info">
                        <strong>Банковская карта</strong>
                    </div>
                </label>

                <label className="payment-option">
                    <input
                        type="radio"
                        name="paymentMethod"
                        value="cash"
                        checked={formData.paymentMethod === 'cash'}
                        onChange={onInputChange}
                    />
                    <div className="payment-info">
                        <strong>Наличные при получении</strong>
                    </div>
                </label>
            </div>

            <div className="form-group full-width">
                <label>Комментарий к заказу</label>
                <textarea
                    name="comment"
                    value={formData.comment}
                    onChange={onInputChange}
                    className="form-textarea"
                    rows="3"
                    placeholder="Дополнительная информация"
                />
            </div>
        </div>
    );
};

export default Step3Payment;