import React from 'react';
import './CheckoutSidebar.css';
import { formatPrice } from 'shared/lib/utils/formatPrice';
import { API_URL } from 'shared/api/baseUrl';

const CheckoutSidebar = ({ cartItems, getTotalPrice, getDeliveryPrice, getTotalWithDelivery }) => {
    const getImageUrl = (item) => {
        let imageUrl = item.product?.images?.[0]?.imageUrl || item.product?.images?.[0]?.ImageUrl;
        if (imageUrl && !imageUrl.startsWith('http')) {
            imageUrl = `${API_URL}${imageUrl.startsWith('/') ? '' : '/'}${imageUrl}`;
        }
        return imageUrl;
    };

    return (
        <div className="order-summary-card">
            <h3>Ваш заказ</h3>

            <div className="order-items-summary">
                {cartItems.slice(0, 3).map((item, index) => {
                    const imageUrl = getImageUrl(item);
                    return (
                        <div key={item.id || index} className="summary-item">
                            <div className="summary-item-info">
                                {imageUrl && (
                                    <img
                                        src={imageUrl}
                                        alt={item.product?.name || 'Товар'}
                                        className="summary-item-image"
                                        onError={(e) => {
                                            e.target.onerror = null;
                                            e.target.style.display = 'none';
                                        }}
                                    />
                                )}
                                <div className="summary-item-details">
                                    <div className="summary-item-name">
                                        {item.product?.name || 'Товар'}
                                    </div>
                                    <div className="summary-item-quantity">x{item.quantity}</div>
                                </div>
                            </div>
                            <div className="summary-item-price">
                                {formatPrice((item.product?.price || 0) * item.quantity)}
                            </div>
                        </div>
                    );
                })}
                {cartItems.length > 3 && (
                    <div className="summary-more">
                        и еще {cartItems.length - 3} товар(ов)
                    </div>
                )}
            </div>

            <div className="summary-divider"></div>

            <div className="summary-row">
                <span>Товары</span>
                <span>{formatPrice(getTotalPrice())}</span>
            </div>
            <div className="summary-row">
                <span>Доставка</span>
                <span>
                    {getDeliveryPrice() === 0 ? 'Бесплатно' : formatPrice(getDeliveryPrice())}
                </span>
            </div>
            <div className="summary-total">
                <span>Итого</span>
                <span>{formatPrice(getTotalWithDelivery())}</span>
            </div>
        </div>
    );
};

export default CheckoutSidebar;