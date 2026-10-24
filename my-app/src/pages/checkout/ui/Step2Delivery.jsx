import React from 'react';

const Step2Delivery = ({ formData, totalPrice, onInputChange }) => {
    return (
        <div className="checkout-step">
            <h2 className="step-title">Доставка</h2>

            <div className="delivery-methods">
                <label className="delivery-option">
                    <input
                        type="radio"
                        name="deliveryMethod"
                        value="courier"
                        checked={formData.deliveryMethod === 'courier'}
                        onChange={onInputChange}
                    />
                    <div className="delivery-info">
                        <strong>Курьерская доставка</strong>
                        <span>{totalPrice >= 3000 ? 'Бесплатно' : '300 ₽'}</span>
                    </div>
                </label>

                <label className="delivery-option">
                    <input
                        type="radio"
                        name="deliveryMethod"
                        value="pickup"
                        checked={formData.deliveryMethod === 'pickup'}
                        onChange={onInputChange}
                    />
                    <div className="delivery-info">
                        <strong>Самовывоз</strong>
                        <span>Бесплатно</span>
                    </div>
                </label>
            </div>

            <div className="form-row">
                <div className="form-group full-width">
                    <label>Адрес</label>
                    <input
                        type="text"
                        name="address"
                        value={formData.address}
                        onChange={onInputChange}
                        className="form-input"
                        placeholder="Улица, дом, квартира"
                    />
                </div>
            </div>

            <div className="form-row">
                <div className="form-group">
                    <label>Город</label>
                    <input
                        type="text"
                        name="city"
                        value={formData.city}
                        onChange={onInputChange}
                        className="form-input"
                        placeholder="Город"
                    />
                </div>

                <div className="form-group">
                    <label>Почтовый индекс</label>
                    <input
                        type="text"
                        name="postalCode"
                        value={formData.postalCode}
                        onChange={onInputChange}
                        className="form-input"
                        placeholder="123456"
                    />
                </div>
            </div>
        </div>
    );
};

export default Step2Delivery;