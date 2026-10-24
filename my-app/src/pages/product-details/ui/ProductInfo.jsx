import React from 'react';
import './ProductInfo.css';
import { formatPrice } from 'shared/lib/utils/formatPrice';

const ProductInfo = ({
    product,
    quantity,
    isAddingToCart,
    onQuantityIncrease,
    onQuantityDecrease,
    onQuantityInput,
    onAddToCart,
    attributes = [],
    attributesLoading = false,
    extraActions = null
}) => {
    // Остаток приходит с сервера (StockQuantity); старые ответы без поля считаем «в наличии»
    const stock = product.StockQuantity ?? product.stockQuantity;
    const isActive = (product.IsActive ?? product.isActive) !== false;
    const hasStockInfo = stock !== undefined && stock !== null;
    const isInStock = isActive && (!hasStockInfo || stock > 0);
    const reachedStockLimit = hasStockInfo && quantity >= stock;

    const availabilityText = !isActive
        ? 'Снят с продажи'
        : !isInStock
            ? 'Нет в наличии'
            : hasStockInfo && stock <= 5
                ? `В наличии: ${stock} шт.`
                : 'В наличии';

    const groupAttributes = (attrs) => {
        return attrs.reduce((acc, attr) => {
            const group = attr.Group || attr.group || 'Прочее';
            if (!acc[group]) acc[group] = [];
            acc[group].push(attr);
            return acc;
        }, {});
    };

    const grouped = groupAttributes(attributes);
    const groupKeys = Object.keys(grouped);

    return (
        <div className="product-info">
            <h1 className="product-title">{product.Name}</h1>

            <div className="product-price-row">
                <span className="product-price">{formatPrice(product.Price)}</span>
                <span className={`product-availability ${isInStock ? '' : 'product-availability--out'}`.trim()}>
                    {availabilityText}
                </span>
            </div>

            <div className="specs-section">
                <h3 className="specs-title">Характеристики:</h3>

                {attributesLoading && (
                    <p className="specs-loading">Загрузка...</p>
                )}

                {!attributesLoading && attributes.length === 0 && (
                    <p className="no-specs">Характеристики отсутствуют</p>
                )}

                {!attributesLoading && attributes.length > 0 && (
                    <div className="specs-groups">
                        {groupKeys.map(group => (
                            <div key={group} className="specs-group">
                                <h4 className="specs-group-title">{group}</h4>
                                <table className="specs-table">
                                    <tbody>
                                        {grouped[group].map(attr => (
                                            <tr key={attr.Id ?? attr.id ?? attr.AttributeId ?? attr.attributeId ?? (attr.AttributeName ?? attr.attributeName)}>
                                                <td className="spec-name">
                                                    {attr.AttributeName ?? attr.attributeName}
                                                </td>
                                                <td className="spec-value">
                                                    {attr.Value ?? attr.value}
                                                    {attr.Unit && ` ${attr.Unit}`}
                                                </td>
                                            </tr>
                                        ))}
                                    </tbody>
                                </table>
                            </div>
                        ))}
                    </div>
                )}
            </div>

            <div className="quantity-section">
                <div className="quantity-header">
                    <span className="quantity-label">Количество</span>
                    <span className="total-price">
                        Итого: {formatPrice(product.Price * quantity)}
                    </span>
                </div>
                <div className="quantity-controls">
                    <button
                        className="qty-btn"
                        onClick={onQuantityDecrease}
                        disabled={quantity <= 1}
                    >−</button>
                    <input
                        className="qty-input"
                        type="text"
                        value={quantity}
                        onChange={onQuantityInput}
                    />
                    <button
                        className="qty-btn"
                        onClick={onQuantityIncrease}
                        disabled={!isInStock || reachedStockLimit}
                    >+</button>
                </div>

                <button
                    className="add-to-cart-btn"
                    onClick={onAddToCart}
                    disabled={isAddingToCart || !isInStock}
                >
                    {isAddingToCart ? 'Добавление...' : isInStock ? 'В корзину' : 'Нет в наличии'}
                </button>

                {extraActions && <div className="product-extra-actions">{extraActions}</div>}
            </div>
        </div>
    );
};

export default ProductInfo;