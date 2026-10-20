import React from 'react';
import './ProductCard.css';
import { formatPrice } from 'shared/lib/utils/formatPrice';

const ProductCard = ({
    product,
    onClick,
    onAddToCart,
    getBrandName,
    getProductImage,
    getProductType,
    placeholderText = 'Товар'
}) => {
    const pick = (obj, ...keys) => {
        for (const key of keys) {
            if (obj[key] !== undefined && obj[key] !== null) return obj[key];
        }
        return undefined;
    };

    const productId = pick(product, 'Id', 'id');
    const name = pick(product, 'Name', 'name') || placeholderText;
    const price = pick(product, 'Price', 'price') || 0;
    const description = pick(product, 'Description', 'description') || '';
    const brand = getBrandName ? getBrandName(product) : pick(product, 'BrandName', 'brandName') || 'Не указан';
    const type = getProductType ? getProductType(name) : 'standard';
    const imageUrl = getProductImage ? getProductImage(product) : null;

    // Проверка наличия: учитываем и IsActive, и StockQuantity
    const isActive = pick(product, 'IsActive', 'isActive');
    const stock = pick(product, 'StockQuantity', 'stockQuantity', 'Stock', 'stock');
    const isInStock = isActive !== false && (stock === undefined || stock > 0);

    const handleCardClick = () => {
        if (onClick) onClick(productId);
    };

    const handleAddToCart = (e) => {
        e.stopPropagation();
        if (!isInStock) return;
        if (onAddToCart) onAddToCart(productId, 1, e, name);
    };

    return (
        <div className="product-card" onClick={handleCardClick}>
            <div className="product-image-container">
                {imageUrl ? (
                    <img
                        src={imageUrl}
                        alt={name}
                        className={`product-image ${type}`}
                        onError={(e) => {
                            e.target.onerror = null;
                            e.target.style.display = 'none';
                        }}
                    />
                ) : (
                    <div className="no-image-placeholder">📷</div>
                )}
                {brand && brand !== 'Не указан' && (
                    <span className="brand-badge">{brand}</span>
                )}
            </div>

            <div className="product-content">
                <h3 className="product-name">{name}</h3>

                <div className="product-meta">
                    <div className="meta-row">
                        <span className="meta-label">Бренд:</span>
                        <span className="meta-value">{brand}</span>
                    </div>
                    {description && (
                        <div className="meta-row">
                            <span className="meta-label">Описание:</span>
                            <span className="meta-value">{description}</span>
                        </div>
                    )}
                </div>

                <div className="product-price">{formatPrice(price)}</div>

                <div className="product-availability">
                    {isInStock ? (
                        <span className="in-stock">В наличии</span>
                    ) : (
                        <span className="out-of-stock">Нет в наличии</span>
                    )}
                </div>

                <button
                    className="add-to-cart-btn"
                    onClick={handleAddToCart}
                    disabled={!isInStock}
                >
                    В корзину
                </button>
            </div>
        </div>
    );
};

export default ProductCard;