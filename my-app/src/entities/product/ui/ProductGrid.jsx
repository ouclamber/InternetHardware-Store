import React from 'react';
import ProductCard from './ProductCard';
import LoadingSpinner from 'shared/ui/LoadingSpinner/LoadingSpinner';
import ErrorMessage from 'shared/ui/ErrorMessage/ErrorMessage';
import './ProductGrid.css';

const pickId = (product) => {
    return product.Id ?? product.id ?? product.ProductId ?? product.productId;
};

const ProductGrid = ({
    products,
    loading,
    error,
    onRetry,
    onClearCache,
    onProductClick,
    onAddToCart,
    getBrandName,
    getProductImage,
    getProductType,
    productType = 'товаров',
    placeholderText = 'Товар'
}) => {
    if (loading) return <LoadingSpinner text={`Загрузка ${productType}...`} />;
    if (error && !loading) return <ErrorMessage message={error} onRetry={onRetry} onClearCache={onClearCache} />;

    if (products.length === 0) {
        return (
            <div className="no-products">
                <p>Товары не найдены для {productType}</p>
                <div className="suggestions">
                    <p>Попробуйте следующие варианты:</p>
                    <ul>
                        <li>Проверьте наличие {productType} в нашей базе данных</li>
                        <li>Обновите страницу или очистите кэш</li>
                        <li>Попробуйте выбрать другую категорию</li>
                    </ul>
                </div>
                <button onClick={onRetry} className="clear-filters-btn">Обновить список</button>
                {onClearCache && (
                    <button
                        onClick={onClearCache}
                        className="clear-filters-btn"
                        style={{ marginLeft: '10px', backgroundColor: '#dc3545' }}
                    >
                        Очистить кэш
                    </button>
                )}
            </div>
        );
    }

    return (
        <div className="products-grid">
            {products.map((product, index) => (
                <ProductCard
                    key={pickId(product) ?? index}
                    product={product}
                    onClick={onProductClick}
                    onAddToCart={onAddToCart}
                    getBrandName={getBrandName}
                    getProductImage={getProductImage}
                    getProductType={getProductType}
                    placeholderText={placeholderText}
                />
            ))}
        </div>
    );
};

export default ProductGrid;