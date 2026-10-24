import React, { useCallback, useEffect, useState } from 'react';
import Header from 'widgets/header/ui/Header';
import Notification from 'widgets/notification/ui/Notification';
import ProductCard from 'entities/product/ui/ProductCard';
import LoadingSpinner from 'shared/ui/LoadingSpinner/LoadingSpinner';
import BackButton from 'shared/ui/BackButton/BackButton';
import CatalogLayout from 'shared/ui/CatalogLayout/CatalogLayout';
import Pagination from 'shared/ui/Pagination/Pagination';
import { useDebouncedValue } from 'shared/lib/hooks/useDebouncedValue';
import { useCategoryProducts } from 'entities/product/model/useCategoryProducts';
import { useProductHelpers } from 'entities/product/lib/useProductHelpers';
import { detectProductType } from 'entities/product/lib/detectProductType';
import { useAddToCart } from 'features/add-to-cart/model/useAddToCart';
import WishlistButton from 'features/toggle-wishlist/ui/WishlistButton';
import './CategoryCatalog.css';

export const ITEMS_PER_PAGE = 6;
export const PRICE_DEBOUNCE_MS = 400;

const SORT_OPTIONS = [
    { value: 'relevance', label: 'По релевантности' },
    { value: 'price_asc', label: 'Сначала дешевые' },
    { value: 'price_desc', label: 'Сначала дорогие' },
    { value: 'name_asc', label: 'По названию (А-Я)' },
    { value: 'newest', label: 'Сначала новые' }
];

const pick = (obj, ...keys) => {
    for (const key of keys) {
        if (obj[key] !== undefined && obj[key] !== null) return obj[key];
    }
    return undefined;
};

/**
 * Общая страница категории каталога.
 *
 * Раньше пять страниц (смартфоны, ноутбуки, ПК, ТВ, колонки) были копиями друг друга
 * по ~200 строк и фильтровали/листали товары в браузере. Теперь страница категории —
 * это конфигурация этого виджета, а фильтры, сортировка и пагинация выполняются на сервере.
 */
const CategoryCatalog = ({
    navigate,
    title,
    categoryName,
    brandHints = {},
    typeRules = [],
    placeholderText = 'Товар',
    pageClassName = '',
    containerClassName = '',
    pageSize = ITEMS_PER_PAGE
}) => {
    const [sortBy, setSortBy] = useState('relevance');
    const [minPrice, setMinPrice] = useState('');
    const [maxPrice, setMaxPrice] = useState('');
    const [inStockOnly, setInStockOnly] = useState(false);
    const [currentPage, setCurrentPage] = useState(1);

    const debouncedMin = useDebouncedValue(minPrice, PRICE_DEBOUNCE_MS);
    const debouncedMax = useDebouncedValue(maxPrice, PRICE_DEBOUNCE_MS);

    // Пока диапазон некорректен (от > до), верхнюю границу не отправляем — иначе сервер вернёт 400
    const debouncedRangeInvalid = debouncedMin !== '' && debouncedMax !== ''
        && Number(debouncedMin) > Number(debouncedMax);

    const { products, totalPages, totalCount, loading, error, reload } = useCategoryProducts(categoryName, {
        sort: sortBy,
        minPrice: debouncedMin,
        maxPrice: debouncedRangeInvalid ? '' : debouncedMax,
        inStockOnly,
        page: currentPage,
        pageSize
    });

    const { getProductImage, getBrandName } = useProductHelpers();
    const { addToCart, notification } = useAddToCart();

    // Любое изменение фильтров — с первой страницы
    useEffect(() => {
        setCurrentPage(1);
    }, [sortBy, debouncedMin, debouncedMax, inStockOnly]);

    const handlePriceChange = (e) => {
        const { name, value } = e.target;
        if (name === 'minPrice') setMinPrice(value);
        if (name === 'maxPrice') setMaxPrice(value);
    };

    const handleClearFilters = () => {
        setSortBy('relevance');
        setMinPrice('');
        setMaxPrice('');
        setInStockOnly(false);
    };

    const handlePageChange = useCallback((page) => {
        setCurrentPage(page);
        if (typeof window !== 'undefined' && typeof window.scrollTo === 'function') {
            try { window.scrollTo({ top: 0, behavior: 'smooth' }); } catch { /* jsdom */ }
        }
    }, []);

    const priceRangeInvalid = minPrice !== '' && maxPrice !== '' && Number(minPrice) > Number(maxPrice);
    const hasActiveFilters = Boolean(minPrice || maxPrice || inStockOnly || sortBy !== 'relevance');

    const filters = (
        <>
            <div className="filter-section">
                <h3 className="filter-title">Цена</h3>
                <div className="price-range">
                    <input type="number" min="0" name="minPrice" className="price-input" placeholder="От"
                        aria-label="Цена от" value={minPrice} onChange={handlePriceChange} />
                    <span className="price-separator">—</span>
                    <input type="number" min="0" name="maxPrice" className="price-input" placeholder="До"
                        aria-label="Цена до" value={maxPrice} onChange={handlePriceChange} />
                </div>
                {priceRangeInvalid && (
                    <p className="filter-hint filter-hint--error">Минимальная цена больше максимальной</p>
                )}
            </div>

            <div className="filter-section">
                <h3 className="filter-title">Сортировка</h3>
                <select className="sort-select" aria-label="Сортировка" value={sortBy}
                    onChange={(e) => setSortBy(e.target.value)}>
                    {SORT_OPTIONS.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
                </select>
            </div>

            <div className="filter-section">
                <label className="stock-filter">
                    <input
                        type="checkbox"
                        checked={inStockOnly}
                        onChange={(e) => setInStockOnly(e.target.checked)}
                    />
                    Только в наличии
                </label>
            </div>

            {hasActiveFilters && (
                <button className="clear-filters-btn" onClick={handleClearFilters}>
                    Сбросить все фильтры
                </button>
            )}
        </>
    );

    return (
        <div className={pageClassName}>
            <Notification
                show={notification.show}
                productName={notification.productName}
                message={notification.message}
                type={notification.type}
            />
            <Header navigate={navigate} />

            <main className="main-content">
                <div className={`container ${containerClassName}`.trim()}>
                    <BackButton onClick={() => navigate(-1)} />

                    <CatalogLayout
                        title={title}
                        filters={filters}
                        pagination={
                            <Pagination
                                currentPage={currentPage}
                                totalPages={totalPages}
                                onPageChange={handlePageChange}
                            />
                        }
                    >
                        {!loading && !error && totalCount > 0 && (
                            <p className="catalog-results-count">Найдено товаров: {totalCount}</p>
                        )}

                        {loading && <LoadingSpinner text="Загрузка..." />}

                        {error && !loading && (
                            <div className="error-message">
                                <p>{error}</p>
                                <button onClick={reload} className="retry-btn">Повторить</button>
                            </div>
                        )}

                        {!loading && !error && products.length === 0 && (
                            <div className="no-products">
                                <p>Товары не найдены</p>
                            </div>
                        )}

                        {!loading && !error && products.length > 0 && (
                            <div className="products-grid">
                                {products.map((product, index) => {
                                    const productId = pick(product, 'Id', 'id');
                                    return (
                                        <ProductCard
                                            key={productId ?? index}
                                            product={product}
                                            onClick={(id) => navigate(`/product/${id}`)}
                                            onAddToCart={addToCart}
                                            getBrandName={(p) => getBrandName(p, brandHints)}
                                            getProductImage={getProductImage}
                                            getProductType={(name) => detectProductType(name, typeRules)}
                                            placeholderText={placeholderText}
                                            actions={<WishlistButton productId={productId} />}
                                        />
                                    );
                                })}
                            </div>
                        )}
                    </CatalogLayout>
                </div>
            </main>
        </div>
    );
};

export default CategoryCatalog;
