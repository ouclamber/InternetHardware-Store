import React from 'react';
import { formatPrice } from 'shared/lib/utils/formatPrice';
import { API_URL } from 'shared/api/baseUrl';

const Step5Confirmation = ({ cartItems, getTotalPrice, getDeliveryPrice, getTotalWithDelivery }) => {
    const getImageUrl = (item) => {
        let imageUrl = item.product?.images?.[0]?.imageUrl || item.product?.images?.[0]?.ImageUrl;
        if (imageUrl && !imageUrl.startsWith('http')) {
            imageUrl = `${API_URL}${imageUrl.startsWith('/') ? '' : '/'}${imageUrl}`;
        }
        return imageUrl;
    };

    const totalQuantity = cartItems.reduce((s, i) => s + i.quantity, 0);

    return (
        <div className="checkout-step">
            <h2 className="step-title">Подтверждение заказа</h2>

            <div className="order-summary">
                <h3>Ваш заказ</h3>

                <div className="order-items">
                    {cartItems.map((item, index) => {
                        const imageUrl = getImageUrl(item);
                        return (
                            <div key={item.id || index} className="order-item">
                                <div className="order-item-image">
                                    {imageUrl ? (
                                        <img
                                            src={imageUrl}
                                            alt={item.product?.name || 'Товар'}
                                            onError={(e) => {
                                                e.target.onerror = null;
                                                e.target.style.display = 'none';
                                            }}
                                        />
                                    ) : (
                                        <div className="no-image">📷</div>
                                    )}
                                </div>
                                <div className="order-item-info">
                                    <div className="order-item-name">
                                        {item.product?.name || 'Товар'}
                                    </div>
                                </div>
                                <div className="order-item-price">
                                    {formatPrice((item.product?.price || 0) * item.quantity)}
                                </div>
                            </div>
                        );
                    })}
                </div>

                <div className="summary-totals">
                    <div className="summary-row">
                        <span>Товары ({totalQuantity} шт.)</span>
                        <span>{formatPrice(getTotalPrice())}</span>
                    </div>
                    <div className="summary-row">
                        <span>Доставка</span>
                        <span>
                            {getDeliveryPrice() === 0 ? 'Бесплатно' : formatPrice(getDeliveryPrice())}
                        </span>
                    </div>
                    <div className="summary-total">
                        <span>Итого к оплате</span>
                        <span>{formatPrice(getTotalWithDelivery())}</span>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default Step5Confirmation;