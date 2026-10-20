import React, { useState, useMemo, useEffect } from 'react';
import './Laptop.css';
import { withNavigate } from 'shared/lib/hoc/withNavigate';
import Header from 'widgets/header/ui/Header';
import Notification from 'widgets/notification/ui/Notification';
import ProductCard from 'entities/product/ui/ProductCard';
import LoadingSpinner from 'shared/ui/LoadingSpinner/LoadingSpinner';
import BackButton from 'shared/ui/BackButton/BackButton';
import CatalogLayout from 'shared/ui/CatalogLayout/CatalogLayout';
import Pagination from 'shared/ui/Pagination/Pagination';
import { useFetchProductsByCategory } from 'entities/product/model/useFetchProductsByCategory';
import { useProductHelpers } from 'entities/product/lib/useProductHelpers';
import { useAddToCart } from 'features/add-to-cart/model/useAddToCart';

const LAPTOP_BRANDS = {
    'apple': 'Apple', 'macbook': 'Apple',
    'asus': 'ASUS', 'lenovo': 'Lenovo',
    'xiaomi': 'Xiaomi', 'redmibook': 'Xiaomi'
};

const ITEMS_PER_PAGE = 6;

const pick = (obj, ...keys) => {
    for (const key of keys) {
        if (obj[key] !== undefined && obj[key] !== null) return obj[key];
    }
    return undefined;
};

const Laptop = ({ navigate }) => {
    const { products, loading, error, fetchProducts, handleClearCache } = useFetchProductsByCategory(
        'Ноутбуки',
        'laptops_cache',
        'laptops_etag'
    );
    const { getProductImage, getBrandName } = useProductHelpers();
    const { addToCart, notification } = useAddToCart();

    const [sortBy, setSortBy] = useState('relevance');
    const [minPrice, setMinPrice] = useState('');
    const [maxPrice, setMaxPrice] = useState('');
    const [currentPage, setCurrentPage] = useState(1);

    useEffect(() => {
        setCurrentPage(1);
    }, [sortBy, minPrice, maxPrice, products.length]);

    const getLaptopType = (name) => {
        if (!name) return 'standard';
        const n = name.toLowerCase();
        if (n.includes('macbook') || n.includes('apple')) return 'macbook';
        if (n.includes('gaming') || n.includes('tuf')) return 'gaming';
        if (n.includes('vivobook')) return 'vivobook';
        if (n.includes('ideapad')) return 'ideapad';
        if (n.includes('redmibook')) return 'xiaomi';
        return 'standard';
    };

    const filteredProducts = useMemo(() => {
        let filtered = [...products];

        if (minPrice) {
            filtered = filtered.filter(p => (pick(p, 'Price', 'price') || 0) >= Number(minPrice));
        }
        if (maxPrice) {
            filtered = filtered.filter(p => (pick(p, 'Price', 'price') || 0) <= Number(maxPrice));
        }

        if (sortBy === 'price_asc') {
            filtered.sort((a, b) => (pick(a, 'Price', 'price') || 0) - (pick(b, 'Price', 'price') || 0));
        } else if (sortBy === 'price_desc') {
            filtered.sort((a, b) => (pick(b, 'Price', 'price') || 0) - (pick(a, 'Price', 'price') || 0));
        } else if (sortBy === 'name_asc') {
            filtered.sort((a, b) => {
                const na = pick(a, 'Name', 'name') || '';
                const nb = pick(b, 'Name', 'name') || '';
                return na.localeCompare(nb);
            });
        }

        return filtered;
    }, [products, minPrice, maxPrice, sortBy]);

    const totalPages = Math.ceil(filteredProducts.length / ITEMS_PER_PAGE);
    const paginatedProducts = filteredProducts.slice(
        (currentPage - 1) * ITEMS_PER_PAGE,
        currentPage * ITEMS_PER_PAGE
    );

    const handlePriceChange = (e) => {
        const { name, value } = e.target;
        if (name === 'minPrice') setMinPrice(value);
        if (name === 'maxPrice') setMaxPrice(value);
    };

    const handleClearFilters = () => {
        setSortBy('relevance');
        setMinPrice('');
        setMaxPrice('');
    };

    const hasActiveFilters = minPrice || maxPrice || sortBy !== 'relevance';

    const filters = (
        <>
            <div className="filter-section">
                <h3 className="filter-title">Цена</h3>
                <div className="price-range">
                    <input
                        type="number"
                        name="minPrice"
                        className="price-input"
                        placeholder="От"
                        value={minPrice}
                        onChange={handlePriceChange}
                    />
                    <span className="price-separator">—</span>
                    <input
                        type="number"
                        name="maxPrice"
                        className="price-input"
                        placeholder="До"
                        value={maxPrice}
                        onChange={handlePriceChange}
                    />
                </div>
            </div>

            <div className="filter-section">
                <h3 className="filter-title">Сортировка</h3>
                <select className="sort-select" value={sortBy} onChange={(e) => setSortBy(e.target.value)}>
                    <option value="relevance">По релевантности</option>
                    <option value="price_asc">Сначала дешевые</option>
                    <option value="price_desc">Сначала дорогие</option>
                    <option value="name_asc">По названию (А-Я)</option>
                </select>
            </div>

            {hasActiveFilters && (
                <button className="clear-filters-btn" onClick={handleClearFilters}>
                    Сбросить все фильтры
                </button>
            )}
        </>
    );

    return (
        <div className="laptop-page">
            <Notification show={notification.show} productName={notification.productName} />
            <Header navigate={navigate} />

            <main className="main-content">
                <div className="container laptop-container">
                    <BackButton onClick={() => navigate(-1)} />

                    <CatalogLayout
                        title="Ноутбуки"
                        filters={filters}
                        pagination={
                            <Pagination
                                currentPage={currentPage}
                                totalPages={totalPages}
                                onPageChange={setCurrentPage}
                            />
                        }
                    >
                        {loading && <LoadingSpinner text="Загрузка..." />}

                        {error && !loading && (
                            <div className="error-message">
                                <p>{error}</p>
                                <button onClick={fetchProducts} className="retry-btn">Повторить</button>
                                {handleClearCache && (
                                    <button onClick={handleClearCache} className="retry-btn" style={{ marginLeft: 10 }}>
                                        Очистить кэш
                                    </button>
                                )}
                            </div>
                        )}

                        {!loading && !error && paginatedProducts.length === 0 && (
                            <div className="no-products">
                                <p>Товары не найдены</p>
                            </div>
                        )}

                        {!loading && !error && paginatedProducts.length > 0 && (
                            <div className="products-grid">
                                {paginatedProducts.map((product, index) => (
                                    <ProductCard
                                        key={pick(product, 'Id', 'id') ?? index}
                                        product={product}
                                        onClick={(id) => navigate(`/product/${id}`)}
                                        onAddToCart={addToCart}
                                        getBrandName={(p) => getBrandName(p, LAPTOP_BRANDS)}
                                        getProductImage={getProductImage}
                                        getProductType={(p) => getLaptopType(pick(p, 'Name', 'name'))}
                                        placeholderText="Ноутбук"
                                    />
                                ))}
                            </div>
                        )}
                    </CatalogLayout>
                </div>
            </main>
        </div>
    );
};

export default withNavigate(Laptop);